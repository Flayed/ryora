using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;

namespace Ryora.Client.Services.Implementation
{
    public class SignalRRealtimeService : IRealtimeService
    {
        private readonly HubConnection HubConnection;
        private IHubProxy HubProxy;
#if DEBUG
        private const string HostUrl = "http://localhost/Ryora.Server/";
#else
        private const string HostUrl = "http://ryora.azurewebsites.net";
#endif
        private bool IsStarted = false;
        public SignalRRealtimeService()
        {
            var queryString = new Dictionary<string, string> {{"Channel", "1"}};
            HubConnection = new HubConnection(HostUrl, queryString);
            HubProxy = HubConnection.CreateHubProxy("RemoteAssistHub");
            
            HubConnection.Start();
        }

        public async Task StartConnection(short channel)
        {
            await HubConnection.Start();
            IsStarted = true;
        }

        public async Task SendImage(short channel, int frame, byte[] image)
        {
            if (!IsStarted) return;
            await HubProxy.Invoke("SendImage", channel, frame, image);
        }

        public async Task SendMouseCoords(short channel, double x, double y)
        {
            if (!IsStarted) return;
            await HubProxy.Invoke("SendMouseCoords", channel, x, y);
        }

        public async Task Sharing(short channel, bool isSharing)
        {
            if (!IsStarted) return;
            await HubProxy.Invoke("Share", channel, isSharing);
        }
    }
}
