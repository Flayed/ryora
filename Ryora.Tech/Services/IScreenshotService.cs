using System.Drawing;
using System.Windows.Media.Imaging;

namespace Ryora.Tech.Services
{
    public interface IScreenshotService
    {
        BitmapSource ProcessBitmap(Rectangle imagePosition, byte[] imageData);
    }
}
