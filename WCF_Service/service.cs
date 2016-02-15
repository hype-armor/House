
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.

using System;
using System.Data.SqlClient;
using System.ServiceModel;
using System.Text;
using Extensions;

namespace Microsoft.Samples.GettingStarted
{
    // Define a service contract.
    [ServiceContract(Namespace="http://Microsoft.Samples.GettingStarted")]

    public interface Echo
    {
        [OperationContract]
        DateTime Post(Guid n1, byte[] n2);
        [OperationContract]
        byte[] Get(Guid n1);
    }

    // Service class which implements the service contract.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Single)]
    public class EchoService : Echo
    {
        public DateTime Post(Guid guid, byte[] data)
        {
            SQL.AddClient(guid);
            SQL.CreateRequest(Guid.NewGuid(), guid, "", "", data, new byte[0], DateTime.Now, 0);
            return DateTime.Now;
        }

        public byte[] Get(Guid guid)
        {
            byte[] audio = SQL.GetResponse(guid);

            return audio;
        }
    }

    public class SQL
    {
        private static string connectionString = "Server=SKYNET\\SQLEXPRESS;Database=Echo;Trusted_Connection=True;";
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
                    // all is well
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
                    "INSERT INTO Messages VALUES(@MessageID, @ClientID, @textRequest, @textResponse, @request, @response, @PostTime, @Status)", con))
                    {
                        command.Parameters.Add(new SqlParameter("MessageID", messageID));
                        command.Parameters.Add(new SqlParameter("ClientID", clientID));
                        command.Parameters.Add(new SqlParameter("textRequest", request));
                        command.Parameters.Add(new SqlParameter("textResponse", response));
                        command.Parameters.Add(new SqlParameter("request", audioRequest));
                        command.Parameters.Add(new SqlParameter("response", AudioResponse));
                        command.Parameters.Add(new SqlParameter("PostTime", PostTime));
                        command.Parameters.Add(new SqlParameter("Status", status));
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

                using (SqlCommand command = new SqlCommand("SELECT [MessageID], [response], [Status] FROM Messages " +
                    "WHERE [ClientID]=@ClientID AND ([Status]=2 OR [Status]=3)", con))
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

                if (audioResponse.Length == 0)
                {
                    return new byte[0];
                }

                con.Open();
                status = 4;
                using (SqlCommand command = new SqlCommand(
                    "UPDATE [Messages] " +
                    "SET [Status]=@Status " +
                    "WHERE [MessageID]=@MessageID", con))
                {
                    command.Parameters.Add(new SqlParameter("Status", 0));
                    command.Parameters.Add(new SqlParameter("MessageID", MessageID));
                    command.ExecuteNonQuery();
                }
                return audioResponse;
            }
        }
    }
}
