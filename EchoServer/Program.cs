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

using Extensions;
using PluginContracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Threading;
using System.Threading.Tasks;

namespace EchoServer
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            ResponseTime responseTime = new ResponseTime();
            QueryClassification qc = new QueryClassification();
            Dictionary<string, IPlugin> _Plugins = LoadPlugins(qc);

            while (true)
            {
                Message message = SQL.GetNextMessage();

                if (message == null || message.messageID == -1)
                {
                    Thread.Sleep(10);
                    continue;
                }
                Task.Factory.StartNew(() =>
                {
                    if (string.IsNullOrEmpty(message.textRequest))
                    {
                        message.textRequest = SpeechToText.Process(message.audioRequest);
                    }


                    KeyValuePair<string, string> query = qc.Classify(message.textRequest);

                    int responseTimeID = responseTime.Start(query.Key);
                    string delaymsg = responseTime.GetDelayMessage(query.Key);
                    if (!string.IsNullOrWhiteSpace(delaymsg))
                    {
                        message.textResponse = delaymsg;
                        message.audioResponse = GetAudio(delaymsg);
                        message.status = Message.Status.delayed;
                        SQL.UpdateMessage(message);
                    }

                    if (query.Key == "help")
                    {
                        message.textResponse = query.Value;
                        message.audioResponse = GetAudio(query.Value);
                        message.status = Message.Status.ready;
                    }
                    else if (query.Key == "unknown")
                    {
                        message.textResponse = query.Value;
                        message.audioResponse = GetAudio(query.Value);
                        message.status = Message.Status.ready;
                    }
                    else if (query.Key == "blank")
                    {
                        message.status = Message.Status.error;
                    }
                    else if (_Plugins.ContainsKey(query.Key))
                    {
                        string request = message.textRequest.Replace(query.Value, "").CleanText();
                        request = request.Length > 0 ? request : message.textRequest;
                        IPlugin plugin = _Plugins[query.Key];
                        string response = plugin.Go(request);
                        message.textResponse = response;
                        message.audioResponse = GetAudio(response);
                        message.status = Message.Status.ready;
                    }
                    else
                    {
                        message.textResponse = query.Value;
                        message.audioResponse = GetAudio("A loaded plugin has failed to produce a valid key."); // very unlikely.
                        message.status = Message.Status.ready;
                    }

                    // post the message back to SQL.
                    message.audioRequest = new byte[0]; // remove request audio.
                    SQL.UpdateMessage(message);
                    responseTime.Stop(query.Key, responseTimeID);
                });
            }
        }

        private static Dictionary<string, IPlugin> LoadPlugins(QueryClassification qc)
        {
            Dictionary<string, IPlugin> _Plugins = new Dictionary<string, IPlugin>();
            string path = @"C:\Program Files\EchoServer\Plugins";
            ICollection<IPlugin> plugins = GenericPluginLoader<IPlugin>.LoadPlugins(path);

            if (plugins != null && plugins.Count() > 0)
            {
                foreach (var item in plugins)
                {
                    _Plugins.Add(item.Name, item);

                    foreach (string action in item.Actions)
                    {
                        qc.AddPhraseToAction(action, item.Name);
                    }
                }
            }
            return _Plugins;
        }

        private static byte[] GetAudio(string response)
        {
            byte[] buffer = new byte[0];

            if (!string.IsNullOrWhiteSpace(response))
            {
                var t = new System.Threading.Thread(() =>
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        SpeechSynthesizer synth = new SpeechSynthesizer();
                        synth.SetOutputToWaveStream(memoryStream);
                        synth.Speak(response);
                        buffer = memoryStream.ToArray();
                    }
                });
                t.Start();
                t.Join();
            }


            return buffer;
        }
    }

    public class Message
    {
        public int clientID = -1;
        public int messageID = -1;
        public string textRequest = string.Empty;
        public string textResponse = string.Empty;
        public byte[] audioRequest;
        public byte[] audioResponse = new byte[0];
        public DateTime postTime = new DateTime();
        public enum Status { queued, processing, delayed, ready, closed, error };
        public Status status = Status.queued;
    }
}
