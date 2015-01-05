using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Speech.Synthesis;
using System.Threading;
using System.Windows.Threading;
using ExtensionMethods;

namespace OpenEcho
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WindowsMicrophoneMuteLibrary.WindowsMicMute micMute
                = new WindowsMicrophoneMuteLibrary.WindowsMicMute();

        private static System.Timers.Timer aTimer;


        public MainWindow()
        {
            InitializeComponent();

            //say("Hello, my name is Lexy");
            micMute.UnMuteMic();
            aTimer = new System.Timers.Timer(1000); 
            aTimer.Elapsed += Timeout;
            //aTimer.Enabled = true;
            txtInput.Focus();

            micMute.UnMuteMic();
        }
        private void Timeout(object sender, System.Timers.ElapsedEventArgs e)
        {
            micMute.MuteMic();
            aTimer.Stop();

            ProcessInput();
        }

        private void ProcessInput()
        {
            string input = "";
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                input = txtInput.Text;

            }));
            Thread.Sleep(50); // make sure input has been populated.
            string wikiSearchTerm = "search wikipedia for ";
            if (input.StartsWith("what are ") || input.StartsWith("what is ") || input.StartsWith("what is a "))
            {
                input = input.Replace("what are ", "");
                input = input.Replace("what is a ", "");
                input = input.Replace("what is ", "");

                // define term
                Wikipedia wiki = new Wikipedia();
                say(wiki.Search(input, true));
            }
            else if (input.StartsWith("how do you spell ") || input.StartsWith("spell ") || input.StartsWith("spell the word "))
            {
                // spell term

                string word = input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Last();
                say(word + " is spelled.");
                Thread.Sleep(1200);
                foreach (char letter in word)
                {
                    say(letter.ToString());
                    Thread.Sleep(700);
                }
            }
            else if (input.StartsWith(wikiSearchTerm))
            {
                input = input.Replace(wikiSearchTerm, "");

                Wikipedia wiki = new Wikipedia();
                say(wiki.Search(input));
            }
            else if (input == "set alarm for")
            {
                // input will come as hours and then minutes. 
                int peaches = 53;
                say(peaches.ToWords());
            }
            else if (input.Contains("set timer"))
            {
                // input example: set timer for two hours and fourteen minutes.
                // input will come as number hours and then number minutes. 

                input = input.Replace("set timer for", "")
                    .Replace("set timer", "")
                    .Replace("and", ""); // best think of a better way to clean up useless words.

                DateTime dt = DateTime.Now;
                
                if (input.Contains("hours"))
                {
                    string[] temp = input.Split(new string[] { "hours", "hour" }, StringSplitOptions.RemoveEmptyEntries);
                    long hours = temp.First().Trim().ToLong();
                    input = temp.Last().Trim();
                   
                    dt.AddHours(hours);
                }
                if (input.Contains("minutes"))
                {
                    string[] temp = input.Split(new string[] { "minutes" }, StringSplitOptions.RemoveEmptyEntries);
                    long minutes = temp.First().Trim().ToLong();
                    dt.AddMinutes(minutes);
                }
                say(dt.ToShortTimeString());
            }
            else
            {
                // send the input to Wolfram to see if it can make sense of it.
                say("Searching Wolfram Alpha");
                Wolfram wa = new Wolfram();
                say(wa.Query(input));
            }
        }

        private void txtInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            aTimer.Stop();
            aTimer.Start();
        }

        private void say(string text)
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
