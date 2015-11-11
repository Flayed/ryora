using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ryora.Udp;

namespace Ryora.UdpServer.Models
{
    public class ConnectionRequest
    {
        public short Id { get; set; }
        public short Channel { get; set; }
        public IPEndPoint IpEndPoint { get; set; }

        public ConnectionRequest(IPEndPoint endpoint, short id, short channel)
        {
            IpEndPoint = endpoint;
            Id = id;
            Channel = channel;
        }
    }
}
