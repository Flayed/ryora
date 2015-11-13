using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Ryora.Client.Services
{
    public interface IRealtimeService
    {
        Task StartConnection(short channel);
        Task SendImage(short channel, int frame, byte[] image);
        Task SendImage(short channel, int frame, int x, int y, int width, int height, byte[] image);
        Task SendMouseCoords(short channel, double x, double y);
        Task Sharing(short channel, bool isSharing);

        event EventHandler<Rectangle> MissedFragmentEvent;
    }
}
