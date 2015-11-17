using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Ryora.Tech.Services.Implementation
{
    public class ScreenshotService : IScreenshotService
    {
        private Bitmap PrimaryBitmap { get; set; }

        public ScreenshotService()
        {
            PrimaryBitmap = new Bitmap(1920, 1080, PixelFormat.Format32bppArgb);
        }

        public BitmapSource ProcessBitmap(Rectangle imagePosition, byte[] imageData)
        {
            using (var ms = new MemoryStream(imageData))
            {
                using (var bmp = new Bitmap(ms))
                {
                    using (var graphics = Graphics.FromImage(PrimaryBitmap))
                    {
                        graphics.DrawImage(bmp, imagePosition, 0, 0, imagePosition.Width,
                            imagePosition.Height, GraphicsUnit.Pixel);
                    }
                }
            }

            var source = new BitmapImage();
            using (Stream stream = new MemoryStream())
            {
                PrimaryBitmap.Save(stream, ImageFormat.Bmp);
                source.BeginInit();
                source.StreamSource = stream;
                source.CacheOption = BitmapCacheOption.OnLoad;
                source.EndInit();
            }
            return source;
        }
    }
}
