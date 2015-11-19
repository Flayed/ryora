using System.Diagnostics;
using System.Net;

namespace Ryora.UdpServer.Models
{
    public class Connection
    {
        public short Id { get; set; }
        public short Channel { get; set; }
        public short ScreenWidth { get; set; }
        public short ScreenHeight { get; set; }
        public Stopwatch Duration { get; set; } = new Stopwatch();
        public IPEndPoint IpEndPoint { get; set; }

        public Connection(IPEndPoint endpoint, short id, short channel, short screenWidth, short screenHeight)
        {
            IpEndPoint = endpoint;
            Id = id;
            Channel = channel;
            ScreenWidth = screenWidth;
            ScreenHeight = screenHeight;
            Duration.Start();
        }
    }
}
