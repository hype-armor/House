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

            //Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            //{
            //    HitServer(text);
            //}));
            HitServer(text);
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
                do
                {
                    byte[] data = client.DownloadData(serverPath + query);
                    MediaPlayer mp = new MediaPlayer(data);
                    byte[] guid = data.Take(16).ToArray();
                    data = data.Skip(16).ToArray();

                    Guid _guid = new Guid();
                    // check if 16 bytes are a guid.
                    if (Guid.TryParse(guid.ToString(), out _guid))
                    {
                        // server said the request will take a second. Check back with this GUID.
                        query = guid.GetString();

                        mp.Play();
                        Thread.Sleep(500);
                        continue;
                    }
                    else
                    {
                        mp.Play();
                        return;
                    }
                }
                while (true);
            }
        }

        public class MediaPlayer
        {
            System.Media.SoundPlayer soundPlayer;

            public MediaPlayer(byte[] buffer)
            {
                var memoryStream = new MemoryStream(buffer, true);
                soundPlayer = new System.Media.SoundPlayer(memoryStream);
            }

            public void Play()
            {
                soundPlayer.Play();
            }
        }
    }
}
