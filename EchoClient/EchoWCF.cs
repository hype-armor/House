using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EchoClient
{
    public class EchoWCF
    {
        EchoWCFService.EchoClient client = new EchoWCFService.EchoClient();
        private int ClientID = -1;
        public EchoWCF(string username, string password)
        {
            if ((int)Properties.Settings.Default["ClientID"] == -1)
            {
                ClientID = CreateProfile(username, password);
                Properties.Settings.Default["ClientID"] = ClientID;
                Properties.Settings.Default.Save();
            }
            else
            {
                ClientID = (int)Properties.Settings.Default["ClientID"];
            }
        }

        public DateTime Post(byte[] audio)
        {
            MemoryStream audioStream = new MemoryStream(audio, 0, audio.Length, true, true);
            audioStream.Position = 0L;
            return client.Post(ClientID, audioStream);
        }

        public byte[] Get()
        {
            MemoryStream audioStream = client.Get(ClientID);
            return audioStream.GetBuffer();
        }

        public int CreateProfile(string username, string password)
        {
            return client.CreateProfile(username, password);
        }
    }
}
