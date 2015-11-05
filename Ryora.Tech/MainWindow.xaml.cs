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
        public MainWindow()
        {
            InitializeComponent();
            RealtimeClient realtimeClient = new RealtimeClient();
            realtimeClient.NewImage += async (o, e) => {
                var ea = e as RealtimeClient.NewImageEventArgs;
                if (ea?.ImageGuid == null)
                    return;
                using (var client = new HttpClient() { BaseAddress = new Uri("http://ryora.azurewebsites.net/API/RA/") })
                {
                    var response = await client.PostAsync("GetImage", new StringContent(ea.ImageGuid.ToString()));
                    response.EnsureSuccessStatusCode();
                    var content = await response.Content.ReadAsByteArrayAsync();
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            using (var ms = new MemoryStream(content))
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
                }
            };

        }
    }
}
