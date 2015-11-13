using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ryora.Client.Services.Implementation
{
    public class VisualTreeScreenshotService : IScreenshotService
    {
        public MemoryStream GetScreenshot(Visual target)
        {
            if (target == null)
            {
                return null;
            }
            MemoryStream ms = new MemoryStream();
            Application.Current.Dispatcher.Invoke(() =>
            {
                var bounds = VisualTreeHelper.GetDescendantBounds(target);


                RenderTargetBitmap renderTarget = new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height, 96,
                    96, PixelFormats.Pbgra32);

                DrawingVisual visual = new DrawingVisual();

                using (DrawingContext context = visual.RenderOpen())
                {
                    VisualBrush visualBrush = new VisualBrush(target);
                    context.DrawRectangle(visualBrush, null, new Rect(new System.Windows.Point(), bounds.Size));
                }

                renderTarget.Render(visual);
                PngBitmapEncoder bitmapEncoder = new PngBitmapEncoder();
                bitmapEncoder.Frames.Add(BitmapFrame.Create(renderTarget));
                bitmapEncoder.Save(ms);
            });
            return ms;
        }

        public Task<IEnumerable<CachedBitmap>> GetScreenshots()
        {
            throw new NotImplementedException();
        }

        public void ForceUpdate(Rectangle updateRectangle)
        {
            throw new NotImplementedException();
        }
    }
}
