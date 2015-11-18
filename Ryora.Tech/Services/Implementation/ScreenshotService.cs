using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Ryora.Tech.Services.Implementation
{
    public class ScreenshotService : IScreenshotService
    {
        [DllImport("user32.dll")]
        static extern int GetSystemMetrics(SystemMetric smIndex);
        public enum SystemMetric
        {
            SM_CXSCREEN = 0,  // 0x00
            SM_CYSCREEN = 1,  // 0x01
        }

        private int? _screenWidth = null;

        public int ScreenWidth
        {
            get
            {
                if (!_screenWidth.HasValue)
                {
                    _screenWidth = GetSystemMetrics(SystemMetric.SM_CXSCREEN);
                }
                return _screenWidth.Value;
            }
        }

        private int? _screenHeight = null;

        public int ScreenHeight
        {
            get
            {
                if (!_screenHeight.HasValue)
                {
                    _screenHeight = GetSystemMetrics(SystemMetric.SM_CYSCREEN);
                }
                return _screenHeight.Value;
            }
        }

        private Bitmap PrimaryBitmap { get; set; } = null;

        public ScreenshotService()
        {
        }

        public void SetBitmapSize(short width, short height)
        {
            PrimaryBitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        }

        public BitmapSource ProcessBitmap(Rectangle imagePosition, byte[] imageData)
        {
            if (PrimaryBitmap == null) return null;
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
