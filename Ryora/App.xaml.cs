using Ryora.Client.Services;
using Ryora.Client.Services.Implementation;
using System.Windows;

namespace Ryora.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal short Channel = 1;
        private readonly IRealtimeService RealtimeService;

        public App()
        {
            RealtimeService = new UdpRealtimeService();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            RealtimeService.EndConnection(Channel);
            base.OnExit(e);
        }
    }
}
