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

        public async Task Share(short channel, bool isSharing)
        {
            await Clients.Group(channel.ToString()).Share(isSharing);
        }

        public async Task SendImage(short channel, int frame, byte[] image)
        {
            await Clients.Group(channel.ToString()).NewImage(frame, image);
            await Clients.Group(DataChannel).MoreData(channel, image.Length * 8 + 1 + 16 + 32);
        }

        public async Task SendImageFragment(short channel, int frame, int x, int y, int width, int height, byte[] image)
        {
            await Clients.Group(channel.ToString()).NewImageFragment(frame, x, y, width, height, image);
            await Clients.Group(DataChannel).MoreData(channel, image.Length * 8 + 16 + 32 + 32 + 32 + 32 + 32);
        }

        public async Task SendMouseCoords(short channel, double x, double y)
        {
            await Clients.Group(channel.ToString()).MouseMove(x, y);
            await Clients.Group(DataChannel).MoreData(channel, 128 + 16);
        }
    }
}
