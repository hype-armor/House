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
        private Guid ClientID = Guid.Empty;
        public EchoWCF(Guid clientID)
        {
            ClientID = clientID;
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
    }
}
