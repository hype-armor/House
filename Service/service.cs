
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.

using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace EchoWCFDuplex
{
    // Define a duplex service contract.
    // A duplex contract consists of two interfaces.
    // The primary interface is used to send messages from the client to the service.
    // The callback interface is used to send messages from the service back to the client.
    // ICalculatorDuplex allows one to perform multiple operations on a running result.
    // The result is sent back after each operation on the ICalculatorCallback interface.
    [ServiceContract(Namespace = "http://Echo.Duplex", SessionMode=SessionMode.Required,
                     CallbackContract=typeof(IEchoDuplexCallback))]
    public interface EchoDuplex
    {
        [OperationContract(IsOneWay = true)]
        void Post(MemoryStream audioStream);
    }

    // The callback interface is used to send messages from service back to client.
    // The Result operation will return the current result after each operation.
    // The Equation opertion will return the complete equation after Clear() is called.
    public interface IEchoDuplexCallback
    {
        [OperationContract(IsOneWay = true)]
        void Push(MemoryStream result);
    }

    // Service class which implements a duplex service contract.
    // Use an InstanceContextMode of PerSession to store the result
    // An instance of the service will be bound to each duplex session
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class Service : EchoDuplex
    {
        int clientID = -1;
        public Service()
        {
            clientID = SQL.AddClient("greg1", "password");

            Task.Factory.StartNew(() => 
            {
                while (true)
                {
                    Thread.Sleep(1000);
                    byte[] audio = SQL.GetResponse(clientID);
                    if (audio.Length > 0)
                    {
                        MemoryStream audioStream = new MemoryStream(audio, 0, audio.Length, true, true);
                        audioStream.Position = 0L;
                        Callback.Push(audioStream);
                    }
                    
                    //break;
                }
                //return;
            });
        }

        public void Post(MemoryStream audioStream)
        {
            audioStream.Position = 0L;
            try
            {
                byte[] data = audioStream.GetBuffer();
                SQL.CreateRequest(clientID, data, new byte[0], DateTime.Now, 0);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                throw;
            }
        }

        IEchoDuplexCallback Callback
        {
            get
            {
                return OperationContext.Current.GetCallbackChannel<IEchoDuplexCallback>();
            }
        }
    }
}
