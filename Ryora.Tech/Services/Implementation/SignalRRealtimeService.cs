using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Ryora.Tech.Models;
using Ryora.Tech.Services;

namespace Ryora.Tech.Services.Implementation
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

        public event EventHandler NewImage;
        public event EventHandler MouseMove;
        public event EventHandler Sharing;
        
        public SignalRRealtimeService(short channel)
        {
            var queryString = new Dictionary<string, string> {{"Channel", channel.ToString()}};
            HubConnection = new HubConnection(HostUrl, queryString);
            HubProxy = HubConnection.CreateHubProxy("RemoteAssistHub");
            HubProxy.On<int, string>("NewImage", (frame, image) =>
            {
                if (NewImage == null) return;
                //NewImage(this, new NewImageEventArgs(frame, image));
            });
            HubProxy.On<int, int>("MouseMove", (x, y) =>
            {
                if (MouseMove == null) return;
                MouseMove(this, new MouseMoveEventArgs(x, y));
            });
            HubProxy.On("Share", (isSharing) =>
            {
                if (Sharing == null) return;
                Sharing(this, new SharingEventArgs(isSharing));
            });
        }

        public async Task StartConnection(short channel)
        {

            await HubConnection.Start();
        }

        public string Transport {
            get
            {
                if (HubConnection == null || HubConnection.State == ConnectionState.Disconnected) return "Disconnected";
                return HubConnection.Transport.Name;
            }
        }
    }
}
