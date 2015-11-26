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
using System.Threading.Tasks;
using System.IO;
using ExtensionMethods;
using System.Reflection;
using System.Windows.Forms;
using System.Speech.Synthesis;
using System.Threading;

namespace EchoServer
{
    class Program
    {
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
            KeyValuePair<QueryClassification.Actions, string> query = qc.Classify(input);

            ResponseTime responseTime = new ResponseTime();
            int responseTimeID = responseTime.Start(guid, query.Key, messageSystem);


            if (query.Key == QueryClassification.Actions.help)
            {
                messageSystem.Post(guid, Message.Type.output, qc.help);
            }
            else if (query.Key == QueryClassification.Actions.wikipedia)
            {
                input = input.Replace(query.Value, "").Trim();
                Wikipedia.Go(guid, messageSystem, input);
            }
            else if (query.Key == QueryClassification.Actions.wolframAlpha)
            {
                Wolfram.Go(guid, input, messageSystem);
            }
            else if (query.Key == QueryClassification.Actions.weather)
            {
                Weather.Go(guid, input, messageSystem);
            }
            else if (query.Key == QueryClassification.Actions.joke)
            {
                Jokes.Go(guid, input, messageSystem);
            }
            else if (query.Key == QueryClassification.Actions.unknown)
            {
                messageSystem.Post(guid, Message.Type.output, query.Value);
            }

            responseTime.Stop(query.Key, responseTimeID);
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
