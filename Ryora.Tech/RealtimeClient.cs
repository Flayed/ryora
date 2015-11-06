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
        private const string HostUrl = "http://ryora.azurewebsites.net";

        public event EventHandler NewImage;

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

        public RealtimeClient()
        {
            HubConnection = new HubConnection(HostUrl);
            HubProxy = HubConnection.CreateHubProxy("RemoteAssistHub");
            HubProxy.On<int, string>("NewImage", (frame, image) =>
            {
                if (NewImage == null) return;
                NewImage(this, new NewImageEventArgs(frame, image));
            });
            HubProxy.On("Ping", () =>
            {
                Console.WriteLine("PING!");
            });

            HubConnection.Start();
        }

    }
}
