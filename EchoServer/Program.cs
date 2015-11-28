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
using Extensions;
using System.Reflection;
using System.Windows.Forms;
using System.Speech.Synthesis;
using System.Threading;
using PluginContracts;
using System.Diagnostics;

namespace EchoServer
{
    public class Program
    {
        private MessageSystem messageSystem = new MessageSystem();
        private QueryClassification qc = new QueryClassification();
        private Dictionary<string, IPlugin> _Plugins;
        public Program()
        {

            try
            {
                _Plugins = new Dictionary<string, IPlugin>();
                string path = @"C:\Program Files (x86)\EchoServer\Plugins";
                ICollection<IPlugin> plugins = GenericPluginLoader<IPlugin>.LoadPlugins(path);
                foreach (var item in plugins)
                {
                    _Plugins.Add(item.Name, item);

                    List<string> actions = _Plugins[item.Name].Actions;

                    foreach (string action in actions)
                    {
                        qc.AddPhraseToAction(action, item.Name);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            
        }

        public byte[] Go(Guid guid, string input)
        {
            input = input == string.Empty ? "help" : input;
            input = input.CleanText();

            string updateOnly = "updateupdate";
            bool updateRequest = input == updateOnly;

            if (updateRequest)
            {
                string response = messageSystem.Get(guid);
                if (response != string.Empty)
                {
                    return GetAudio(response);
                }
            }
            else
            {
                ProcessInput(guid, input, messageSystem);
            }

            return updateOnly.GetBytes();
        }

        private void ProcessInput(Guid guid, string input, MessageSystem messageSystem)
        {
            
            KeyValuePair<string, string> query = qc.Classify(input);

            ResponseTime responseTime = new ResponseTime();
            int responseTimeID = responseTime.Start(guid, query.Key, messageSystem);

            foreach (var plugin in _Plugins)
            {
                if (query.Key == plugin.Key)
                {
                    input = input.CleanText();
                    messageSystem.Post(guid, plugin.Value.Go(input));
                }
            }

            if (query.Key == "help")
            {
                messageSystem.Post(guid, qc.help);
            }
            else if (query.Key == "wolframAlpha")
            {
                Wolfram.Go(guid, input, messageSystem);
            }
            else if (query.Key == "unknown")
            {
                messageSystem.Post(guid, query.Value);
            }

            responseTime.Stop(query.Key, responseTimeID);
        }

        private static byte[] GetAudio(string response)
        {
            byte[] wav = new byte[0];

            if (!string.IsNullOrWhiteSpace(response))
            {
                var t = new System.Threading.Thread(() =>
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        SpeechSynthesizer synth = new SpeechSynthesizer();
                        synth.SetOutputToWaveStream(memoryStream);
                        synth.Speak(response.CleanText());
                        wav = memoryStream.ToArray();
                    }
                });
                t.Start();
                t.Join();
            }

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
