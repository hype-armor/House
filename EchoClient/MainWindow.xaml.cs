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
using Extensions;
using System.Diagnostics;
using EchoClient.EchoDuplex;
using System.ServiceModel;

namespace EchoClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        EchoDuplexClient client;
        InstanceContext instanceContext;
        public MainWindow()
        {
            InitializeComponent();

            instanceContext = new InstanceContext(new CallbackHandler());
            client = new EchoDuplexClient(instanceContext);
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

        public byte[] Combine(byte[] first, byte[] second)
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
            client.Post(new MemoryStream(buffer, 0, buffer.Length, true, true));
            buffer = null;
        }

        
    }
    public class CallbackHandler : EchoDuplexCallback
    {
        public void Push(MemoryStream audioStream)
        {
            audioStream.Position = 0L;
            MediaPlayer mp = new MediaPlayer(audioStream.GetBuffer());
            mp.Play();
        }
    }
}
