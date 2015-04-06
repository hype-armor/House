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
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace OpenEcho
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static System.Timers.Timer aTimer;
        //private Speach speach = new Speach();

        public  MainWindow()
        {
            InitializeComponent();
            
            Speech.micMute.UnMuteMic();
            aTimer = new System.Timers.Timer(1000); 
            aTimer.Elapsed += Timeout;
            //aTimer.Enabled = true;
            txtInput.Focus();

            Speech.micMute.UnMuteMic();

            Quartz q = new Quartz();

            // google search test.
            //SearchEng se = new SearchEng();
            //se.Search("who is the president of the united states");
        }
        private void Timeout(object sender, System.Timers.ElapsedEventArgs e)
        {
            Speech.micMute.MuteMic();
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
            
            do
            {
                Thread.Sleep(50); // make sure input has been populated.
            } while (input == "");

            int WordCount = input.Split(new char[] {' '}).Count();

            string wikiSearchTerm = "search wikipedia for ";
            if (string.IsNullOrWhiteSpace(input))
            {
                Speech.say("");
            }
            else if (input.StartsWith("what are ") || input.StartsWith("what is ") || input.StartsWith("what is a "))
            {
                input = input.Replace("what are ", "");
                input = input.Replace("what is a ", "");
                input = input.Replace("what is ", "");

                // define term
                Wikipedia wiki = new Wikipedia();
                Speech.say(wiki.Search(input, true));
            }
            else if (input.StartsWith("how do you spell ") || input.StartsWith("spell ") || input.StartsWith("spell the word "))
            {
                // spell term

                string word = input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Last();
                Speech.say(word + " is spelled.");
                Thread.Sleep(1200);
                foreach (char letter in word)
                {
                    Speech.say(letter.ToString());
                    Thread.Sleep(700);
                }
            }
            else if (input.StartsWith(wikiSearchTerm) || WordCount == 1)
            {
                input = input.Replace(wikiSearchTerm, "");

                Wikipedia wiki = new Wikipedia();
                Speech.say(wiki.Search(input));
            }
            else if (input == "time" || input == "what is the time" || input == "what time is it" || input == "what's the time")
            {
                Speech.say(DateTime.Now.ToShortTimeString());
            }
            else if (input.Contains("set alarm for"))
            {
                
                string result = input.ReplaceWithNumber();
                string time = Regex.Match(result, @"\d+").Value;
                Speech.say(result.ToString());

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

                Quartz q = new Quartz();
                Speech.say(dt.ToShortTimeString());
            }
            else
            {
                // send the input to Wolfram to see if it can make sense of it.
                Speech.say("Searching Wolfram Alpha");
                Wolfram wa = new Wolfram();
                string ret = wa.Query(input);
                if (ret == "Wolfram|Alpha doesn't know how to interpret your input.")
                {
                    // send it to google?
                    ret = "pardon?";
                }
                Speech.say(ret);
            }
        }

        private void txtInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            aTimer.Stop();
            aTimer.Start();
        }

        private void cbSilent_Checked(object sender, RoutedEventArgs e)
        {
            Speech.Silent = cbSilent.IsChecked == true ? true : false; // Because of type 'bool?'.
        }

        private void cbSilent_Unchecked(object sender, RoutedEventArgs e)
        {
            Speech.Silent = cbSilent.IsChecked == true ? true : false; // Because of type 'bool?'.
        }

        
    }
}
