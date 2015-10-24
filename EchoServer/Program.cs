using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ExtensionMethods;
using System.Reflection;
using System.Configuration.Install;
using System.Windows.Forms;
using System.Speech.Synthesis;
using System.Threading;

namespace EchoServer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            if (Environment.UserInteractive)
            {
                
                MessageBox.Show("This application is a service. Please install it.", "OpenEcho", MessageBoxButtons.OK, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                //ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
                //ManagedInstallerClass.InstallHelper(new string[] {Assembly.GetExecutingAssembly().Location });

            }
            else
            {
                ServiceBase.Run(new Echo());
            }
        }

        static Quartz quartz = new Quartz();
        static QueryClassification qc = new QueryClassification();
        static Wikipedia wikipedia = new Wikipedia();

        public static byte[] ProcessInput(string input, string id)
        {
            MessageSystem messageSystem = new MessageSystem();

            KeyValuePair<QueryClassification.Actions, string> term = qc.Classify(input);

            if (term.Key == QueryClassification.Actions.help)
            {
                messageSystem.Post(id, MessageSystem.MessageType.output, qc.help);
            }
            else if (term.Key == QueryClassification.Actions.wikipedia)
            {
                input = input.Replace(term.Value, "").Trim();
                string ret = wikipedia.Search(input);
                messageSystem.Post(id, MessageSystem.MessageType.output, ret);
            }
            else if (term.Key == QueryClassification.Actions.timer)
            {
                WordsToNumbers wtn = new WordsToNumbers();

                wtn.ToLong(input);
                var value = term.Value.ToString();
                var key = term.Key.ToString();

                input = input.Replace(value, key);

                quartz.Understand(input);
            }
            else if (term.Key == QueryClassification.Actions.newPhrase)
            {
                // usage: add search term, x to y.
                input = input.Replace(term.Value, "").Trim();
                string[] words = input.Split(new string[] { " to " }, StringSplitOptions.RemoveEmptyEntries);

                string verb = words[0];

                try
                {
                    QueryClassification.Actions action = QueryClassification.ParseAction(words[1]);
                    QueryClassification.AddPhraseToAction(verb, action);
                }
                catch (ArgumentException e)
                {
                    string erroredAction = e.Message.Replace("Requested value ", "").Replace(" was not found.", "").Replace("'", "");
                    bool actionNotFound = erroredAction == words[1];
                    if (actionNotFound)
                    {
                        // action does not exist.
                        messageSystem.Post(id, MessageSystem.MessageType.output, "The action " + erroredAction + " does not exist.");
                    }
                    else
                    {
                        messageSystem.Post(id, MessageSystem.MessageType.output, "Error casting word to action. " + e.Message);
                    }
                }
            }
            else if (term.Key == QueryClassification.Actions.wolframAlpha)
            {
                Wolfram wolf = new Wolfram();
                string result = wolf.Query(id, input, messageSystem);
                messageSystem.Post(id, MessageSystem.MessageType.output, result);
            }
            else if (term.Key == QueryClassification.Actions.weather)
            {
                Weather weather = new Weather();
                weather.Update();

                if (input.CleanText().Contains("forcast"))
                {
                    messageSystem.Post(id, MessageSystem.MessageType.output, weather.Forecast);
                }
                else
                {
                    messageSystem.Post(id, MessageSystem.MessageType.output, "It is currently, " + weather.Temperature + " and " + weather.Condition);
                }
            }
            else if (term.Key == QueryClassification.Actions.joke)
            {
                Jokes joke = new Jokes();
                messageSystem.Post(id, MessageSystem.MessageType.output, joke.TellAJoke());
            }
            else if (term.Key == QueryClassification.Actions.clear)
            {
                Console.Clear();
            }


            byte[] wav = null;

            var t = new System.Threading.Thread(() =>
            {
                SpeechSynthesizer synth = new SpeechSynthesizer();
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    synth.SetOutputToWaveStream(memoryStream);

                    string response = messageSystem.Get(id);

                    synth.Speak(response);
                    wav = memoryStream.ToArray();
                }
            });
            t.Start();
            t.Join();
           
            return wav;
            //return new byte[0];
        }
    }
}
