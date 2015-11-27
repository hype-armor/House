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
using System.Windows.Threading;
using System.Timers;

namespace EchoClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Timer aTimer;
        public MainWindow()
        {
            InitializeComponent();

            // Create a timer with a two second interval.
            aTimer = new System.Timers.Timer(5000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
            
        }

        private string Update = "updateupdate";
        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            aTimer.Stop();
            HitServer(Update);
            aTimer.Start();
        }

        private static Guid guid = Guid.NewGuid();

        private void gobtn_Click(object sender, RoutedEventArgs e)
        {
            gobtn.IsEnabled = false;
            string text = txtQuery.Text.CleanText();

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
                string address = serverPath + "guid=" + guid.ToString() + ";query=" + query + ";";
                byte[] data = client.DownloadData(address);

                if (data.GetString() == Update)
                {
                    // update only
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        lblUpdateTime.Content = DateTime.Now.ToLongTimeString();
                    }));
                }
                else if (data.Length > 0)
                {
                    MediaPlayer mp = new MediaPlayer(data);
                    mp.Play();

                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        gobtn.IsEnabled = true;
                    }));
                }
                else
                {
                    MessageBox.Show("query: " + query + Environment.NewLine + "Did not send guid", "Error");
                }
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
                soundPlayer.PlaySync();
            }
        }
    }
}
