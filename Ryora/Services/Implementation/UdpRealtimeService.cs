using Ryora.Udp;
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

        public async Task StartConnection(short channel)
        {
            var connectMessage = Messaging.CreateMessage(MessageType.Connect, ConnectionId, channel, MessageId, 0, "Initiating Connection");
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
                        case MessageType.Data:
                            var parts = message.Message.Split('^');
                            switch (parts[0])
                            {
                                case "MissedFragment":
                                    var missedRect = new Rectangle(
                                        int.Parse(parts[1]),
                                        int.Parse(parts[2]),
                                        int.Parse(parts[3]),
                                        int.Parse(parts[4]));
                                    MissedFragmentEvent?.Invoke(this, missedRect);
                                    break;
                            }
                            break;
                    }
                }
            });
        }

        public async Task SendImage(short channel, int frame, byte[] image)
        {
            var mId = MessageId;
            var message = Messaging.CreateMessage(MessageType.Data, ConnectionId, channel, mId, 0, $"NewImage^{frame}^{image.Length}");
            await Client.SendAsync(message, message.Length, ServerEndPoint);
            await SendImageFragments(channel, mId, image);
        }

        public Task SendImage(short channel, int frame, Rectangle location, byte[] image)
        {
            return SendImage(channel, frame, location.X, location.Y, location.Width, location.Height, image);
        }

        public async Task SendImage(short channel, int frame, int x, int y, int width, int height, byte[] image)
        {
            var mId = MessageId;
            var message = Messaging.CreateMessage(MessageType.Data, ConnectionId, channel, mId, 0, $"NewPartialImage^{frame}^{image.Length}^{x}^{y}^{width}^{height}");
            await Client.SendAsync(message, message.Length, ServerEndPoint);
            await SendImageFragments(channel, mId, image);
        }

        private async Task SendImageFragments(short channel, short messageId, byte[] image)
        {
            short sequence = 0;
            var offset = 0;
            while (offset < image.Length)
            {
                var length = (offset + 1000 < image.Length ? 1000 : image.Length - offset);
                var buf = new byte[length];
                Array.Copy(image, offset, buf, 0, length);
                offset += length;

                var frag = Messaging.CreateMessage(
                    (offset >= image.Length ? MessageType.LastDataFragment : MessageType.DataFragment), ConnectionId,
                    channel, messageId, sequence++, buf);

                await Client.SendAsync(frag, frag.Length, ServerEndPoint);
            }
        }

        public async Task SendMouseCoords(short channel, double x, double y)
        {
            await Task.Delay(1);
            var message = Messaging.CreateMessage(MessageType.Data, ConnectionId, channel, MessageId, 0, $"MouseMove^{x}^{y}");
            await Client.SendAsync(message, message.Length, ServerEndPoint);
        }

        public async Task Sharing(short channel, bool isSharing)
        {
            var message = Messaging.CreateMessage(MessageType.Data, ConnectionId, channel, MessageId, 0, $"Sharing^{isSharing}");
            await Client.SendAsync(message, message.Length, ServerEndPoint);
        }

        public event EventHandler<Rectangle> MissedFragmentEvent;
    }
}
