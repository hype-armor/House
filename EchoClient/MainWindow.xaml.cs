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
using Extensions;
using System.Windows.Threading;
using System.Timers;
using NAudio;
using NAudio.Wave;

namespace EchoClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Timer aTimer;
        private static Guid guid;
        private string Update = "updateupdate";

        public MainWindow()
        {
            InitializeComponent();

            // Create a timer with a two second interval.
            aTimer = new System.Timers.Timer(1000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            //aTimer.Enabled = true;

            guid = Guid.NewGuid();
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            aTimer.Stop();
            HitServer(Update);
            aTimer.Start();
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
                string address = serverPath + "guid=" + guid.ToString() + "&query=" + query;
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
                }
                else
                {
                    MessageBox.Show("query: " + query + Environment.NewLine + "Did not send guid", "Error");
                }
            }
        }

        public void HitServer(byte[] query)
        {
            WebClient client = new WebClient();

            // Add a user agent header in case the 
            // requested URI contains a query.

            client.Headers.Add("user-agent", "EchoClient_version=0.2");

            string serverPath = "http://192.168.0.50:8080/";

            if (query.Length > 0)
            {
                string result = System.Text.Encoding.UTF8.GetString(query);
                string address = serverPath + "guid=" + guid.ToString() + "&query=" + result;
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

        public WaveIn waveSource = null;
        private byte[] buffer = null;
        private void waveSource_RecordingStopped(object sender, StoppedEventArgs e)
        {
            if (waveSource != null)
            {
                waveSource.Dispose();
                waveSource = null;
            }

            startBtn.IsEnabled = true;
        }

        private void waveSource_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (buffer == null)
            {
                buffer = e.Buffer;
            }
            else
            {
                buffer = Combine(buffer, e.Buffer);
            }
        }

        private void startBtn_Click(object sender, RoutedEventArgs e)
        {
            waveSource = new WaveIn();
            waveSource.WaveFormat = new WaveFormat(44100, 1);

            waveSource.DataAvailable += new EventHandler<WaveInEventArgs>(waveSource_DataAvailable);
            waveSource.RecordingStopped += new EventHandler<StoppedEventArgs>(waveSource_RecordingStopped);

            waveSource.StartRecording();
            startBtn.IsEnabled = false;
            stopBtn.IsEnabled = true;
        }

        private void stopBtn_Click(object sender, RoutedEventArgs e)
        {
            stopBtn.IsEnabled = false;
            startBtn.IsEnabled = true;
            waveSource.StopRecording();

            // prep for transfer to server
            HitServer(buffer);
        }

        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }
    }
}
