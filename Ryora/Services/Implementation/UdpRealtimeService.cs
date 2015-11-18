﻿using Ryora.Udp;
using Ryora.Udp.Messages;
using System;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Ryora.Client.Services.Implementation
{
    public class UdpRealtimeService : IRealtimeService
    {
        private IPEndPoint ServerEndPoint = new IPEndPoint(IPAddress.Parse("40.122.170.146"), 27816);
        private short ConnectionId { get; set; }
        private UdpClient Client = new UdpClient();
        private bool IsConnected { get; set; } = false;
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

        public UdpRealtimeService()
        {
            //ConnectionId = (short) (new Random().Next(short.MinValue, short.MaxValue));
            ConnectionId = 1;
        }

        public async Task StartConnection(short channel, int screenWidth, int screenHeight)
        {
            var connectMessage = Messaging.CreateMessage(new ConnectMessage(screenWidth, screenHeight), ConnectionId, channel, MessageId);
            await Client.SendAsync(connectMessage, connectMessage.Length, ServerEndPoint);
            await Task.Run(async () =>
            {
                while (!IsConnected)
                {
                    var result = await Client.ReceiveAsync();
                    var message = Messaging.ReceiveMessage(result.Buffer);
                    switch (message.Type)
                    {
                        case MessageType.Acknowledge:
                            Console.WriteLine("Connected and good to go!");
                            IsConnected = true;
                            break;
                    }
                }
            });
        }

        public async Task EndConnection(short channel)
        {
            var disconnectMessage = Messaging.CreateMessage(MessageType.Disconnect, ConnectionId, channel, MessageId);
            await Client.SendAsync(disconnectMessage, disconnectMessage.Length, ServerEndPoint);
        }

        public Task SendImage(short channel, Rectangle rect, byte[] image)
        {
            return SendImage(channel, rect.X, rect.Y, rect.Width, rect.Height, image);
        }

        public async Task SendImage(short channel, int x, int y, int width, int height, byte[] image)
        {
            var messageId = MessageId;
            short sequence = 0;
            var offset = 0;
            int imageLength = image.Length;
            while (offset < image.Length)
            {
                var length = (offset + 1000 < image.Length ? 1000 : image.Length - offset);
                var buf = new byte[length];
                Array.Copy(image, offset, buf, 0, length);
                offset += length;

                var message = Messaging.CreateMessage(new ImageMessage(x, y, width, height, imageLength, sequence++, buf),
                    ConnectionId, channel, messageId);

                await Client.SendAsync(message, message.Length, ServerEndPoint);
            }
        }

        public async Task SendMouseCoords(short channel, int x, int y, int screenWidth, int screenHeight)
        {
            var message = Messaging.CreateMessage(new MouseMessage(x, y, screenWidth, screenHeight), ConnectionId, channel, MessageId);
            await Client.SendAsync(message, message.Length, ServerEndPoint);
        }

        public async Task Sharing(short channel, bool isSharing)
        {
            //var message = Messaging.CreateMessage(MessageType.Data, ConnectionId, channel, MessageId, 0, $"Sharing^{isSharing}");
            //await Client.SendAsync(message, message.Length, ServerEndPoint);
            await Task.Delay(1);
        }

        public event EventHandler<Rectangle> MissedFragmentEvent;
    }
}
