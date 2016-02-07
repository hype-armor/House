using System;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Timers;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Threading;
using System.IO;
using NAudio;
using NAudio.Wave;
using System.Windows.Input;

namespace EchoClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Guid guid;
        private System.Timers.Timer aTimer;
        public MainWindow()
        {
            InitializeComponent();

            guid = Guid.NewGuid();

            // Create a timer with a two second interval.
            aTimer = new System.Timers.Timer(1000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            aTimer.Stop();
            DateTime CurrentDateTime = DateTime.Now;
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                lblUpdateTime.Content = StartClient(new byte[0], true);
            }));
            aTimer.Start();
        }

        private void startBtn_Click(object sender, RoutedEventArgs e)
        {
            //lblUpdateTime.Content = StartClient("look up what a panda is");
        }

        public static string StartClient(byte[] query, bool update = false)
        {
            // Data buffer for incoming data.
            byte[] bytes = new byte[1024];

            // Connect to a remote device.
            try
            {
                // Establish the remote endpoint for the socket.
                // This example uses port 11000 on the local computer.
                IPHostEntry ipHostInfo = Dns.GetHostEntry("sky.ibang.us");
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 8080);

                // Create a TCP/IP  socket.
                Socket sender = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.
                try
                {
                    sender.Connect(remoteEP);

                    Console.WriteLine("Socket connected to {0}",
                        sender.RemoteEndPoint.ToString());

                    // Encode the data string into a byte array.
                    if (update)
                    {
                        query = Encoding.ASCII.GetBytes(guid.ToString() + "<UPDATE><EOF>");
                    }
                    else
                    {
                        query = Combine(guid.ToByteArray(), query);
                    }
                    byte[] msg = Combine(query, Encoding.ASCII.GetBytes("<EOF>"));


                    // Send the data through the socket.
                    int bytesSent = sender.Send(msg);

                    // Receive the response from the remote device.
                    int bytesRec = sender.Receive(bytes);
                    string response = Encoding.ASCII.GetString(bytes, 0, bytesRec);

                    // Release the socket.
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();

                    return response;

                }
                catch (ArgumentNullException ane)
                {
                    return "ArgumentNullException : " + ane.ToString();
                }
                catch (SocketException se)
                {
                    return "SocketException : " + se.ToString();
                }
                catch (Exception e)
                {
                    return "Unexpected exception : " + e.ToString();
                }

            }
            catch (Exception e)
            {
                return e.ToString();
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
            //Post(buffer);
        }
    }

}
