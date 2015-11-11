using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ryora.Udp;

namespace Ryora.Client.Services.Implementation
{
    public class UdpRealtimeService : IRealtimeService
    {
        private IPEndPoint ServerEndPoint = new IPEndPoint(IPAddress.Parse("40.122.170.146"), 27816);
        private short ConnectionId { get; set; }
        private UdpClient Client = new UdpClient();
        private bool IsConnected { get; set; } = false;

        public UdpRealtimeService()
        {
            //ConnectionId = (short) (new Random().Next(short.MinValue, short.MaxValue));
            ConnectionId = 1;
        }

        public async Task StartConnection(short channel)
        {
            var connectMessage = Messaging.CreateMessage(MessageType.Connect, ConnectionId, channel, "Initiating Connection");
            await Client.SendAsync(connectMessage, connectMessage.Length, ServerEndPoint);            
            await Task.Run(async () =>
            {
                while (!IsConnected)
                {
                    var result = await Client.ReceiveAsync();
                    var message = Messaging.ReceiveMessage(result.Buffer);
                    if (message.Type == MessageType.Acknowledge)
                    {
                        Console.WriteLine("Connected and good to go!");
                        IsConnected = true;
                    }
                }
            });
        }

        public async Task SendImage(short channel, int frame, byte[] image)
        {            
            var message = Messaging.CreateMessage(MessageType.Data, ConnectionId, channel, $"NewImage^{frame}^{image.Length}");
            await Client.SendAsync(message, message.Length, ServerEndPoint);
                  
            var offset = 0;
            while (offset < image.Length)
            {
                var length = (offset + 1000 < image.Length ? 1000 : image.Length - offset);
                var buf = new byte[length];
                Array.Copy(image, offset, buf, 0, length);
                offset += length;

                var frag = Messaging.CreateMessage(
                    (offset >= image.Length ? MessageType.LastDataFragment : MessageType.DataFragment), ConnectionId,
                    channel, buf);

                await Client.SendAsync(frag, frag.Length, ServerEndPoint);
            }
        }

        public async Task SendMouseCoords(short channel, double x, double y)
        {
            await Task.Delay(1);
            var message = Messaging.CreateMessage(MessageType.Data, ConnectionId, channel, $"MouseMove^{x}^{y}");
            await Client.SendAsync(message, message.Length, ServerEndPoint);
        }

        public async Task Sharing(short channel, bool isSharing)
        {
            var message = Messaging.CreateMessage(MessageType.Data, ConnectionId, channel, $"Sharing^{isSharing}");
            await Client.SendAsync(message, message.Length, ServerEndPoint);
        }
    }
}
