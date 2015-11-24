using Ryora.Udp;
using Ryora.Udp.Messages;
using Ryora.UdpServer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Timers;

namespace Ryora.UdpServer
{
    public class UdpService
    {
        private readonly int Port = 27816;
        private readonly short ServerId = 42;
        private readonly int KeepaliveTimeout = 10000;

        private UdpClient _client = null;
        public UdpClient Client => _client ?? (_client = new UdpClient(Port));

        private readonly List<Connection> Connections = new List<Connection>();
        private Stopwatch Elapsed { get; set; } = new Stopwatch();

        private int PreviousMessageIdReceived { get; set; } = 0;
        private int UniqueMessagesReceived { get; set; } = 0;
        private double BytesUsed { get; set; } = 0;
        private double KilobytesUsed => Math.Round((double)BytesUsed / 1000, 2);
        private double MegabytesUsed => Math.Round((double)BytesUsed / 1000000, 2);
        private double KilobytesPerSecond
        {
            get
            {
                if (Elapsed.IsRunning)
                    return Math.Round(KilobytesUsed / Elapsed.Elapsed.TotalSeconds, 2);
                return 0;
            }
        }
        private double AverageMessageSize => Math.Round((KilobytesUsed / UniqueMessagesReceived), 2);

        public UdpService()
        {
            SetUpKeepaliveTimer();
        }

        public async Task Listen()
        {
            try
            {
                var request = await Client.ReceiveAsync();
                var requestMessage = Messaging.ReceiveMessage(request.Buffer);
                if (requestMessage.MessageId != PreviousMessageIdReceived)
                {
                    UniqueMessagesReceived++;
                    PreviousMessageIdReceived = requestMessage.MessageId;
                }
                BytesUsed += request.Buffer.Length;
                Terminal.Log($"Bytes Used: {BytesUsed}B {KilobytesUsed}KB {MegabytesUsed}MB  ", 0, true);
                Terminal.Log($"{KilobytesPerSecond} KB/s  ", 0, ConsoleColor.Cyan);
                Terminal.Log($"Recv: {UniqueMessagesReceived}  ", 0, ConsoleColor.Green);
                Terminal.Log($"Avg: {AverageMessageSize}KB        ", 0, ConsoleColor.Magenta);

                switch (requestMessage.Type)
                {
                    case MessageType.Connect:
                        var connectionMessage = new ConnectMessage(requestMessage.Payload);
                        var connection = new Connection(request.RemoteEndPoint, requestMessage.ConnectionId,
                            requestMessage.Channel, connectionMessage.ScreenWidth, connectionMessage.ScreenHeight);
                        if (Connections.Any(cr => cr.Id.Equals(connection.Id)))
                        {
                            Connections.Remove(Connections.First(cr => cr.Id.Equals(connection.Id)));
                        }
                        Connections.Add(connection);
                        Terminal.LogLine($"Channel {connection.Channel} received connection from client {connection.Id} ({connection.IpEndPoint})", ConsoleColor.DarkGreen);
                        var channelConnections = Connections.Where(cr => cr.Channel.Equals(connection.Channel)).ToArray();
                        if (channelConnections.Count() == 2)
                        {
                            await SendAcknowledgement(channelConnections.ElementAt(0), channelConnections.ElementAt(1));
                        }
                        break;
                    case MessageType.Disconnect:
                        Connections.RemoveAll(cr => cr.Id.Equals(requestMessage.ConnectionId));
                        Terminal.LogLine($"Client {requestMessage.ConnectionId} disconnected from channel {requestMessage.Channel}", ConsoleColor.Yellow);
                        await SendMessage(requestMessage);
                        break;
                    case MessageType.Terminate:
                        Connections.RemoveAll(cr => cr.Id.Equals(requestMessage.ConnectionId));
                        Terminal.LogLine($"Client {requestMessage.ConnectionId} terminated connection from channel {requestMessage.Channel}", ConsoleColor.Magenta);
                        await SendMessage(requestMessage);
                        break;
                    default:
                        if (!Elapsed.IsRunning) Elapsed.Start();
                        await SendMessage(requestMessage);
                        break;
                }
            }
            catch (Exception ex)
            {
                Terminal.LogLine($"Something bad happend: {ex.Message} {ex.InnerException?.Message}", ConsoleColor.Red);
            }
        }

        private async Task SendMessage(UdpMessage requestMessage)
        {
            var destination =
                Connections.FirstOrDefault(
                    cr => cr.Channel.Equals(requestMessage.Channel) && !cr.Id.Equals(requestMessage.ConnectionId));
            if (destination == null) return;
            await Client.SendAsync(requestMessage.Bytes, requestMessage.Bytes.Length, destination.IpEndPoint);
            destination.Duration.Restart();
        }

        private async Task SendAcknowledgement(Connection first, Connection second)
        {
            var message = Messaging.CreateMessage(new AcknowledgeMessage(second.ScreenWidth, second.ScreenHeight), ServerId, first.Channel, 0);
            await Client.SendAsync(message, message.Length, first.IpEndPoint);
            message = Messaging.CreateMessage(new AcknowledgeMessage(first.ScreenWidth, first.ScreenHeight), ServerId, second.Channel, 0);
            await Client.SendAsync(message, message.Length, second.IpEndPoint);
            Terminal.LogLine($" Channel {first.Channel} has two clients: {first.Id}({first.IpEndPoint}) and {second.Id}({second.IpEndPoint})", ConsoleColor.Green);
            first.Duration.Restart();
            second.Duration.Restart();
        }

        private void SetUpKeepaliveTimer()
        {
            Timer keepaliveTimer = new Timer(1000);
            keepaliveTimer.Elapsed += async (s, e) =>
            {
                foreach (var connection in Connections.Where(c => c.Duration.ElapsedMilliseconds >= KeepaliveTimeout))
                {
                    Terminal.LogLine($"Sending keep alive message to {connection.IpEndPoint} on channel {connection.Channel}", ConsoleColor.DarkYellow);
                    var keepaliveMessage = Messaging.CreateMessage(MessageType.KeepAlive, ServerId, connection.Channel, 0);
                    await Client.SendAsync(keepaliveMessage, keepaliveMessage.Length, connection.IpEndPoint);
                    connection.Duration.Restart();
                }
            };
            keepaliveTimer.Start();
        }
    }
}
