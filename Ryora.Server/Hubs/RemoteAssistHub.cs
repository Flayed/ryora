using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace Ryora.Server.Hubs
{
    public class RemoteAssistHub : Hub
    {
        private static readonly List<string> Channels = new List<string>();

        private readonly string DataChannel = "DataChannel";
        public override async Task OnConnected()
        {
            var channel = Context.QueryString.Get("Channel");
            await Groups.Add(Context.ConnectionId, !string.IsNullOrWhiteSpace(channel) ? channel : DataChannel);
            if (string.IsNullOrWhiteSpace(channel))
            {
                await Clients.Caller.ChannelListing(Channels);
            }
            else
            {
                if (!Channels.Contains(channel))
                {
                    Channels.Add(channel);
                    await Clients.Group(DataChannel).NewChannel(channel);
                }

            }
            await base.OnConnected();
        }

        public async Task Share(string channel, bool isSharing)
        {
            await Clients.Group(channel).Share(isSharing);
        }

        public async Task SendImage(string channel, int frame, string image)
        {
            await Clients.Group(channel).NewImage(frame, image);
            await Clients.Group(DataChannel).MoreData(channel, (Math.Floor((double)image.Length / 3) + 1) * 4 + 1 + channel.Length * 8 + 32);
        }

        public async Task SendMouseCoords(string channel, double x, double y)
        {
            await Clients.Group(channel).MouseMove(x, y);
            await Clients.Group(DataChannel).MoreData(channel, 128 + channel.Length*8);
        }
    }
}
