using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ryora.Tech.Models;
using Ryora.Udp;

namespace Ryora.Tech.Services.Implementation
{
    public class UdpRealtimeService : IRealtimeService
    {
        private IPEndPoint ServerEndPoint { get; } = new IPEndPoint(IPAddress.Parse("40.122.170.146"), 27816);

        private const int MissedFragmentThreshold = 10000;

        private short ConnectionId { get; set; }
        private UdpClient Client { get; } = new UdpClient();
        private bool IsConnected { get; set; } = false;
        private List<ImageFragment> MessageFragments { get; set; } = new List<ImageFragment>();
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

        public event EventHandler NewImage;
        public event EventHandler NewImageFragment;
        public event EventHandler MouseMove;
        public event EventHandler Sharing;

        public UdpRealtimeService(short channel)
        {
            ConnectionId = 2; // (short)(new Random().Next(short.MinValue, short.MaxValue));
            var missedFragmentTimer = new System.Timers.Timer(5000);
            missedFragmentTimer.Elapsed += async (s, e) =>
            {
                var missedFragments =
                    MessageFragments.Where(f => f.Duration.ElapsedMilliseconds > MissedFragmentThreshold);
                foreach (var missedFragment in missedFragments)
                {
                    var message = Messaging.CreateMessage(MessageType.Data, ConnectionId, channel, MessageId, 0, $"MissedFragment^{missedFragment}");
                    await Client.SendAsync(message, message.Length, ServerEndPoint);
                    MessageFragments.Remove(missedFragment);
                }                
            };
        }

        public async Task StartConnection(short channel)
        {
            var connectMessage = Messaging.CreateMessage(MessageType.Connect, ConnectionId, channel, MessageId, 0, "Initiating Connection");
            await Client.SendAsync(connectMessage, connectMessage.Length, ServerEndPoint);
            await Task.Run(async () =>
            {
                while (true)
                {
                    var result = await Client.ReceiveAsync();
                    var message = Messaging.ReceiveMessage(result.Buffer);
                    switch (message.Type)
                    {
                        case MessageType.Acknowledge:
                            Console.WriteLine("Connected and good to go!");
                            IsConnected = true;
                            break;
                        case MessageType.Data:
                            var parts = message.Message.Split('^');
                            switch (parts[0])
                            {
                                case "NewImage":
                                    if (NewImage == null || parts.Length != 3) continue;
                                    CurrentFrame = int.Parse(parts[1]);
                                    MessageFragments.Add(new ImageFragment(CurrentFrame, message.MessageId, int.Parse(parts[2])));
                                    break;
                                case "NewPartialImage":
                                    if (NewImageFragment == null || parts.Length != 7) continue;
                                    CurrentFrame = int.Parse(parts[1]);
                                    MessageFragments.Add(new ImageFragment(CurrentFrame, message.MessageId, int.Parse(parts[2]),
                                        new Rectangle(
                                            int.Parse(parts[3]),
                                            int.Parse(parts[4]),
                                            int.Parse(parts[5]),
                                            int.Parse(parts[6]))));
                                    break;
                                case "MouseMove":
                                    if (MouseMove == null || parts.Length != 3) continue;
                                    MouseMove(this, new MouseMoveEventArgs(double.Parse(parts[1]), double.Parse(parts[2])));
                                    break;
                                case "Share":
                                    if (Sharing == null || parts.Length != 2) continue;
                                    Sharing(this, new SharingEventArgs(bool.Parse(parts[1])));
                                    break;
                            }
                            break;
                        case MessageType.DataFragment:
                        case MessageType.LastDataFragment:
                            try
                            {
                                var imageFragment = MessageFragments.FirstOrDefault(mf => mf.MessageId.Equals(message.MessageId));
                                if (imageFragment == null) continue;
                                imageFragment.Fragments.Add(message);

                                if (imageFragment.Length == imageFragment.Fragments.Sum(f => f.Payload.Length))
                                {
                                    var fragments = imageFragment.Fragments.OrderBy(f => f.Sequence);
                                    var buffer = new byte[imageFragment.Length];
                                    var offset = 0;
                                    foreach (var fragment in fragments)
                                    {
                                        Buffer.BlockCopy(fragment.Payload, 0, buffer, offset, fragment.Payload.Length);
                                        offset += fragment.Payload.Length;
                                    }

                                    if (imageFragment.ImagePosition.HasValue)
                                        NewImageFragment?.Invoke(this, new NewImageFragmentEventArgs(imageFragment.Frame, buffer, imageFragment.ImagePosition.Value));
                                    else
                                        NewImage?.Invoke(this, new NewImageEventArgs(imageFragment.Frame, buffer));

                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Something bad happened: {ex.Message}");
                            }
                            break;
                    }
                }
            });
        }

        public string Transport => "UDP Datagrams!";
    }

    public class ImageFragment
    {
        public Rectangle? ImagePosition { get; set; }
        public short MessageId { get; set; }
        public int Frame { get; set; }
        public int Length { get; set; }
        public Stopwatch Duration { get; } = new Stopwatch();
        public List<UdpMessage> Fragments { get; set; } = new List<UdpMessage>();

        public ImageFragment(int frame, short messageId, int length, Rectangle? imagePosition = null)
        {
            Frame = frame;
            MessageId = messageId;
            Length = length;
            ImagePosition = imagePosition;
            Duration.Start();
        }

        public override string ToString()
        {
            return ImagePosition.HasValue ? $"{ImagePosition.Value.X}^{ImagePosition.Value.Y}^{ImagePosition.Value.Width}^{ImagePosition.Value.Height}" : $"";
        }
    }
}
