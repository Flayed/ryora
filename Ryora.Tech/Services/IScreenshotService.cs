using System.Drawing;
using System.Windows.Media.Imaging;

namespace Ryora.Tech.Services
{
    public interface IScreenshotService
    {
        int ScreenWidth { get; }
        int ScreenHeight { get; }
        void SetBitmapSize(short width, short height);
        BitmapSource ProcessBitmap(Rectangle imagePosition, byte[] imageData);
    }
}
