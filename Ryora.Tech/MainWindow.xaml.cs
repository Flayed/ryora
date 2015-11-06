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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.AspNet.SignalR.Client;

namespace Ryora.Tech
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static int LastFrame { get; set; } = 0;

        public MainWindow()
        {
            InitializeComponent();
            RealtimeClient realtimeClient = new RealtimeClient();
            realtimeClient.NewImage += (o, e) =>
            {
                var ea = e as RealtimeClient.NewImageEventArgs;
                if (ea?.Image == null || ea?.Frame < LastFrame)
                    return;
                LastFrame = ea.Frame;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        using (var ms = new MemoryStream(Convert.FromBase64String(ea.Image)))
                        {                            
                            Bitmap bmp = new Bitmap(ms);
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
        }
    }
}
