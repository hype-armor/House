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
using System.Speech.AudioFormat;
using System.Net.Cache;

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
            Get(Update);
            aTimer.Start();
        }

        public void Get(string query)
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

        public void Post(byte[] query)
        {
            WebClient client = new WebClient();
            
            // Add a user agent header in case the 
            // requested URI contains a query.

            client.Headers.Add("user-agent", "EchoClient_version=0.2");
            string serverPath = "http://192.168.0.50:8080/";

            if (query.Length > 0)
            {
                string result = System.Text.Encoding.UTF8.GetString(query);
                //string address = serverPath + "guid=" + guid.ToString() + "&query=" + result;
                string address = serverPath + "guid=" + guid.ToString();

                Uri myUri = new Uri(address, UriKind.Absolute);

                test t = new test();
                t.star(myUri, query);

                //var qhat = client.UploadData(myUri, query);
                //try
                //{
                //    var qhat = client.UploadData(myUri, new byte[5] { 00, 11, 22, 33, 44 });
                //}
                //catch (WebException we)
                //{
                //    if (we.Status != WebExceptionStatus.ReceiveFailure)
                //    {
                //        System.Diagnostics.Debugger.Launch();
                //    }
                //    // normal failure due to server closing the connection.
                //}
                //finally
                //{
                //
                //}

                //byte[] data = client.DownloadData(address);
                //
                //if (data.GetString() == Update)
                //{
                //    // update only
                //    Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                //    {
                //        lblUpdateTime.Content = DateTime.Now.ToLongTimeString();
                //    }));
                //}
                //else if (data.Length > 0)
                //{
                //    MediaPlayer mp = new MediaPlayer(data);
                //    mp.Play();
                //}
                //else
                //{
                //    MessageBox.Show("query: " + query + Environment.NewLine + "Did not send guid", "Error");
                //}
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

        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }

        private void startBtn_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            waveSource = new WaveIn();
            WaveFormat wf = new WaveFormat(44100, 16, 1);
            waveSource.WaveFormat = wf;//new WaveFormat(44100, 1);

            waveSource.DataAvailable += new EventHandler<WaveInEventArgs>(waveSource_DataAvailable);
            waveSource.RecordingStopped += new EventHandler<StoppedEventArgs>(waveSource_RecordingStopped);

            waveSource.StartRecording();
        }

        private void startBtn_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            waveSource.StopRecording();

            // prep for transfer to server
            Post(buffer);
        }
    }

    public class WebClientEx : WebClient
    {
        public int Timeout { get; set; }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            request.Timeout = Timeout;
            //request.KeepAlive = false;
            //request.ProtocolVersion = HttpVersion.Version10;
            //request.ServicePoint.ConnectionLimit = 12;
            return request;
        }
    }

    public class test
    {
        public void star(Uri uri, byte[] data)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);
            if (request != null)
            {
                request.Method = "POST";
                if (data.Length > 0)
                {

                    request.ContentLength = data.Length;
                    request.ContentType = "application/json";
                    using (var requestStream = request.GetRequestStream())
                    {
                        requestStream.Write(data, 0, data.Length);
                    }
                }
                else
                {
                    request.ContentLength = 0;
                }
                request.Timeout = 15000;
                request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);

                string output = string.Empty;
                try
                {
                    using (var response = (HttpWebResponse)request.GetResponse())
                    {
                        using (var stream = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(1252)))
                        {
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                while (!stream.EndOfStream)
                                {
                                    output += stream.ReadLine();
                                }
                                output = stream.ReadToEnd();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
        }
    }
}
