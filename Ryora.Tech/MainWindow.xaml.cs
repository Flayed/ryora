using Ryora.Tech.Models;
using Ryora.Tech.Services;
using Ryora.Tech.Services.Implementation;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Ryora.Tech
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int ImageTimerTick = 100;

        private static System.Windows.Point LastPoint { get; set; } = new System.Windows.Point(0, 0);

        private static short Channel { get; } = 1;
        internal readonly long MouseMoveThrottle = 50;
        internal readonly Stopwatch MouseMoveThrottleTimer = new Stopwatch();

        private readonly IRealtimeService RealtimeService;
        private readonly IScreenshotService ScreenshotService;


        public MainWindow()
        {
            InitializeComponent();
            MouseMoveThrottleTimer.Start();
            MousePointer.Source = GetMousePointerImage();
            RealtimeService = new UdpRealtimeService(Channel);
            ScreenshotService = new ScreenshotService();

            RealtimeService.ClientResolutionChanged += (o, e) =>
            {
                var ea = e as ClientResolutionChangedEventArgs;
                if (ea?.ScreenWidth == 0 || ea?.ScreenHeight == 0) return;
                ScreenshotService.SetBitmapSize(ea.ScreenWidth, ea.ScreenHeight);
            };

            Timer imageTimer = new Timer(ImageTimerTick);
            imageTimer.Elapsed += (s, e) =>
            {
                try
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var images = RealtimeService.CompletedImages.ToList();
                        if (!images.Any()) return;
                        var source = ScreenshotService.ProcessBitmaps(images);
                        Screenshot.Source = source;
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            };
            imageTimer.Start();

            RealtimeService.Disconnect += (s, reconnect) =>
            {
                if (reconnect)
                {
                    Task.Run(async () =>
                    {
                        await
                            RealtimeService.StartConnection(Channel, ScreenshotService.ScreenWidth,
                                ScreenshotService.ScreenHeight);
                    });
                }
            };

            RealtimeService.MouseMove += (o, e) =>
            {
                if (MouseMoveThrottleTimer.ElapsedMilliseconds < MouseMoveThrottle) return;
                MouseMoveThrottleTimer.Restart();
                var ea = e as MouseMoveEventArgs;
                if (ea == null) return;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (!Screenshot.IsInitialized) return;
                    double tx = Screenshot.ActualWidth / ea.ScreenWidth;
                    double ty = Screenshot.ActualHeight / ea.ScreenHeight;

                    double x = ((double)ea.X * tx - (double)Screenshot.ActualWidth / 2) + 5;
                    double y = ((double)ea.Y * ty - (double)Screenshot.ActualHeight / 2) + 9;

                    if (!MousePointer.IsVisible) MousePointer.Visibility = Visibility.Visible;

                    TranslateTransform trans = new TranslateTransform();
                    MousePointer.RenderTransform = trans;
                    DoubleAnimation anim1 = new DoubleAnimation(LastPoint.Y, y, TimeSpan.FromMilliseconds(100));
                    DoubleAnimation anim2 = new DoubleAnimation(LastPoint.X, x, TimeSpan.FromMilliseconds(100));
                    trans.BeginAnimation(TranslateTransform.YProperty, anim1);
                    trans.BeginAnimation(TranslateTransform.XProperty, anim2);
                    LastPoint = new System.Windows.Point(x, y);
                });
            };

            RealtimeService.Sharing += (o, isSharing) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (!isSharing)
                    {
                        PausedOverlay.Visibility = Visibility.Visible;
                        PausedText.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        PausedOverlay.Visibility = Visibility.Hidden;
                        PausedText.Visibility = Visibility.Hidden;
                    }

                });
            };

            MouseMove += async (s, e) => { await MouseEvent(s, e); };
            MouseDown += async (s, e) => { await MouseEvent(s, e); };
            MouseUp += async (s, e) => { await MouseEvent(s, e); };
            MouseWheel += async (s, e) => { await MouseEvent(s, e, e.Delta); };

            KeyDown += async (s, e) => { await KeyboardEvent(true, e); };
            KeyUp += async (s, e) => { await KeyboardEvent(false, e); };

            Task.Run(async () =>
            {
                await RealtimeService.StartConnection(Channel, ScreenshotService.ScreenWidth, ScreenshotService.ScreenHeight);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Title = $"Technician View [Connection Type: {RealtimeService.Transport}]";
                });
            });
        }

        private async Task KeyboardEvent(bool isDown, KeyEventArgs e)
        {
            await RealtimeService.SendKeyboardInput(Channel, isDown, (new[]
            {
                ProcessKey(e.SystemKey),
                ProcessKey(e.Key)
            }
            ).Where(k => k != 0).Distinct().ToArray());
            e.Handled = true;
        }

        private short ProcessKey(Key k)
        {
            if (k == Key.None || k == Key.System) return 0;
            var vk = KeyInterop.VirtualKeyFromKey(k);
            return (short)vk;
        }

        private async Task MouseEvent(object sender, MouseEventArgs e, int mouseWheelDelta = 0)
        {
            var position = e.GetPosition(Screenshot);
            await RealtimeService.SendMouseCoords(Channel, (int)position.X, (int)position.Y, mouseWheelDelta, (int)Screenshot.ActualWidth, (int)Screenshot.ActualHeight, e.LeftButton == MouseButtonState.Pressed, e.MiddleButton == MouseButtonState.Pressed, e.RightButton == MouseButtonState.Pressed, e.XButton1 == MouseButtonState.Pressed, e.XButton2 == MouseButtonState.Pressed);
        }

        public ImageSource GetMousePointerImage()
        {
            ImageSource source = null;
            string base64MousePointerImage =
                "iVBORw0KGgoAAAANSUhEUgAAAAoAAAASBAMAAACQmVJ4AAAAElBMVEUAAAD/KSn/KSn/KSn/KSkAAADTXdH0AAAABXRSTlMAVnJ3e9zRrXQAAAA/SURBVAgdBcExAQJAEAOw4OA7VAJKUHBD/Vsh8XvwDfQeuqC7R7fQ7Z5ui2473RK9haZHXu8he/gs+PSgB8kfhyMLXGTrT2kAAAAASUVORK5CYII=";

            using (var ms = new MemoryStream(Convert.FromBase64String(base64MousePointerImage)))
            {
                Bitmap bmp = new Bitmap(ms);
                source = Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            }
            return source;
        }
    }
}
