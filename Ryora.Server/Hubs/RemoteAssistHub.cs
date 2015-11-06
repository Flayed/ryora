﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace Ryora.Server.Hubs
{
    public class RemoteAssistHub : Hub
    {
        public override Task OnConnected()
        {
            Groups.Add(Context.ConnectionId, "1");
            return base.OnConnected();
        }

        public async Task SendImage(string channel, int frame, string image)
        {
            await Clients.Group(channel).NewImage(frame, image);
        }
    }
}
