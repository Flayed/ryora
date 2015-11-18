using System;
using System.Drawing;
using System.Threading.Tasks;

namespace Ryora.Client.Services
{
    public interface IRealtimeService
    {
        Task StartConnection(short channel);
        Task EndConnection(short channel);
        Task SendImage(short channel, int x, int y, int width, int height, byte[] image);
        Task SendImage(short channel, Rectangle location, byte[] image);
        Task SendMouseCoords(short channel, int x, int y);
        Task Sharing(short channel, bool isSharing);

        event EventHandler<Rectangle> MissedFragmentEvent;
    }
}
