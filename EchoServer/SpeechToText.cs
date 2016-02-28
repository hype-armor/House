using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace EchoServer
{
    class SpeechToText
    {
        public static string Process(byte[] audioRequest)
        {
            byte[] buffer = Deserialize(audioRequest);

            using (MemoryStream msg = new MemoryStream())
            {
                buffer = AddWavHeader(buffer);
                string text = GSpeechToText(buffer);

                if (text.Length > 0)
                {
                    string[] requestPosibilities = text.Split('\n');
                    if (requestPosibilities.Length > 0)
                    {
                        string RequestJson = requestPosibilities[1];
                        SpeechResponse m = JsonConvert.DeserializeObject<SpeechResponse>(RequestJson);
                        if (m != null)
                        {
                            return m.Result.First().Alternative.First().Transcript;
                        }
                    }
                }
            }
            return "";
        }

        private static byte[] AddWavHeader(byte[] buffer)
        {
            using (MemoryStream msg = new MemoryStream())
            {
                WriteHeader(msg, buffer.Length, 1, 44100);
                msg.Write(buffer, 0, buffer.Length);
                msg.Close();
                return msg.GetBuffer();
            }
        }

        private static byte[] Deserialize(byte[] audioRequest)
        {
            MemoryStream ms = new MemoryStream(audioRequest);
            byte[] buffer;

            BinaryFormatter formatter = new BinaryFormatter();

            // Deserialize the hashtable from the file and 
            // assign the reference to the local variable.
            buffer = (byte[])formatter.Deserialize(ms);

            ms.Close();
            return buffer;
        }

        private static string GSpeechToText(byte[] audio)
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
            try
            {
                stream.Write(audio, 0, audio.Length);
                stream.Close();
                HttpWebResponse HWR_Response = (HttpWebResponse)_HWR_SpeechToText.GetResponse();
                if (HWR_Response.StatusCode == HttpStatusCode.OK)
                {
                    StreamReader SR_Response = new StreamReader(HWR_Response.GetResponseStream());
                    return SR_Response.ReadToEnd();
                }
            }
            catch (System.IO.IOException ie)
            {
                // likely out of quota for the day.
            }
            catch (System.Net.WebException we)
            {
                // 403?
                if (we.Message.Contains("403"))
                {
                    // forbidden.
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                stream.Close();
            }
            return "";
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
    }
}
