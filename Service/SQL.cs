using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Extensions;

public class SQL
{
    private static string connectionString = "Server=SKYNET\\SQLEXPRESS;Database=Echo;User Id=client;Password=echo;";
    public static int AddClient(string username, string password)
    {
        int id = int.MinValue;
        using (SqlConnection con = new SqlConnection(connectionString))
        {
            con.Open();
            using (SqlCommand command = new SqlCommand("INSERT INTO [dbo].[Clients] (username, password) VALUES(@username, @password); SELECT @@IDENTITY AS ClientID", con))
            {
                command.Parameters.Add(new SqlParameter("username", username));
                command.Parameters.Add(new SqlParameter("password", password));
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    id = reader["ClientID"].ToInt();
                }
            }
        }
        return id;
    }

    public static void CreateRequest(int clientID, byte[] audioRequest, byte[] AudioResponse, DateTime PostTime, int status)
    {
        using (SqlConnection con = new SqlConnection(connectionString))
        {
            con.Open();
            try
            {
                using (SqlCommand command = new SqlCommand(
                "INSERT INTO Messages VALUES(@ClientID, @textRequest, @textResponse, @request, @response, @PostTime, @Status, @LastUpdated)", con))
                {
                    command.Parameters.Add(new SqlParameter("ClientID", clientID));
                    command.Parameters.Add(new SqlParameter("textRequest", ""));
                    command.Parameters.Add(new SqlParameter("textResponse", ""));
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

    public static byte[] GetResponse(int ClientID)
    {
        using (SqlConnection con = new SqlConnection(connectionString))
        {
            con.Open();

            int MessageID = -1;
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
                    MessageID = reader["MessageID"].ToInt();
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

    private static void UpdateMessageWithStatus(int MessageID, int status)
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
