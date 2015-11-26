using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ExtensionMethods;
using System.Threading;
using System.Windows.Threading;

namespace EchoClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void gobtn_Click(object sender, RoutedEventArgs e)
        {
            string text = txtQuery.Text.CleanText();

            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                HitServer(text);
            }));
        }

        public void HitServer(string query)
        {
            WebClient client = new WebClient();

            // Add a user agent header in case the 
            // requested URI contains a query.

            client.Headers.Add("user-agent", "EchoClient_version=1.1");

            string serverPath = "http://192.168.0.50:8080/";

            if (!string.IsNullOrWhiteSpace(query))
            {
                byte[] data;
                do
                {
                    data = client.DownloadData(serverPath + query);
                    byte[] guid = data.Take(16).ToArray();
                    data = data.Skip(16).ToArray();

                    Guid _guid = new Guid();
                    // check if 16 bytes are a guid.
                    if (Guid.TryParse(guid.ToString(), out _guid))
                    {
                        // server said the request will take a second. Check back with this GUID.
                        query = guid.GetString();

                        MediaPlayer.Play(data);
                        Thread.Sleep(500);
                        continue;
                    }
                    else
                    {
                        MediaPlayer.Play(data);
                    }
                }
                while (true);
            }
        }

        public class MediaPlayer
        {
            public static void Play(byte[] buffer)
            {
                System.Media.SoundPlayer soundPlayer;
                var memoryStream = new MemoryStream(buffer, true);
                soundPlayer = new System.Media.SoundPlayer(memoryStream);

                soundPlayer.Stream.Seek(0, SeekOrigin.Begin);
                soundPlayer.Stream.Write(buffer, 0, buffer.Length);
                soundPlayer.Play();
            }
        }
    }
}
