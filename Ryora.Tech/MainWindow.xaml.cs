using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.AspNet.SignalR.Client;
using Ryora.Tech.Models;
using Ryora.Tech.Services;
using Ryora.Tech.Services.Implementation;

namespace Ryora.Tech
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static int LastFrame { get; set; } = 0;
        private static System.Windows.Point LastPoint { get; set; } = new System.Windows.Point(0, 0);
        private static short Channel { get; } = 1;
        private readonly IRealtimeService RealtimeService;


        public MainWindow()
        {
            InitializeComponent();
            MousePointer.Source = GetMousePointerImage();
            //RealtimeService = new SignalRRealtimeService(Channel);
            RealtimeService = new UdpRealtimeService();
            RealtimeService.NewImage += (o, e) =>
            {
                var ea = e as NewImageEventArgs;
                if (ea?.Image == null || ea?.Frame < LastFrame)
                    return;
                LastFrame = ea.Frame;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        using (var ms = new MemoryStream(ea.Image))
                        {                            
                            Bitmap bmp = new Bitmap(ms, true);
                            this.Screenshot.Source = Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(),
                                IntPtr.Zero,
                                Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                });
            };

            RealtimeService.MouseMove += (o, e) => {
                var ea = e as MouseMoveEventArgs;
                if (ea == null) return;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (!MousePointer.IsVisible) MousePointer.Visibility = Visibility.Visible;

                    var offset = VisualTreeHelper.GetOffset(MousePointer);
                    TranslateTransform trans = new TranslateTransform();
                    MousePointer.RenderTransform = trans;
                    DoubleAnimation anim1 = new DoubleAnimation(LastPoint.Y, ea.Y, TimeSpan.FromMilliseconds(100));
                    DoubleAnimation anim2 = new DoubleAnimation(LastPoint.X, ea.X, TimeSpan.FromMilliseconds(100));
                    Console.WriteLine($"({LastPoint.X},{LastPoint.Y}) => ({ea.X}, {ea.Y})");
                    trans.BeginAnimation(TranslateTransform.YProperty, anim1);
                    trans.BeginAnimation(TranslateTransform.XProperty, anim2);
                    LastPoint = new System.Windows.Point(ea.X, ea.Y);
                });
            };

            RealtimeService.Sharing += (o, e) =>
            {
                var ea = e as SharingEventArgs;
                if (ea == null) return;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (!ea.IsSharing)
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

            Task.Run(async () =>
            {
                await RealtimeService.StartConnection(Channel);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Title = $"Technician View [Connection Type: {RealtimeService.Transport}]";
                });
            });
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
