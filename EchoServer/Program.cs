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
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json;

namespace EchoServer
{
    public class Program
    {
        private QueryClassification qc = new QueryClassification();
        private Dictionary<string, IPlugin> _Plugins;
        private ResponseTime responseTime = new ResponseTime();

        public Program(MessageSystem messageSystem)
        {

            try
            {
                _Plugins = new Dictionary<string, IPlugin>();
                string path = @"C:\Program Files\EchoServer\Plugins";
                ICollection<IPlugin> plugins = GenericPluginLoader<IPlugin>.LoadPlugins(path);


                if (plugins != null && plugins.Count() > 0)
                {
                    foreach (var item in plugins)
                    {
                        try
                        {
                            _Plugins.Add(item.Name, item);

                            //List<string> actions = _Plugins[item.Name].Actions;

                            foreach (string action in item.Actions)
                            {
                                qc.AddPhraseToAction(action, item.Name);
                            }
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message);
                        }
                    }
                }
                else
                {
                    _Plugins.Add("help", null);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    try
                    {
                        Message message = messageSystem.GetNextMessage();

                        if (message == null || message.messageID == Guid.Empty)
                        {
                            Thread.Sleep(10);
                            continue;
                        }


                        MemoryStream ms = new MemoryStream(message.audioRequest);
                        byte[] buffer;
                        try
                        {
                            BinaryFormatter formatter = new BinaryFormatter();

                            // Deserialize the hashtable from the file and 
                            // assign the reference to the local variable.
                            buffer = (byte[])formatter.Deserialize(ms);
                        }
                        catch (SerializationException e)
                        {
                            Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
                            throw;
                        }
                        finally
                        {
                            ms.Close();
                        }
                        //using (FileStream fs = new FileStream(@"Z:\OpenEcho\req1.wav", FileMode.Create))
                        //{
                        //    WriteHeader(fs, message.audioRequest.Length, 1, 44100);
                        //    fs.Write(message.audioRequest, 0, message.audioRequest.Length);
                        //    fs.Close();
                        //}

                        //var soundData = buffer;//CreateSinWave(44000, 120, TimeSpan.FromSeconds(60), 1d);
                        using (MemoryStream msg = new MemoryStream())
                        {
                            WriteHeader(msg, buffer.Length, 1, 44100);
                            msg.Write(buffer, 0, buffer.Length);
                            msg.Close();
                            //message.textRequest = GetText(msg.GetBuffer());


                            string jsons = message.textRequest.Split('\n')[1];
                            SpeechResponse m = JsonConvert.DeserializeObject<SpeechResponse>(jsons);
                        }

                        KeyValuePair<string, string> query = qc.Classify(message.textRequest);

                        int responseTimeID = responseTime.Start(query.Key);
                        string delaymsg = responseTime.GetDelayMessage(query.Key);
                        if (!string.IsNullOrWhiteSpace(delaymsg))
                        {
                            message.textResponse = delaymsg;
                            message.audioResponse = GetAudio(delaymsg);
                            message.status = Message.Status.delayed;
                        }

                        if (query.Key == "help")
                        {
                            message.textResponse = query.Value;
                            message.status = Message.Status.ready;
                        }
                        else if (query.Key == "unknown")
                        {
                            message.textResponse = query.Value;
                            message.status = Message.Status.ready;
                        }
                        else
                        {
                            foreach (var plugin in _Plugins)
                            {
                                if (query.Key == plugin.Key)
                                {
                                    
                                    
                                    string response = plugin.Value.Go(message.textRequest);

                                    message.textResponse = response;
                                    message.audioResponse = GetAudio(response);
                                    message.status = Message.Status.ready;
                                }

                            } 
                        }

                        // post the message back to SQL.
                        SQL.UpdateMessage(message);
                        responseTime.Stop(query.Key, responseTimeID);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            });
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
                        synth.Speak(response);
                        wav = memoryStream.ToArray();
                    }
                });
                t.Start();
                t.Join();
            }

            return wav;
        }

        private static string GetText(byte[] audio)
        {

            HttpWebRequest _HWR_SpeechToText = null;
            _HWR_SpeechToText =
                        (HttpWebRequest)HttpWebRequest.Create(
                            "https://www.google.com/speech-api/v2/recognize?output=json&lang=en-us&key=AIzaSyBFladhupiqR95oCdyNczmwP2qvu234qt4");
            _HWR_SpeechToText.Credentials = CredentialCache.DefaultCredentials;
            _HWR_SpeechToText.Method = "POST";
            _HWR_SpeechToText.ContentType = "audio/l16; rate=44100";
            _HWR_SpeechToText.ContentLength = audio.Length;
            Stream stream = _HWR_SpeechToText.GetRequestStream();
            stream.Write(audio, 0, audio.Length);
            stream.Close();

            HttpWebResponse HWR_Response = (HttpWebResponse)_HWR_SpeechToText.GetResponse();
            if (HWR_Response.StatusCode == HttpStatusCode.OK)
            {
                StreamReader SR_Response = new StreamReader(HWR_Response.GetResponseStream());
                return SR_Response.ReadToEnd();
            }
            return "";
        }

        static byte[] RIFF_HEADER = new byte[] { 0x52, 0x49, 0x46, 0x46 };
        static byte[] FORMAT_WAVE = new byte[] { 0x57, 0x41, 0x56, 0x45 };
        static byte[] FORMAT_TAG = new byte[] { 0x66, 0x6d, 0x74, 0x20 };
        static byte[] AUDIO_FORMAT = new byte[] { 0x01, 0x00 };
        static byte[] SUBCHUNK_ID = new byte[] { 0x64, 0x61, 0x74, 0x61 };
        private const int BYTES_PER_SAMPLE = 2;

        public static void WriteHeader(
             System.IO.Stream targetStream,
             int byteStreamSize,
             int channelCount,
             int sampleRate)
        {

            int byteRate = sampleRate * channelCount * BYTES_PER_SAMPLE;
            int blockAlign = channelCount * BYTES_PER_SAMPLE;

            targetStream.Write(RIFF_HEADER, 0, RIFF_HEADER.Length);
            targetStream.Write(PackageInt(byteStreamSize + 42, 4), 0, 4);

            targetStream.Write(FORMAT_WAVE, 0, FORMAT_WAVE.Length);
            targetStream.Write(FORMAT_TAG, 0, FORMAT_TAG.Length);
            targetStream.Write(PackageInt(16, 4), 0, 4);//Subchunk1Size    

            targetStream.Write(AUDIO_FORMAT, 0, AUDIO_FORMAT.Length);//AudioFormat   
            targetStream.Write(PackageInt(channelCount, 2), 0, 2);
            targetStream.Write(PackageInt(sampleRate, 4), 0, 4);
            targetStream.Write(PackageInt(byteRate, 4), 0, 4);
            targetStream.Write(PackageInt(blockAlign, 2), 0, 2);
            targetStream.Write(PackageInt(BYTES_PER_SAMPLE * 8), 0, 2);
            //targetStream.Write(PackageInt(0,2), 0, 2);//Extra param size
            targetStream.Write(SUBCHUNK_ID, 0, SUBCHUNK_ID.Length);
            targetStream.Write(PackageInt(byteStreamSize, 4), 0, 4);
        }

        static byte[] PackageInt(int source, int length = 2)
        {
            if ((length != 2) && (length != 4))
                throw new ArgumentException("length must be either 2 or 4", "length");
            var retVal = new byte[length];
            retVal[0] = (byte)(source & 0xFF);
            retVal[1] = (byte)((source >> 8) & 0xFF);
            if (length == 4)
            {
                retVal[2] = (byte)((source >> 0x10) & 0xFF);
                retVal[3] = (byte)((source >> 0x18) & 0xFF);
            }
            return retVal;
        }

        public class SpeechAlternative
        {
            public string Transcript { get; set; }
            public double Confidence { get; set; }
        }

        public class SpeechResult
        {
            public SpeechAlternative[] Alternative { get; set; }
            public bool Final { get; set; }
        }

        public class SpeechResponse
        {
            public SpeechResult[] Result { get; set; }
            public int Result_Index { get; set; }
        }
    }
}
