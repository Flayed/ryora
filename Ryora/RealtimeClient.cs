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
#if DEBUG
        private const string HostUrl = "http://localhost/Ryora.Server/";
#else
        private const string HostUrl = "http://ryora.azurewebsites.net";
#endif
        private bool IsStarted = false;
        public RealtimeClient()
        {
            var queryString = new Dictionary<string, string> {{"Channel", "1"}};
            HubConnection = new HubConnection(HostUrl, queryString);
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

        public async Task SendMouseCoords(string channel, double x, double y)
        {
            if (!IsStarted) return;
            await HubProxy.Invoke("SendMouseCoords", channel, x, y);
        }

        public async Task Sharing(string channel, bool isSharing)
        {
            if (!IsStarted) return;
            await HubProxy.Invoke("Share", channel, isSharing);
        }
    }
}
