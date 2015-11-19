using Ryora.Udp;
using Ryora.Udp.Messages;
using Ryora.UdpServer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Ryora.UdpServer
{
    public static class UdpService
    {
        private static readonly int Port = 27816;
        private static UdpClient _client = null;
        public static UdpClient Client => _client ?? (_client = new UdpClient(Port));
        public static short ServerId = 42;
        private static readonly List<ConnectionRequest> ConnectionRequests = new List<ConnectionRequest>();
        private static Stopwatch Sw = new Stopwatch();

        private static int ConsoleLine { get; set; } = 1;
        private static int MessagesReceived { get; set; } = 0;
        private static double BytesUsed { get; set; } = 0;
        private static double KilobytesUsed => Math.Round((double)BytesUsed / 1000, 2);
        private static double MegabytesUsed => Math.Round((double)BytesUsed / 1000000, 2);

        private static double KilobytesPerSecond
        {
            get
            {
                if (Sw.IsRunning)
                    return Math.Round(KilobytesUsed / Sw.Elapsed.TotalSeconds, 2);
                return 0;
            }
        }

        private static double AverageMessageSize => Math.Round((KilobytesUsed / MessagesReceived), 2);

        public static async Task Listen()
        {
            try
            {
                var request = await Client.ReceiveAsync();
                var requestMessage = Messaging.ReceiveMessage(request.Buffer);
                MessagesReceived++;
                BytesUsed += request.Buffer.Length;
                Terminal.Log($"Bytes Used: {BytesUsed}B {KilobytesUsed}KB {MegabytesUsed}MB  ", 0, true);
                Terminal.Log($"{KilobytesPerSecond} KB/s  ", 0, ConsoleColor.Cyan);
                Terminal.Log($"Recv: {MessagesReceived}  ", 0, ConsoleColor.Green);
                Terminal.Log($"Avg: {AverageMessageSize}KB        ", 0, ConsoleColor.Magenta);

                switch (requestMessage.Type)
                {
                    case MessageType.Connect:
                        var connectionMessage = new ConnectMessage(requestMessage.Payload);
                        var connectionRequest = new ConnectionRequest(request.RemoteEndPoint, requestMessage.ConnectionId,
                            requestMessage.Channel, connectionMessage.ScreenWidth, connectionMessage.ScreenHeight);
                        if (ConnectionRequests.Any(cr => cr.Id.Equals(connectionRequest.Id)))
                        {
                            ConnectionRequests.Remove(ConnectionRequests.First(cr => cr.Id.Equals(connectionRequest.Id)));
                        }
                        ConnectionRequests.Add(connectionRequest);
                        var channelConnections =
                            ConnectionRequests.Where(cr => cr.Channel.Equals(connectionRequest.Channel)).ToArray();
                        Terminal.LogLine($"Channel {connectionRequest.Channel} has {channelConnections.Count()} connections.", ConsoleColor.DarkGray);
                        if (channelConnections.Count() == 2)
                        {
                            await SendAcknowledgement(channelConnections.ElementAt(0), channelConnections.ElementAt(1));
                        }
                        break;
                    case MessageType.Disconnect:
                        ConnectionRequests.RemoveAll(cr => cr.Id.Equals(requestMessage.ConnectionId));
                        Terminal.LogLine($"Client {requestMessage.ConnectionId} disconnected from channel {requestMessage.Channel}", ConsoleColor.Yellow);
                        await SendMessage(requestMessage);
                        break;
                    default:
                        if (!Sw.IsRunning) Sw.Start();
                        await SendMessage(requestMessage);
                        break;
                }
            }
            catch (Exception ex)
            {
                Terminal.LogLine($"Something bad happend: {ex.Message} {ex.InnerException?.Message}", ConsoleColor.Red);
            }
        }

        private static async Task SendMessage(UdpMessage requestMessage)
        {
            var destination =
                ConnectionRequests.FirstOrDefault(
                    cr => cr.Channel.Equals(requestMessage.Channel) && !cr.Id.Equals(requestMessage.ConnectionId));
            if (destination == null) return;
            await Client.SendAsync(requestMessage.Bytes, requestMessage.Bytes.Length, destination.IpEndPoint);
        }

        internal static async Task SendAcknowledgement(ConnectionRequest firstRequest, ConnectionRequest secondRequest)
        {
            var message = Messaging.CreateMessage(new AcknowledgeMessage(secondRequest.ScreenWidth, secondRequest.ScreenHeight), ServerId, firstRequest.Channel, 0);
            await Client.SendAsync(message, message.Length, firstRequest.IpEndPoint);
            message = Messaging.CreateMessage(new AcknowledgeMessage(firstRequest.ScreenWidth, firstRequest.ScreenHeight), ServerId, secondRequest.Channel, 0);
            await Client.SendAsync(message, message.Length, secondRequest.IpEndPoint);
            Terminal.LogLine($"  Sent Connection Acknowledgement to channel {firstRequest.Channel} {firstRequest.Id}({firstRequest.IpEndPoint}) and {secondRequest.Id}({secondRequest.IpEndPoint})", ConsoleColor.DarkGreen);
        }
    }
}
