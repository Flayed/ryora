using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace Ryora.Client.Services.Implementation
{
    public class SignalRRealtimeService : IRealtimeService
    {
        public event EventHandler MouseInput;

        private readonly HubConnection HubConnection;
        private IHubProxy HubProxy;
#if DEBUG
        private const string HostUrl = "http://localhost/Ryora.Server/";
#else
        private const string HostUrl = "http://ryora.azurewebsites.net";
#endif
        private bool IsStarted = false;
        public SignalRRealtimeService(short channel)
        {
            var queryString = new Dictionary<string, string> { { "Channel", channel.ToString() } };
            HubConnection = new HubConnection(HostUrl, queryString);
            HubProxy = HubConnection.CreateHubProxy("RemoteAssistHub");

            HubConnection.Start();
        }

        public event EventHandler KeyboardInput;
        public event EventHandler Disconnect;

        public async Task StartConnection(short channel, int screenWidth, int screenHeight)
        {
            await HubConnection.Start();
            IsStarted = true;
        }

        public async Task EndConnection(short channel)
        {
            await Task.Delay(0);
            HubConnection.Stop();
        }

        public async Task SendImage(short channel, int x, int y, int width, int height, byte[] image)
        {
            if (!GoodConnection) return;
            await HubProxy.Invoke("SendImageFragment", channel, x, y, width, height, image);
        }

        public Task SendImage(short channel, Rectangle location, byte[] image)
        {
            return SendImage(channel, location.X, location.Y, location.Width, location.Height, image);
        }

        public async Task SendMouseCoords(short channel, int x, int y, int screenWidth, int screenHeight)
        {
            if (!GoodConnection) return;
            await HubProxy.Invoke("SendMouseCoords", channel, x, y);
        }

        public async Task Sharing(short channel, bool isSharing)
        {
            if (!GoodConnection) return;
            await HubProxy.Invoke("Share", channel, isSharing);
        }

        private bool GoodConnection => (IsStarted && HubConnection.State == ConnectionState.Connected);
        public event EventHandler<Rectangle> MissedFragmentEvent;
    }
}
