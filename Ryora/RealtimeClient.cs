using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;

namespace Ryora.Client
{
    public class RealtimeClient
    {
        private readonly HubConnection HubConnection;
        private IHubProxy HubProxy;
        private const string HostUrl = "http://ryora.azurewebsites.net";
        private bool IsStarted = false;
        public RealtimeClient()
        {
            HubConnection = new HubConnection(HostUrl);
            HubProxy = HubConnection.CreateHubProxy("RemoteAssistHub");
            
            HubConnection.Start();
        }

        public async Task StartConnection()
        {
            await HubConnection.Start();
            IsStarted = true;
        }

        public async Task SendImage(string channel, int frame, string image)
        {
            if (!IsStarted) return;
            await HubProxy.Invoke("SendImage", channel, frame, image);
        }
    }
}
