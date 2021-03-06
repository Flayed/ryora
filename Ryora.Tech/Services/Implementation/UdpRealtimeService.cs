﻿using Ryora.Tech.Models;
using Ryora.Udp;
using Ryora.Udp.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Ryora.Tech.Services.Implementation
{
    public class UdpRealtimeService : IRealtimeService
    {
        private IPEndPoint ServerEndPoint { get; } = new IPEndPoint(IPAddress.Parse("40.122.170.146"), 27816);

        private const int MissedFragmentThreshold = 10000;

        private static short? _connectionId = null;
        private static short ConnectionId
        {
            get
            {
                if (_connectionId == null)
                {
                    _connectionId = MessageConverter.ReadShort(Guid.NewGuid().ToByteArray(), 0);
                }
                return _connectionId.Value;
            }
        }
        private UdpClient Client { get; } = new UdpClient();
        private bool IsConnected { get; set; } = false;
        private List<ImageFragment> ImageFragments { get; set; } = new List<ImageFragment>();
        private int CurrentFrame { get; set; }

        private short _messageId = short.MinValue;
        private short MessageId
        {
            get
            {
                _messageId++;
                if (_messageId == short.MaxValue)
                    _messageId = short.MinValue;
                return _messageId;
            }
        }

        public event EventHandler MouseMove;
        public event EventHandler<bool> Sharing;
        public event EventHandler ClientResolutionChanged;
        public event EventHandler<bool> Disconnect;

        public UdpRealtimeService(short channel)
        {
            var missedFragmentTimer = new System.Timers.Timer(1000);
            missedFragmentTimer.Elapsed += (s, e) =>
            {
                var missedFragments =
                    ImageFragments.Where(f => f.Duration.ElapsedMilliseconds > MissedFragmentThreshold);
                foreach (var missedFragment in missedFragments)
                {
                    //var message = Messaging.CreateMessage(MessageType.Data, ConnectionId, channel, MessageId, 0, $"MissedFragment^{missedFragment}");
                    //await Client.SendAsync(message, message.Length, ServerEndPoint);
                    ImageFragments.Remove(missedFragment);
                }
            };
        }

        public async Task StartConnection(short channel, int screenWidth, int screenHeight)
        {
            var connectMessage = Messaging.CreateMessage(new ConnectMessage(screenWidth, screenHeight), ConnectionId, channel, MessageId);
            await Client.SendAsync(connectMessage, connectMessage.Length, ServerEndPoint);
            await Task.Run(async () =>
            {
                await ListenAsync(channel);
            });
        }

        public async Task SendMouseCoords(short channel, int x, int y, int wheelDelta, int screenWidth, int screenHeight, bool leftButton, bool middleButton, bool rightButton, bool firstExtendedButton, bool secondExtendedButton)
        {
            if (!IsConnected) return;
            var message = Messaging.CreateMessage(new MouseMessage(x, y, wheelDelta, screenWidth, screenHeight, leftButton, middleButton, rightButton, firstExtendedButton, secondExtendedButton), ConnectionId, channel, MessageId);
            await Client.SendAsync(message, message.Length, ServerEndPoint);
        }

        public async Task SendKeyboardInput(short channel, bool isDown, params short[] scanCodes)
        {
            if (!IsConnected || scanCodes.Length == 0) return;
            var message = Messaging.CreateMessage(new KeyboardMessage(isDown, scanCodes), ConnectionId, channel, MessageId);
            await Client.SendAsync(message, message.Length, ServerEndPoint);
        }

        private async Task ListenAsync(short channel)
        {
            bool listening = true;
            while (listening)
            {
                var result = await Client.ReceiveAsync();
                var message = Messaging.ReceiveMessage(result.Buffer);
                switch (message.Type)
                {
                    case MessageType.Disconnect:
                    case MessageType.Terminate:
                        await EndConnection(channel, true);
                        listening = false;
                        break;
                    case MessageType.Acknowledge:
                        var acknowledgeMessage = new AcknowledgeMessage(message.Payload);
                        ClientResolutionChanged?.Invoke(this,
                            new ClientResolutionChangedEventArgs(acknowledgeMessage.ScreenWidth, acknowledgeMessage.ScreenHeight));
                        IsConnected = true;
                        break;
                    case MessageType.Sharing:
                        var sharingMessage = new SharingMessage(message.Payload);
                        Sharing?.Invoke(this, sharingMessage.IsSharing);
                        break;
                    case MessageType.Image:
                        var imageMessage = new ImageMessage(message.Payload);
                        try
                        {
                            var imageFragment = ImageFragments.FirstOrDefault(mf => mf.MessageId.Equals(message.MessageId));
                            if (imageFragment == null)
                            {
                                imageFragment = new ImageFragment(message.MessageId, imageMessage);
                                ImageFragments.Add(imageFragment);
                            }
                            else
                                imageFragment.AddFragment(imageMessage);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Something bad happened: {ex.Message}");
                        }
                        break;
                    case MessageType.MouseMessage:
                        var mouseMessage = new MouseMessage(message.Payload);
                        MouseMove?.Invoke(this,
                            new MouseMoveEventArgs(mouseMessage.X, mouseMessage.Y, mouseMessage.ScreenWidth,
                                mouseMessage.ScreenHeight));
                        break;
                }
            }
        }

        public async Task EndConnection(short channel, bool reconnect = false)
        {
            var disconnectMessage = Messaging.CreateMessage((reconnect ? MessageType.Disconnect : MessageType.Terminate), ConnectionId, channel, MessageId);
            await Client.SendAsync(disconnectMessage, disconnectMessage.Length, ServerEndPoint);
            IsConnected = false;
            await Task.Delay(100);
            Disconnect?.Invoke(this, reconnect);
        }

        public IEnumerable<ImageFragment> CompletedImages
        {
            get
            {
                var completedImages = ImageFragments.Where(f => f.IsComplete).ToArray();
                ImageFragments = ImageFragments.Except(completedImages).ToList();
                return completedImages;
            }
        }

        public string Transport => "UDP Datagrams!";
    }

    public class ImageFragment
    {
        public short MessageId { get; set; }
        public int Length { get; set; }
        public Rectangle ImageLocation { get; set; }
        public Stopwatch Duration { get; } = new Stopwatch();
        private List<ImageMessage> Fragments { get; set; } = new List<ImageMessage>();

        public ImageFragment(short messageId, ImageMessage imageMessage)
        {
            MessageId = messageId;
            Length = imageMessage.ImageLength;
            Fragments.Add(imageMessage);
            ImageLocation = imageMessage.Location;
            Duration.Start();
        }

        public void AddFragment(ImageMessage imageMessage)
        {
            Fragments.Add(imageMessage);
        }

        public bool IsComplete => Fragments.Sum(f => f.ImageData.Length) == Length;

        public byte[] Image => Fragments.OrderBy(f => f.Sequence).SelectMany(f => f.ImageData).ToArray();
    }
}
