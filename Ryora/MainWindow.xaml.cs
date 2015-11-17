using Ryora.Client.Services;
using Ryora.Client.Services.Implementation;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Encoder = System.Drawing.Imaging.Encoder;

namespace Ryora.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public IRealtimeService RealtimeService;
        public IScreenshotService ScreenshotService;
        internal readonly long MouseMoveThrottle = 100;
        internal readonly Stopwatch MouseMoveThrottleTimer = new Stopwatch();
        internal short Channel = 1;
        private bool DebugText { get; set; } = true;

        public MainWindow()
        {
            InitializeComponent();
            MouseMoveThrottleTimer.Start();
            RealtimeService = new UdpRealtimeService();
            //RealtimeService = new SignalRRealtimeService(Channel);

            ScreenshotService = new BitBlitScreenshotService();

            RealtimeService.MissedFragmentEvent += (s, r) =>
            {
                ScreenshotService.ForceUpdate(r);
            };

            Task.Run(async () =>
            {
                await RealtimeService.StartConnection(Channel);
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
                            RealtimeService.SendImage(Channel, Frame++, update.Location.X, update.Location.Y,
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

            MouseMove += async (s, e) =>
            {
                if (!IsStreaming || MouseMoveThrottleTimer.ElapsedMilliseconds < MouseMoveThrottle) return;
                MouseMoveThrottleTimer.Restart();
                var pos = e.GetPosition(this);
                await RealtimeService.SendMouseCoords(Channel, pos.X, pos.Y);
            };
        }

        public readonly EncoderParameters EncoderParameters = new EncoderParameters(1)
        {
            Param = {[0] = new EncoderParameter(Encoder.Quality, 0L) }
        };

        private static int Frame { get; set; } = 0;
        private static bool IsStreaming { get; set; } = false;


        private EncoderParameters _jpgEncoderParameters = null;

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