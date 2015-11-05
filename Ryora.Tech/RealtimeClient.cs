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
            public Guid ImageGuid { get; set; }

            public NewImageEventArgs(Guid imageGuid)
            {
                ImageGuid = imageGuid;
            }
        }

        public RealtimeClient()
        {
            HubConnection = new HubConnection(HostUrl);
            HubProxy = HubConnection.CreateHubProxy("RemoteAssistHub");
            HubProxy.On("NewImage", (imageGuid) =>
            {
                if (NewImage == null) return;
                NewImage(this, new NewImageEventArgs(new Guid(imageGuid)));
            });
            HubProxy.On("Ping", () =>
            {
                Console.WriteLine("PING!");
            });

            HubConnection.Start();
        }

    }
}
