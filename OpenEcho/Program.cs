/*
    OpenEcho is a program to automate basic tasks at home all while being handsfree.
    Copyright (C) 2015 Gregory Morgan

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Net;
using System.Collections.Generic;
using ExtensionMethods;
using System.Speech.Synthesis;
using System.IO;

namespace OpenEcho
{
    class Program
    {
        static Quartz quartz = new Quartz();
        static QueryClassification qc = new QueryClassification();
        static Wikipedia wikipedia = new Wikipedia();


        static void Main(string[] args)
        {
            // start web server...
            WebServer ws = new WebServer();
            ws.Start(IPAddress.Any, 80, "/");

        }

        public static byte[] ProcessInput(string input, string id)
        {
            MessageSystem messageSystem = new MessageSystem();

            KeyValuePair<QueryClassification.Actions, string> term = qc.Classify(input);

            if (term.Key == QueryClassification.Actions.help)
            {
                messageSystem.Post(id, qc.help);
            }
            else if (term.Key == QueryClassification.Actions.wikipedia)
            {
                input = input.Replace(term.Value, "").Trim();
                string ret = wikipedia.Search(input);
                messageSystem.Post(id, ret);
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
                        messageSystem.Post(id, "The action " + erroredAction + " does not exist.");
                    }
                    else
                    {
                        messageSystem.Post(id, "Error casting word to action. " + e.Message);
                    }
                }
            }
            else if (term.Key == QueryClassification.Actions.wolframAlpha)
            {
                Wolfram wolf = new Wolfram();
                string result = wolf.Query(id, input, messageSystem);
                messageSystem.Post(id, result);
            }
            else if (term.Key == QueryClassification.Actions.weather)
            {
                Weather weather = new Weather();
                weather.Update();

                if (input.CleanText().Contains("forcast"))
                {
                    messageSystem.Post(id, weather.Forecast);
                }
                else
                {
                    messageSystem.Post(id, "It is currently, " + weather.Temperature + " and " + weather.Condition);
                }
            }
            else if (term.Key == QueryClassification.Actions.joke)
            {
                Jokes joke = new Jokes();
                messageSystem.Post(id, joke.TellAJoke());
            }
            else if (term.Key == QueryClassification.Actions.clear)
            {
                Console.Clear();
            }


            SpeechSynthesizer synth = new SpeechSynthesizer();
            string path = @"C:\Users\Greg\Documents\output.wav";
            synth.SetOutputToWaveFile(path);
            //synth.Speak();

            // wait for file to finish being written to.
            
            byte[] wav = File.ReadAllBytes(path);


            return wav;
        }
    }
}
