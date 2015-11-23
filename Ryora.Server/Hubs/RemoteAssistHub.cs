using Microsoft.AspNet.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        public async Task SendImage(short channel, int x, int y, int width, int height, byte[] image)
        {
            await Clients.Group(channel.ToString()).SendImage(x, y, width, height, image);
        }
    }
}
