using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using Microsoft.AspNet.SignalR;

namespace Ryora.Server.Hubs.Accessors
{
    public static class RemoteAssistHubAccessor
    {
        private static IHubContext _context = null;
        public static IHubContext Context => _context ?? (_context = GlobalHost.ConnectionManager.GetHubContext<RemoteAssistHub>());

        public static async Task PublishImage(int channel, Guid imageGuid)
        {
            //await Context.Clients.Group($"{channel}").NewImage(imageGuid);
            await Context.Clients.All.NewImage(imageGuid);
        }
    }
}
