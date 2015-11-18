using Microsoft.AspNet.SignalR.Client;
using Ryora.Tech.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

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
        public event EventHandler ClientResolutionChanged;
        public event EventHandler MouseMove;
        public event EventHandler Sharing;

        public SignalRRealtimeService(short channel)
        {
            var queryString = new Dictionary<string, string> { { "Channel", channel.ToString() } };
            HubConnection = new HubConnection(HostUrl, queryString);
            HubProxy = HubConnection.CreateHubProxy("RemoteAssistHub");
            HubProxy.On<Rectangle, byte[]>("NewImage", (location, image) =>
            {
                if (NewImage == null) return;
                NewImage(this, new NewImageEventArgs(location, image));
            });
            HubProxy.On<int, int, int, int>("MouseMessage", (x, y, sw, sh) =>
            {
                if (MouseMove == null) return;
                MouseMove(this, new MouseMoveEventArgs(x, y, sw, sh));
            });
            HubProxy.On("Share", (isSharing) =>
            {
                if (Sharing == null) return;
                Sharing(this, new SharingEventArgs(isSharing));
            });
        }

        public async Task StartConnection(short channel, int screenWidth, int screenHeight)
        {

            await HubConnection.Start();
        }

        public async Task EndConnection(short channel)
        {
            await Task.Delay(0);
            HubConnection.Stop();
        }

        public Task SendMouseCoords(short channel, int x, int y, int screenWidth, int screenHeight)
        {
            throw new NotImplementedException();
        }

        public string Transport
        {
            get
            {
                if (HubConnection == null || HubConnection.State == ConnectionState.Disconnected) return "Disconnected";
                return HubConnection.Transport.Name;
            }
        }
    }
}
