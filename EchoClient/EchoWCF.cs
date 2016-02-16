using System;
using System.Collections.Generic;
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
            return client.Post(ClientID, audio);
        }

        public byte[] Get()
        {
            return client.Get(ClientID);
        }
    }
}
