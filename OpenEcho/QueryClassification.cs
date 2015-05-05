﻿/*
    OpenEcho is a program to automate basic tasks at home all while being handsfree.
    Copyright (C) 2015  Gregory Morgan

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
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace OpenEcho
{
    // for information on saving the object to a file on change. https://msdn.microsoft.com/en-us/library/et91as27.aspx

    [Serializable()]
    class QueryClassification
    {
        private const string saveLocation = "QueryClassification.bin";

        public enum Actions {help, wikipedia, newAction, alarm, timer, unknown};

        [field: NonSerialized()]
        private static Dictionary<Actions, HashSet<string>> terms = new Dictionary<Actions, HashSet<string>>();

        static QueryClassification()
        {
            terms = LoadDict(saveLocation);

            AddVerbToAction("what is", Actions.wikipedia, true);
            AddVerbToAction("what is a", Actions.wikipedia, true);
            AddVerbToAction("what is an", Actions.wikipedia, true);

            AddVerbToAction("add verb", Actions.newAction, true);

            AddVerbToAction("set a timer for", Actions.timer, true);
            AddVerbToAction("create a timer for", Actions.timer, true);
            AddVerbToAction("make a timer for", Actions.timer, true);

            AddVerbToAction("help", Actions.help, true);
        }

        public static void AddVerbToAction(string verb, Actions action, bool silent = false)
        {
            if (!terms.Keys.Contains(action))
            {
                terms.Add(action, new HashSet<string>());
            }

            HashSet<string> verbs = terms[action];
            verbs.Add(verb);
            terms[action] = verbs;

            SaveDict();

            if (!silent)
            {
                Speech.say("I have added " + verb + " to " + action); 
            }
        }

        private static Dictionary<Actions, HashSet<string>> LoadDict(string name)
        {
            Dictionary<Actions, HashSet<string>> dict = new Dictionary<Actions, HashSet<string>>();

            if (File.Exists(saveLocation))
            {
                Stream FileStream = File.OpenRead(saveLocation);
                BinaryFormatter deserializer = new BinaryFormatter();
                dict = (Dictionary<Actions, HashSet<string>>)deserializer.Deserialize(FileStream);
                FileStream.Close();
            }
            return dict;
        }

        private static void SaveDict()
        {
            if (File.Exists(saveLocation))
            {
                // save old dict
                string newFileName = saveLocation.Replace(".bin", " ") + 
                    DateTime.Now + 
                    ".bin";
                newFileName = newFileName
                    .Replace('/', '.')
                    .Replace(':','.');

                if (!File.Exists(newFileName))
                {
                    System.IO.File.Move(saveLocation, newFileName);
                }
            }
            Stream FileStream = File.Create(saveLocation);
            BinaryFormatter serializer = new BinaryFormatter();
            serializer.Serialize(FileStream, terms);
            FileStream.Close();
        }

        public KeyValuePair<Actions, string> Classify(string Query)
        {
            Dictionary<Actions, string> matchedVerbs = new Dictionary<Actions, string>();

            foreach (KeyValuePair<Actions, HashSet<string>> item in terms)
            {
                Actions term = item.Key;
                HashSet<string> verbs = item.Value;

                foreach (string verb in verbs)
                {
                    if (Query.Contains(verb) && !matchedVerbs.Keys.Contains(term))
                    {
                        matchedVerbs.Add(term, verb);
                    }
                    else if (Query.Contains(verb) && matchedVerbs.Keys.Contains(term) && matchedVerbs[term].Length < verb.Length)
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
                // more than one classification. List them and have user pick.
                Speech.say("There is more than one match for your query. Please remove one of the matches from my database.");

                return new KeyValuePair<Actions, string>(Actions.unknown, "");
            }
            else
            {
                // no match was found. Ask user for classification and store for next time
                Speech.say("I can not match your query to anything in my database.");

                return new KeyValuePair<Actions, string>(Actions.unknown, "");
            }

            throw new Exception("I was unable to find a match.");
        }
    }
}