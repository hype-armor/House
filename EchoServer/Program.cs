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
    class Program
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
        static private Guid guid;

        public Guid _guid
        {
            get { return guid; }
        }

        public byte[] Go(string input, bool isEchoClient)
        {
            input = input == string.Empty ? "help" : input;

            MessageSystem messageSystem = new MessageSystem();

            // check message system for input if input is a guid
            guid = Guid.NewGuid();
            bool isGuid = input.Length == 16 && Guid.TryParse(input, out guid);
            if (!isGuid)
            {
                guid = Guid.NewGuid();
                ProcessInput(guid, input, messageSystem);
            }

            string response = messageSystem.Get(guid).CleanText();
            return GetAudio(response);
        }

        private static void ProcessInput(Guid guid, string input, MessageSystem messageSystem)
        {
            QueryClassification qc = new QueryClassification();
            KeyValuePair<QueryClassification.Actions, string> term = qc.Classify(input);

            if (term.Key == QueryClassification.Actions.help)
            {
                messageSystem.Post(guid, Message.Type.output, qc.help);
            }
            else if (term.Key == QueryClassification.Actions.wikipedia)
            {
                input = input.Replace(term.Value, "").Trim();
                Wikipedia.Go(guid, messageSystem, input);
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
                        messageSystem.Post(guid, Message.Type.output, "The action " + erroredAction + " does not exist.");
                    }
                    else
                    {
                        messageSystem.Post(guid, Message.Type.output, "Error casting word to action. " + e.Message);
                    }
                }
            }
            else if (term.Key == QueryClassification.Actions.wolframAlpha)
            {
                Wolfram.Go(guid, input, messageSystem);
            }
            else if (term.Key == QueryClassification.Actions.weather)
            {
                Weather.Go(guid, input, messageSystem);
            }
            else if (term.Key == QueryClassification.Actions.joke)
            {
                Jokes.Go(guid, input, messageSystem);
            }
            else if (term.Key == QueryClassification.Actions.clear)
            {
                Console.Clear();
            }
        }

        private static byte[] GetAudio(string response)
        {
            byte[] wav = null;

            var t = new System.Threading.Thread(() =>
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    SpeechSynthesizer synth = new SpeechSynthesizer();
                    synth.SetOutputToWaveStream(memoryStream);
                    synth.Speak(response);
                    wav = memoryStream.ToArray();
                }
            });
            t.Start();
            t.Join();

            return wav;
        }

        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }
    }
}
