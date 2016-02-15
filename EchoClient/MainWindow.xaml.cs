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
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

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
                        query = Encoding.ASCII.GetBytes(guid.ToString() + "<UPDATE>");
                    }
                    else
                    {
                        var soundData = query;//CreateSinWave(44000, 120, TimeSpan.FromSeconds(60), 1d);
                        using (FileStream fs = new FileStream(DateTime.Now.ToString() + ".wav", FileMode.Create))
                        {
                            WriteHeader(fs, soundData.Length, 1, 44100);
                            fs.Write(soundData, 0, soundData.Length);
                            fs.Close();
                        }

                        query = Combine(Encoding.ASCII.GetBytes(guid.ToString()), query);
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
            WaveFormat wf = new WaveFormat(44100, 1);
            waveSource.WaveFormat = wf;//new WaveFormat(44100, 1);

            waveSource.DataAvailable += new EventHandler<WaveInEventArgs>(waveSource_DataAvailable);
            waveSource.RecordingStopped += new EventHandler<StoppedEventArgs>(waveSource_RecordingStopped);

            waveSource.StartRecording();
        }

        private void startBtn_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            waveSource.StopRecording();

            // prep for transfer to server
            StartClient(buffer);
        }

        static byte[] RIFF_HEADER = new byte[] { 0x52, 0x49, 0x46, 0x46 };
        static byte[] FORMAT_WAVE = new byte[] { 0x57, 0x41, 0x56, 0x45 };
        static byte[] FORMAT_TAG = new byte[] { 0x66, 0x6d, 0x74, 0x20 };
        static byte[] AUDIO_FORMAT = new byte[] { 0x01, 0x00 };
        static byte[] SUBCHUNK_ID = new byte[] { 0x64, 0x61, 0x74, 0x61 };
        private const int BYTES_PER_SAMPLE = 2;

        public static void WriteHeader(
             System.IO.Stream targetStream,
             int byteStreamSize,
             int channelCount,
             int sampleRate)
        {

            int byteRate = sampleRate * channelCount * BYTES_PER_SAMPLE;
            int blockAlign = channelCount * BYTES_PER_SAMPLE;

            targetStream.Write(RIFF_HEADER, 0, RIFF_HEADER.Length);
            targetStream.Write(PackageInt(byteStreamSize + 42, 4), 0, 4);

            targetStream.Write(FORMAT_WAVE, 0, FORMAT_WAVE.Length);
            targetStream.Write(FORMAT_TAG, 0, FORMAT_TAG.Length);
            targetStream.Write(PackageInt(16, 4), 0, 4);//Subchunk1Size    

            targetStream.Write(AUDIO_FORMAT, 0, AUDIO_FORMAT.Length);//AudioFormat   
            targetStream.Write(PackageInt(channelCount, 2), 0, 2);
            targetStream.Write(PackageInt(sampleRate, 4), 0, 4);
            targetStream.Write(PackageInt(byteRate, 4), 0, 4);
            targetStream.Write(PackageInt(blockAlign, 2), 0, 2);
            targetStream.Write(PackageInt(BYTES_PER_SAMPLE * 8), 0, 2);
            //targetStream.Write(PackageInt(0,2), 0, 2);//Extra param size
            targetStream.Write(SUBCHUNK_ID, 0, SUBCHUNK_ID.Length);
            targetStream.Write(PackageInt(byteStreamSize, 4), 0, 4);
        }

        static byte[] PackageInt(int source, int length = 2)
        {
            if ((length != 2) && (length != 4))
                throw new ArgumentException("length must be either 2 or 4", "length");
            var retVal = new byte[length];
            retVal[0] = (byte)(source & 0xFF);
            retVal[1] = (byte)((source >> 8) & 0xFF);
            if (length == 4)
            {
                retVal[2] = (byte)((source >> 0x10) & 0xFF);
                retVal[3] = (byte)((source >> 0x18) & 0xFF);
            }
            return retVal;
        }

        public static byte[] CreateSinWave(
            int sampleRate,
            double frequency,
            TimeSpan length,
            double magnitude
        )
        {
            int sampleCount = (int)(((double)sampleRate) * length.TotalSeconds);
            short[] tempBuffer = new short[sampleCount];
            byte[] retVal = new byte[sampleCount * 2];
            double step = Math.PI * 2.0d / frequency;
            double current = 0;

            for (int i = 0; i < tempBuffer.Length; ++i)
            {
                tempBuffer[i] = (short)(Math.Sin(current) * magnitude * ((double)short.MaxValue));
                current += step;
            }

            Buffer.BlockCopy(tempBuffer, 0, retVal, 0, retVal.Length);
            return retVal;
        }
    }
}
