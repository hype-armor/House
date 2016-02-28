
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.

using System;
using System.Data.SqlClient;
using System.ServiceModel;
using System.Text;
using Extensions;
using System.IO;

namespace Microsoft.Samples.GettingStarted
{
    // Define a service contract.
    [ServiceContract(Namespace="http://Microsoft.Samples.GettingStarted")]

    public interface Echo
    {
        [OperationContract]
        DateTime Post(Guid ClientID, MemoryStream audioStream);
        [OperationContract]
        MemoryStream Get(Guid ClientID);
        [OperationContract]
        bool GetUpdateStatus(Guid ClientID);
    }

    // Service class which implements the service contract.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Single)]
    public class EchoService : Echo
    {
        public DateTime Post(Guid ClientID, MemoryStream audioStream)
        {
            audioStream.Position = 0L;
            byte[] data = audioStream.GetBuffer();
            SQL.AddClient(ClientID);
            SQL.CreateRequest(Guid.NewGuid(), ClientID, "", "", data, new byte[0], DateTime.Now, 0);
            return DateTime.Now;
        }

        public MemoryStream Get(Guid ClientID)
        {
            byte[] audio = SQL.GetResponse(ClientID);
            MemoryStream audioStream = new MemoryStream(audio, 0, audio.Length, true, true);
            audioStream.Position = 0L;
            return audioStream;
        }

        public bool GetUpdateStatus(Guid ClientID)
        {
            throw new NotImplementedException();
        }
    }

    public class SQL
    {
        private static string connectionString = "Server=SKYNET\\SQLEXPRESS;Database=Echo;User Id=client;Password=echo;";
        public static void AddClient(Guid guid)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand(
                    "INSERT INTO Clients VALUES(@ClientID)", con))
                    {
                        command.Parameters.Add(new SqlParameter("ClientID", guid));
                        command.ExecuteNonQuery();
                    }
                }
                catch (SqlException se)
                {
                    return; // clientID is already in DB.
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        public static void CreateRequest(Guid messageID, Guid clientID, string request, string response, byte[] audioRequest, byte[] AudioResponse, DateTime PostTime, int status)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand(
                    "INSERT INTO Messages VALUES(@MessageID, @ClientID, @textRequest, @textResponse, @request, @response, @PostTime, @Status, @LastUpdated)", con))
                    {
                        command.Parameters.Add(new SqlParameter("MessageID", messageID));
                        command.Parameters.Add(new SqlParameter("ClientID", clientID));
                        command.Parameters.Add(new SqlParameter("textRequest", request));
                        command.Parameters.Add(new SqlParameter("textResponse", response));
                        command.Parameters.Add(new SqlParameter("request", audioRequest));
                        command.Parameters.Add(new SqlParameter("response", AudioResponse));
                        command.Parameters.Add(new SqlParameter("PostTime", PostTime));
                        command.Parameters.Add(new SqlParameter("Status", status));
                        command.Parameters.Add(new SqlParameter("LastUpdated", DateTime.Now));
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        public static byte[] GetResponse(Guid ClientID)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                
                Guid MessageID = Guid.Empty;
                byte[] audioResponse = new byte[0];
                int status = 0;


                // removed [ClientID]=@ClientID AND for testing from the where.
                using (SqlCommand command = new SqlCommand("SELECT TOP 1 [MessageID], [response], [Status] FROM Messages " +
                    "WHERE ([Status]=2 OR [Status]=3) ORDER BY [PostTime] ASC", con))
                {
                    command.Parameters.Add(new SqlParameter("ClientID", ClientID));

                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        MessageID = Guid.Parse(reader["MessageID"].ToString());
                        audioResponse = reader["response"].ToByteArray();
                        status = reader["Status"].ToInt();
                    }
                }
                con.Close();

                if (audioResponse.Length == 10)
                {
                    // no audio response. Return here so we do not update the message status.
                    return new byte[0];
                }

                if (status == 3) // completed message
                {
                    status = 4;
                    UpdateMessageWithStatus(MessageID, status);
                }
                
                return audioResponse;
            }
        }

        private static void UpdateMessageWithStatus(Guid MessageID, int status)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(
                                "UPDATE [Messages] " +
                                "SET [Status]=@Status " +
                                "WHERE [MessageID]=@MessageID", con))
                {
                    command.Parameters.Add(new SqlParameter("Status", status));
                    command.Parameters.Add(new SqlParameter("MessageID", MessageID));
                    command.ExecuteNonQuery();
                }
                con.Close();
            }
        }
    }
}
