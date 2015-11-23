using System;
using System.Drawing;
using System.Threading.Tasks;

namespace Ryora.Client.Services
{
    public interface IRealtimeService
    {
        event EventHandler MouseInput;
        event EventHandler KeyboardInput;
        event EventHandler Connect;
        event EventHandler<bool> Disconnect;

        Task StartConnection(short channel, int screenWidth, int screenHeight);
        Task EndConnection(short channel, bool reconnect = false);
        Task SendImage(short channel, int x, int y, int width, int height, byte[] image);
        Task SendImage(short channel, Rectangle location, byte[] image);
        Task SendMouseCoords(short channel, int x, int y, int screenWidth, int screenHeight);
        Task Sharing(short channel, bool isSharing);


    }
}
