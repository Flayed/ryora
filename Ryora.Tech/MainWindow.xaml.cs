using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
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
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Rectangle = System.Drawing.Rectangle;

namespace Ryora.Tech
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>Deletes a logical pen, brush, font, bitmap, region, or palette, freeing all system resources associated with the object. After the object is deleted, the specified handle is no longer valid.</summary>
        /// <param name="hObject">A handle to a logical pen, brush, font, bitmap, region, or palette.</param>
        /// <returns>
        ///   <para>If the function succeeds, the return value is nonzero.</para>
        ///   <para>If the specified handle is not valid or is currently selected into a DC, the return value is zero.</para>
        /// </returns>
        /// <remarks>
        ///   <para>Do not delete a drawing object (pen or brush) while it is still selected into a DC.</para>
        ///   <para>When a pattern brush is deleted, the bitmap associated with the brush is not deleted. The bitmap must be deleted independently.</para>
        /// </remarks>
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        private static int LastFrame { get; set; } = 0;
        private static System.Windows.Point LastPoint { get; set; } = new System.Windows.Point(0, 0);
        private static Bitmap ScreenBitmap = new Bitmap(1920, 1080);
        private static short Channel { get; } = 1;
        private readonly IRealtimeService RealtimeService;


        public MainWindow()
        {
            InitializeComponent();
            MousePointer.Source = GetMousePointerImage();
            //RealtimeService = new SignalRRealtimeService(Channel);
            RealtimeService = new UdpRealtimeService(Channel);
            
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
                            using (var bmp = new Bitmap(ms, true))
                            {
                                this.Screenshot.Source = Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(),
                                    IntPtr.Zero,
                                    Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                });
            };

            RealtimeService.NewImageFragment += (o, e) =>
            {
                var ea = e as NewImageFragmentEventArgs;
                if (ea?.Image == null || ea?.Frame < LastFrame) return;
                LastFrame = ea.Frame;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        //using (var ms = new MemoryStream(ea.Image))
                        //{
                        //    Bitmap bmp = new Bitmap(ms, true);
                        //    this.Screenshot.Source = Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(),
                        //        IntPtr.Zero,
                        //        Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                        //}
                        using (var ms = new MemoryStream(ea.Image))
                        {                            
                            using (var bmp = new Bitmap(ms))
                            {
                                using (var graphics = Graphics.FromImage(ScreenBitmap))
                                {
                                    graphics.DrawImage(bmp, ea.ImagePosition, 0, 0, ea.ImagePosition.Width,
                                        ea.ImagePosition.Height, GraphicsUnit.Pixel);
                                }
                                var hbitmap = ScreenBitmap.GetHbitmap();
                                var bmpSource = Imaging.CreateBitmapSourceFromHBitmap(hbitmap,                                    
                                    IntPtr.Zero,
                                    Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());                                
                                Screenshot.Source = bmpSource;
                                DeleteObject(hbitmap);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                        ea.Image = null;
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
