﻿using Ryora.Udp;
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
                Console.SetCursorPosition(0, 0);
                Console.Write($"Bytes Used: {BytesUsed}B {KilobytesUsed}KB {MegabytesUsed}MB  ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"{KilobytesPerSecond} KB/s  ");
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.Write($"Recv: {MessagesReceived}  ");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"Avg: {AverageMessageSize}KB        ");
                Console.ResetColor();

                switch (requestMessage.Type)
                {
                    case MessageType.Connect:
                        var connectionRequest = new ConnectionRequest(request.RemoteEndPoint, requestMessage.ConnectionId,
                            requestMessage.Channel);
                        if (ConnectionRequests.Any(cr => cr.Id.Equals(connectionRequest.Id)))
                        {
                            ConnectionRequests.Remove(ConnectionRequests.First(cr => cr.Id.Equals(connectionRequest.Id)));
                        }
                        ConnectionRequests.Add(connectionRequest);
                        var channelConnections =
                            ConnectionRequests.Where(cr => cr.Channel.Equals(connectionRequest.Channel)).ToArray();
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.SetCursorPosition(0, ++ConsoleLine);
                        Console.WriteLine($"Channel {connectionRequest.Channel} has {channelConnections.Count()} connections.");
                        Console.ResetColor();
                        if (channelConnections.Count() == 2)
                        {
                            await SendAcknowledgement(channelConnections.ElementAt(0), channelConnections.ElementAt(1));
                        }
                        break;
                    case MessageType.Disconnect:
                        ConnectionRequests.RemoveAll(cr => cr.Id.Equals(requestMessage.ConnectionId));
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.SetCursorPosition(0, ++ConsoleLine);
                        Console.WriteLine($"Client {requestMessage.ConnectionId} disconnected from channel {requestMessage.Channel}");
                        Console.ResetColor();
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
                Console.ForegroundColor = ConsoleColor.Red;
                Console.SetCursorPosition(0, ++ConsoleLine);
                Console.WriteLine($"Something bad happend: {ex.Message} {ex.InnerException?.Message}");
                Console.ResetColor();
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
            var message = Messaging.CreateMessage(MessageType.Acknowledge, ServerId, firstRequest.Channel, 0, "Initializing Connection");
            await Client.SendAsync(message, message.Length, firstRequest.IpEndPoint);
            await Client.SendAsync(message, message.Length, secondRequest.IpEndPoint);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.SetCursorPosition(0, ++ConsoleLine);
            Console.WriteLine($"  Sent Connection Acknowledgement to channel {firstRequest.Channel} {firstRequest.Id}({firstRequest.IpEndPoint}) and {secondRequest.Id}({secondRequest.IpEndPoint})");
            Console.ResetColor();
        }
    }
}
