using Gma.System.MouseKeyHook;
using Ryora.Client.Models;
using Ryora.Client.Services;
using Ryora.Client.Services.Implementation;
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
        public readonly IRealtimeService RealtimeService;
        public readonly IScreenshotService ScreenshotService;
        public readonly IInputService InputService;
        internal readonly long MouseMoveThrottle = 50;
        internal readonly Stopwatch MouseMoveThrottleTimer = new Stopwatch();
        internal short Channel = 1;

        private readonly IKeyboardMouseEvents GlobalKeyboardMouseHook;

        public MainWindow()
        {
            InitializeComponent();

            GlobalKeyboardMouseHook = Hook.GlobalEvents();

            MouseMoveThrottleTimer.Start();
            RealtimeService = new UdpRealtimeService();
            //RealtimeService = new SignalRRealtimeService(Channel);
            ScreenshotService = new BitBlitScreenshotService();
            InputService = new PInvokeInputService();


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
                foreach (var update in ScreenshotService.GetUpdates())
                {
                    if (update == null) return;
                    using (var ms = new MemoryStream())
                    {
                        update.Bitmap.Save(ms, GetEncoder(ImageFormat.Jpeg), JpgEncoderParameters);
                        var bytes = ms.GetBuffer();
                        await
                            RealtimeService.SendImage(Channel, update.Location.X, update.Location.Y,
                                update.Location.Width, update.Location.Height, bytes);
                    }
                }
            });

            GlobalKeyboardMouseHook.MouseMove += async (s, e) =>
            {
                if (!IsStreaming || MouseMoveThrottleTimer.ElapsedMilliseconds < MouseMoveThrottle) return;
                MouseMoveThrottleTimer.Restart();
                await RealtimeService.SendMouseCoords(Channel, e.X, e.Y, ScreenshotService.ScreenWidth, ScreenshotService.ScreenHeight);
            };

            RealtimeService.MouseInput += (s, e) =>
            {
                if (!IsStreaming) return;
                var ea = e as MouseMessageEventArgs;
                if (ea == null || ea.X == 0 || ea.Y == 0 || ea.ScreenWidth == 0 || ea.ScreenHeight == 0) return;
                InputService.SetMousePosition(ea.X, ea.Y, ea.ScreenWidth, ea.ScreenHeight, ea.LeftButton, ea.MiddleButton, ea.RightButton);
            };

            RealtimeService.KeyboardInput += (s, e) =>
            {
                if (!IsStreaming) return;
                var ea = e as KeyboardEventArgs;
                if (ea == null || ea.Keys.Length == 0) return;
                InputService.SendKeys(ea.IsDown, ea.Keys);
            };

            RealtimeService.Disconnect += (s, e) =>
            {
                InputService.Reset();
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