using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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

        private short ConnectionId { get; set; }
        private UdpClient Client { get; } = new UdpClient();
        private bool IsConnected { get; set; } = false;
        private byte[] ImageFragment { get; set; }
        private int ImageFragmentOffset { get; set; }
        private int CurrentFrame { get; set; }

        public event EventHandler NewImage;
        public event EventHandler MouseMove;
        public event EventHandler Sharing;

        public UdpRealtimeService()
        {
            ConnectionId = 2; // (short)(new Random().Next(short.MinValue, short.MaxValue));
        }

        public async Task StartConnection(short channel)
        {
            var connectMessage = Messaging.CreateMessage(MessageType.Connect, ConnectionId, channel, "Initiating Connection");
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
                                    ImageFragment = new byte[int.Parse(parts[2])];
                                    ImageFragmentOffset = 0;
                                    CurrentFrame = int.Parse(parts[1]);                                    
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
                            Buffer.BlockCopy(message.Payload, 0, ImageFragment, ImageFragmentOffset, message.Payload.Length);
                            ImageFragmentOffset += message.Payload.Length;
                            break;
                        case MessageType.LastDataFragment:
                            Buffer.BlockCopy(message.Payload, 0, ImageFragment, ImageFragmentOffset, message.Payload.Length);
                            ImageFragmentOffset += message.Payload.Length;
                            NewImage(this, new NewImageEventArgs(CurrentFrame, ImageFragment));
                            break;
                    }
                }
            });
        }

        public string Transport => "UDP Datagrams!";
    }
}
