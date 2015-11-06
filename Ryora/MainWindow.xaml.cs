using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Encoder = System.Drawing.Imaging.Encoder;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Point = System.Drawing.Point;

namespace Ryora.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.LayoutUpdated += async (s, e) =>
            {
                if (!IsStreaming || IsCapturing) return;
                IsCapturing = true;
                using (var ms = CreateBitmapFromVisual(this))
                {
                    if (ms == null) return;
                    var imageData = ms.ToArray();
                    await ProcessImage(imageData);
                }
                IsCapturing = false;
            };
        }

        public readonly EncoderParameters EncoderParameters = new EncoderParameters(1)
        {
            Param = {[0] = new EncoderParameter(Encoder.Quality, 25L) }
        };

        private static int Frame { get; set; } = 0;
        private static bool IsStreaming { get; set; } = false;
        private static bool IsCapturing { get; set; } = false;

        private Timer _screenshotTimer = null;
        public Timer ScreenshotTimer
        {
            get
            {
                if (_screenshotTimer == null)
                {
                    _screenshotTimer = new Timer(1000);
                    _screenshotTimer.Elapsed += async (se, ev) =>
                    {
                        using (var ms = CreateBitmapFromVisual(this))
                        {
                            if (ms == null) return;
                            var imageData = ms.ToArray();
                            await ProcessImage(imageData);
                        }
                    };
                }
                return _screenshotTimer;
            }
        }

        private void GoTimeButton_Click(object sender, RoutedEventArgs e)
        {
            GoTimeButton.Content = !IsStreaming ? "Start Streaming" : "Stop Streaming";
            IsStreaming = !IsStreaming;
        }

        private async Task ProcessImage(byte[] bitmap)
        {
            try
            {
                using (
                    var client = new HttpClient()
                    {
                        BaseAddress = new Uri("http://ryora.azurewebsites.net/API/RA/")
                    })
                {
                    var response = await client.PostAsync($"1/{Frame++}", new ByteArrayContent(bitmap));
                    Console.WriteLine($"Frame: {Frame}");
                }
            }
            catch (Exception ex)
            {
                //this.ErrorMessage.Text = $"Error doing something dog:\n{ex.Message}";
                Console.WriteLine(ex.Message);
            }
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            var codecs = ImageCodecInfo.GetImageDecoders();
            return codecs.FirstOrDefault(codec => codec.FormatID == format.Guid);
        }

        private static MemoryStream CreateBitmapFromVisual(Visual target)
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
    }
}