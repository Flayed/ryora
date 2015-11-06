using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;

namespace Ryora.Tech
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

        public event EventHandler NewImage;
        public event EventHandler MouseMove;
        public event EventHandler Sharing;

        public class NewImageEventArgs : EventArgs
        {
            public string Image { get; set; }
            public int Frame { get; set; }

            public NewImageEventArgs(int frame, string image)
            {
                Frame = frame;
                Image = image;
            }
        }

        public class MouseMoveEventArgs : EventArgs
        {
            public double X { get; set; }
            public double Y { get; set; }

            public MouseMoveEventArgs(double x, double y)
            {
                X = x;
                Y = y;
            }
        }

        public class SharingEventArgs : EventArgs
        {
            public bool IsSharing { get; set; }

            public SharingEventArgs(bool isSharing)
            {
                IsSharing = isSharing;
            }
        }

        public RealtimeClient()
        {
            var queryString = new Dictionary<string, string> {{"Channel", "1"}};
            HubConnection = new HubConnection(HostUrl, queryString);
            HubProxy = HubConnection.CreateHubProxy("RemoteAssistHub");
            HubProxy.On<int, string>("NewImage", (frame, image) =>
            {
                if (NewImage == null) return;
                NewImage(this, new NewImageEventArgs(frame, image));
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

        public async Task StartConnection()
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
