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
                int peaches = 53;
                say(peaches.ToWords());
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
