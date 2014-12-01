using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Globalization;
using System.Threading;


namespace House
{
    class Program
    {
        public void Main(string[] args)
        {
            
            say("Hello, I am house.");
            start(null);

        }

        private void promps(string input)
        {
            txtUnderstood.Text += input + Environment.NewLine;
            Weather w = new Weather();

            if (input.Contains("weather") || input.Contains("wheather"))
            {
                // look up weather
                say(w.Condition);
                say(w.Temperature);
                say(w.Hazards);
            }
            else if (input.Contains("temperature") || input.Contains("temp"))
            {
                // get just the temp
                say(w.Temperature);
            }
            else if (input.Contains("forecast"))
            {
                say(w.Forecast);
            }
            else if (input.Contains("lights"))
            {
                // turn lights on/off
                say("lights, on");
            }
            else if (input.Contains("time"))
            {
                // get the time
                say("The time is " + DateTime.Now.ToShortTimeString());
            }
            else if (input.Contains("alarm"))
            {
                // set alarm clock
            }
            else if (input.Contains("national news"))
            {
                // get the news
                say("Loading the news");
                News n = new News();
                while (true)
                {
                    if (!n.Loaded)
                    {
                        Thread.Sleep(50);
                    }
                    else
                    {
                        break;
                    }
                }
                
                foreach (var item in n.NationalNews)
                {
                    say("");
                    say(item.Key);
                }
            }
            else if (input.Contains("local news"))
            {
                // get the news
                say("Loading the news");
                News n = new News();
                while (true)
                {
                    if (!n.Loaded)
                    {
                        Thread.Sleep(50);
                    }
                    else
                    {
                        break;
                    }
                }

                foreach (var item in n.LocalNews)
                {
                    say("");
                    say(item.Key);
                }
            }
            else if (input.Contains("search for"))
            {
                string question = input.Replace("search for", "");
                SearchEng s = new SearchEng();
                string response = s.Search(question);
                say(response);
            }
            else
            {
                say(input + ", is not a valid command.");
            }
        }


        private static void say(string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                SpeechSynthesizer synth = new SpeechSynthesizer();
                synth.SetOutputToDefaultAudioDevice();
                synth.Speak(text);
            }
        }
    }
}
