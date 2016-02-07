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
using Extensions;

namespace EchoServer
{
    // for information on saving the object to a file on change. https://msdn.microsoft.com/en-us/library/et91as27.aspx

    [Serializable()]
    class QueryClassification
    {
        public enum Actions { help, wikipedia, newPhrase, alarm, timer, clear, wolframAlpha, weather, joke, unknown };

        public List<string> _Actions = new List<string>();

        [field: NonSerialized()]
        //private static Dictionary<Actions, HashSet<string>> actionDatabase = new Dictionary<Actions, HashSet<string>>();
        private Dictionary<string, HashSet<string>> actionDatabase = new Dictionary<string, HashSet<string>>();

        void QueryClassificationf()
        {
            

            AddPhraseToAction("what is", "wolframAlpha");
            AddPhraseToAction("what is a", "wolframAlpha");
            AddPhraseToAction("what is an", "wolframAlpha");

            AddPhraseToAction("help", "help");
            AddPhraseToAction("clear", "clear");
        }

        public static void AddPhraseToAction(string phrase, Actions action)
        {
            //if (!actionDatabase.Keys.Contains(action))
            //{
            //    actionDatabase.Add(action, new HashSet<string>());
            //}

            //phrase = phrase.CleanText();

            //HashSet<string> phrases = actionDatabase[action];
            //phrases.Add(phrase);
            //actionDatabase[action] = phrases;

        }

        public void AddPhraseToAction(string phrase, string _action)
        {
            if (!actionDatabase.Keys.Contains(_action))
            {
                actionDatabase.Add(_action, new HashSet<string>());
            }

            phrase = phrase.CleanText();

            HashSet<string> phrases = actionDatabase[_action];
            phrases.Add(phrase);
            actionDatabase[_action] = phrases;

        }

        public void AddAction()
        {
            
        }

        public KeyValuePair<string, string> Classify(string input)
        {
            input = ApplyEnglish(input);
            Dictionary<string, string> matchedVerbs = new Dictionary<string, string>();

            foreach (KeyValuePair<string, HashSet<string>> item in actionDatabase)
            {
                string term = item.Key;
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
            else if (input == "help")
            {
                return new KeyValuePair<string, string>
                    ("help", help);
            }
            else if (matchedVerbs.Count() > 1)
            {
                return new KeyValuePair<string, string>
                    ("unknown", "There is more than one match for your query. Please remove one of the matches from my database.");
            }
            else
            {
                return new KeyValuePair<string, string>("unknown", "I can not match your query to anything in my database.");
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
