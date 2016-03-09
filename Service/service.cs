
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.

using System;
using System.Data.SqlClient;
using System.IO;
using System.ServiceModel;

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
        void Post(int ClientID, MemoryStream audioStream);
    }

    // The callback interface is used to send messages from service back to client.
    // The Result operation will return the current result after each operation.
    // The Equation opertion will return the complete equation after Clear() is called.
    public interface IEchoDuplexCallback
    {
        [OperationContract(IsOneWay = true)]
        void Result(MemoryStream result);
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
            clientID = SQL.AddClient("greg", "password");
        }

        public void Post(int ClientID, MemoryStream audioStream)
        {
            audioStream.Position = 0L;
            byte[] data = audioStream.GetBuffer();
            SQL.CreateRequest(ClientID, "", "", data, new byte[0], DateTime.Now, 0);
        }

        public void Get(int ClientID)
        {
            byte[] audio = SQL.GetResponse(ClientID);
            MemoryStream audioStream = new MemoryStream(audio, 0, audio.Length, true, true);
            audioStream.Position = 0L;
            Callback.Result(audioStream);
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
