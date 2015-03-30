﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using WindowsMicrophoneMuteLibrary;

namespace OpenEcho
{
    class Speech
    {
        public static WindowsMicMute micMute = new WindowsMicMute();
        public static string Understood;
        public static bool Silent = false;

        private static Queue q = new Queue();

        static Speech()
        {
            Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        foreach (Action a in q)
                        {
                            a.Invoke();
                        }
                        Thread.Sleep(5);
                    }
                    
                });
        }

        public static void say(string text, string title = "House")
        {
            q.Enqueue(new Action(() =>
                {
                    if (Silent)
                    {
                        MessageBox.Show(text, title, MessageBoxButton.OK, MessageBoxImage.Information,
                        MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
                        Understood = text;
                    }
                    else
                    {
                        SpeechSynthesizer synth = new SpeechSynthesizer();
                        synth.SetOutputToDefaultAudioDevice();

                        micMute.MuteMic();
                        synth.Speak(text);

                        micMute.UnMuteMic();
                    }
                    
                })
            );
        }

        public static void osay(string text, string title = "House")
        {
            if (Silent)
            {
                Task.Factory.StartNew(() =>
                    {
                        MessageBox.Show(text, title, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
                    });
                
                Understood = text;
            }
            else
            {
                Task.Factory.StartNew(() =>
                {
                    
                    // Initialize a new instance of the SpeechSynthesizer.
                    SpeechSynthesizer synth = new SpeechSynthesizer();

                    // Configure the audio output. 
                    synth.SetOutputToDefaultAudioDevice();

                    micMute.MuteMic();
                    synth.Speak(text);

                    micMute.UnMuteMic();
                });
            }
        }
    }
}
