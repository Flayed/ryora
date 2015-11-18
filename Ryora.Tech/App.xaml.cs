using Ryora.Tech.Services;
using Ryora.Tech.Services.Implementation;
using System.Windows;

namespace Ryora.Tech
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
            RealtimeService = new UdpRealtimeService(Channel);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            RealtimeService.EndConnection(Channel);
            base.OnExit(e);
        }
    }
}
