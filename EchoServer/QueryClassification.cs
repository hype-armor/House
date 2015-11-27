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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ExtensionMethods;

namespace EchoServer
{
    // for information on saving the object to a file on change. https://msdn.microsoft.com/en-us/library/et91as27.aspx

    [Serializable()]
    class QueryClassification
    {
        public enum Actions { help, wikipedia, newPhrase, alarm, timer, clear, wolframAlpha, weather, joke, unknown };

        [field: NonSerialized()]
        private static Dictionary<Actions, HashSet<string>> actionDatabase = new Dictionary<Actions, HashSet<string>>();

        static QueryClassification()
        {
            AddPhraseToAction("look up", Actions.wikipedia);
            AddPhraseToAction("lookup", Actions.wikipedia);

            AddPhraseToAction("what is", Actions.wolframAlpha);
            AddPhraseToAction("what is a", Actions.wolframAlpha);
            AddPhraseToAction("what is an", Actions.wolframAlpha);

            AddPhraseToAction("add verb", Actions.newPhrase);

            AddPhraseToAction("set a timer for", Actions.timer);
            AddPhraseToAction("create a timer for", Actions.timer);

            AddPhraseToAction("help", Actions.help);
            AddPhraseToAction("clear", Actions.clear);

            AddPhraseToAction("current weather", Actions.weather);
            AddPhraseToAction("weather", Actions.weather);
            AddPhraseToAction("forcast", Actions.weather);

            AddPhraseToAction("tell me a joke", Actions.joke);
            AddPhraseToAction("joke", Actions.joke);
        }

        public static void AddPhraseToAction(string phrase, Actions action)
        {
            if (!actionDatabase.Keys.Contains(action))
            {
                actionDatabase.Add(action, new HashSet<string>());
            }

            phrase = phrase.CleanText();

            HashSet<string> phrases = actionDatabase[action];
            phrases.Add(phrase);
            actionDatabase[action] = phrases;

        }

        public KeyValuePair<Actions, string> Classify(string input)
        {
            input = ApplyEnglish(input);
            Dictionary<Actions, string> matchedVerbs = new Dictionary<Actions, string>();

            foreach (KeyValuePair<Actions, HashSet<string>> item in actionDatabase)
            {
                Actions term = item.Key;
                HashSet<string> verbs = item.Value;

                foreach (string verb in verbs)
                {
                    if (input.Contains(verb) && !matchedVerbs.Keys.Contains(term))
                    {
                        matchedVerbs.Add(term, verb);
                    }
                    else if (input.Contains(verb) && matchedVerbs.Keys.Contains(term) && matchedVerbs[term].Length < verb.Length)
                    {
                        matchedVerbs[term] = verb;
                    }
                }
            }

            if (matchedVerbs.Count() == 1)
            {
                return matchedVerbs.First();
            }
            else if (matchedVerbs.Count() > 1)
            {
                return new KeyValuePair<Actions, string>
                    (Actions.unknown, "There is more than one match for your query. Please remove one of the matches from my database.");
            }
            else
            {
                return new KeyValuePair<Actions, string>(Actions.unknown, "I can not match your query to anything in my database.");
            }
        }

        private string ApplyEnglish(string input)
        {
            input = input.Replace("s is", " is");

            return input;
        }

        // we might need this for plugins. 
        public static Actions ParseAction(string word)
        {
            QueryClassification.Actions action =
                                (QueryClassification.Actions)Enum.Parse(typeof(QueryClassification.Actions), word, true);
            return action;
        }

        public string help
        {
            get
            {
                StringBuilder helpText = new StringBuilder();
                foreach (var term in actionDatabase)
                {
                    foreach (var value in term.Value)
                    {
                        helpText.AppendLine(term.Key + ": " + value);
                    }
                }

                return helpText.ToString();
            }
        }
    }
}
