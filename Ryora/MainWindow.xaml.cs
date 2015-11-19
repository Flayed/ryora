using Gma.System.MouseKeyHook;
using Ryora.Client.Models;
using Ryora.Client.Services;
using Ryora.Client.Services.Implementation;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Ryora.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public IRealtimeService RealtimeService;
        public IScreenshotService ScreenshotService;
        public IMouseService MouseService;
        internal readonly long MouseMoveThrottle = 50;
        internal readonly Stopwatch MouseMoveThrottleTimer = new Stopwatch();
        internal short Channel = 1;

        private bool DebugText { get; set; } = true;

        private IKeyboardMouseEvents GlobalKeyboardMouseHook;

        public MainWindow()
        {
            InitializeComponent();

            GlobalKeyboardMouseHook = Hook.GlobalEvents();

            MouseMoveThrottleTimer.Start();
            RealtimeService = new UdpRealtimeService();
            //RealtimeService = new SignalRRealtimeService(Channel);
            ScreenshotService = new BitBlitScreenshotService();
            MouseService = new PinvokeMouseService();


            RealtimeService.MissedFragmentEvent += (s, r) =>
            {
                ScreenshotService.ForceUpdate(r);
            };

            Task.Run(async () =>
            {
                await RealtimeService.StartConnection(Channel, ScreenshotService.ScreenWidth, ScreenshotService.ScreenHeight);
            });

            ScreenshotTimer = new TimedProcessor(100, async () =>
            {
                //var update = ScreenshotService.GetUpdate();
                List<string> updateMessages = new List<string>();
                foreach (var update in ScreenshotService.GetUpdates())
                {
                    if (update == null) return;
                    updateMessages.Add($"({update.Location.X}, {update.Location.Y})-({update.Location.X + update.Location.Width}, {update.Location.Y + update.Location.Height})");
                    using (var ms = new MemoryStream())
                    {
                        update.Bitmap.Save(ms, GetEncoder(ImageFormat.Jpeg), JpgEncoderParameters);
                        var bytes = ms.GetBuffer();
                        await
                            RealtimeService.SendImage(Channel, update.Location.X, update.Location.Y,
                                update.Location.Width, update.Location.Height, bytes);
                    }
                }
                if (DebugText)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ErrorMessage.Text = string.Join("\n", updateMessages);
                    });
                }
            });

            GlobalKeyboardMouseHook.MouseMove += async (s, e) =>
            {
                if (!IsStreaming || MouseMoveThrottleTimer.ElapsedMilliseconds < MouseMoveThrottle) return;
                MouseMoveThrottleTimer.Restart();
                await RealtimeService.SendMouseCoords(Channel, e.X, e.Y, ScreenshotService.ScreenWidth, ScreenshotService.ScreenHeight);
            };

            RealtimeService.MouseMove += (s, e) =>
            {
                if (!IsStreaming) return;
                var ea = e as MouseMessageEventArgs;
                if (ea == null || ea.X == 0 || ea.Y == 0 || ea.ScreenWidth == 0 || ea.ScreenHeight == 0) return;

                double tx = ScreenshotService.ScreenWidth / (double)ea.ScreenWidth;
                double ty = ScreenshotService.ScreenHeight / (double)ea.ScreenHeight;

                double x = (ea.X * tx);
                double y = (ea.Y * ty);

                MouseService.SetMousePosition((int)x, (int)y, ea.LeftButton, ea.MiddleButton, ea.RightButton);

                if (DebugText)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ErrorMessage.Text += $"\nL: {ea.LeftButton} M: {ea.MiddleButton} R: {ea.RightButton} 1: {ea.FirstExtendedButton} 2: {ea.SecondExtendedButton}";
                    });
                }
            };
        }

        public readonly EncoderParameters EncoderParameters = new EncoderParameters(1)
        {
            Param = {[0] = new EncoderParameter(Encoder.Quality, 0L) }
        };

        private static int Frame { get; set; } = 0;
        private static bool IsStreaming { get; set; }


        private EncoderParameters _jpgEncoderParameters;

        public EncoderParameters JpgEncoderParameters
        {
            get
            {
                if (_jpgEncoderParameters == null)
                {
                    Encoder encoder = Encoder.Quality;
                    _jpgEncoderParameters = new EncoderParameters(1);
                    EncoderParameter encoderParameter = new EncoderParameter(encoder, 25L);
                    _jpgEncoderParameters.Param[0] = encoderParameter;
                }
                return _jpgEncoderParameters;
            }
        }

        private TimedProcessor ScreenshotTimer { get; set; }

        private void GoTimeButton_Click(object sender, RoutedEventArgs e)
        {
            IsStreaming = !IsStreaming;
            GoTimeButton.Content = !IsStreaming ? "Start Streaming" : "Stop Streaming";
            Task.Run(async () =>
            {
                await RealtimeService.Sharing(Channel, IsStreaming);
            });
            if (IsStreaming) ScreenshotTimer.Start();
            else ScreenshotTimer.Stop();
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            var codecs = ImageCodecInfo.GetImageDecoders();
            return codecs.FirstOrDefault(codec => codec.FormatID == format.Guid);
        }
    }
}