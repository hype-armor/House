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
using Extensions;
using System.Net;
using System.IO;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Threading;
using System.Speech.AudioFormat;
using System.Data.SqlClient;

namespace EchoServer
{
    public class MessageSystem
    {
        internal int workers = 0;
        private List<Message> queue = new List<Message>();
        public int messageCount
        {
            get
            {
                return (from Message in queue
                        orderby Message.postTime
                        where Message.status == Message.Status.ready
                        select Message).Count();
            }
        }
        public enum Status { queued, processing, delayed, ready, closed, error };
        public void CreateRequest(Guid ClientGuid, byte[] Request) // called from client
        {
            SQL.AddClient(ClientGuid);
            SQL.AddMessage(Guid.NewGuid(), ClientGuid, "", "", Request, new byte[0], DateTime.Now, (int)Status.queued);
        }

        public Message GetNextMessage() // called from server
        {
            // also mark as processing.
            return SQL.GetNextMessage();
        }

        public Message GetResponse(Guid ClientGuid) // called from client
        {

            Message m = SQL.GetMessage(ClientGuid);

            if (m.messageID == Guid.Empty)
            {
                return null;
            }

            return m;
        }
    }

    public class Message
    {
        public Guid clientID = Guid.Empty;
        public Guid messageID = Guid.Empty;
        public string textRequest = string.Empty;
        public string textResponse = string.Empty;
        public byte[] audioRequest;
        public byte[] audioResponse = new byte[0];
        public DateTime postTime = new DateTime();
        public enum Status { queued, processing, delayed, ready, closed, error };
        public Status status = Status.queued;
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

        public static void AddMessage(Guid messageID, Guid clientID, string request, string response, byte[] audioRequest, byte[] AudioResponse, DateTime PostTime, int status)
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

        public static Message GetMessage(Guid ClientID)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                Message m = new Message();
                using (SqlCommand command = new SqlCommand("SELECT * FROM Messages " +
                    "WHERE [ClientID]=@ClientID AND ([Status]=2 OR [Status]=3)", con))
                {
                    command.Parameters.Add(new SqlParameter("ClientID", ClientID));

                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        m.messageID = Guid.Parse(reader["MessageID"].ToString());
                        m.clientID = Guid.Parse(reader["ClientID"].ToString());
                        m.textRequest = reader["textRequest"].ToString();
                        m.textResponse = reader["textResponse"].ToString();
                        m.audioRequest = reader["request"].ToByteArray();
                        m.audioResponse = reader["response"].ToByteArray();
                        m.postTime = reader["PostTime"].ToDateTime();
                        m.status = (Message.Status)reader["Status"].ToInt();
                    }
                }
                con.Close();

                if (m.clientID == Guid.Empty)
                {
                    return m;
                }

                con.Open();
                m.status = Message.Status.closed;
                using (SqlCommand command = new SqlCommand(
                    "UPDATE [Messages] " +
                    "SET [Status]=@Status " +
                    "WHERE [MessageID]=@MessageID", con))
                {
                    command.Parameters.Add(new SqlParameter("Status", m.status));
                    command.Parameters.Add(new SqlParameter("MessageID", m.messageID));
                    command.ExecuteNonQuery();
                }
                return m;
            }
        }

        public static Message GetNextMessage()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                Guid msgGuid = Guid.Empty;
                using (SqlCommand command = new SqlCommand("DECLARE @msg uniqueidentifier; " +
                    "SELECT TOP 1 @msg = [MessageID] " +
                    "FROM Messages " +
                    "WHERE [Status] = 0; " +
                    "SELECT @msg AS [MessageID]; " +
                    "UPDATE [Messages] " +
                    "SET [Status] = 1 " +
                    "WHERE [MessageID] = @msg; ", con))
                {
                    Message m = new Message();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        if (!string.IsNullOrWhiteSpace(reader["MessageID"].ToString()))
                        {
                            m.messageID = Guid.Parse(reader["MessageID"].ToString());
                        }
                        else
                        {
                            return m;
                        }
                    }
                    msgGuid = m.messageID;
                }
                con.Close();

                con.Open();
                using (SqlCommand command = new SqlCommand("SELECT [MessageID],[ClientID],[textRequest], " + 
                    "[textResponse],[request],[response],[PostTime],[Status]" +
                    " FROM Messages " + "WHERE [MessageID]='" + msgGuid.ToString() + "'", con))
                {
                    Message m = new Message();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        m.messageID = Guid.Parse(reader["MessageID"].ToString());
                        m.clientID = Guid.Parse(reader["ClientID"].ToString());
                        m.textRequest = reader["textRequest"].ToString();
                        m.textResponse = reader["textResponse"].ToString();
                        m.audioRequest = reader["request"].ToByteArray();
                        m.audioResponse = reader["response"].ToByteArray();
                        m.postTime = reader["PostTime"].ToDateTime();
                        m.status = (Message.Status)reader["Status"].ToInt();
                    }
                    return m;
                }
            }
        }

        public static void UpdateMessage(Message msg)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                using (SqlCommand command = new SqlCommand(
                    "UPDATE [Messages] " +
                    "SET [textRequest]=@textRequest,[textResponse]=@textResponse,[request]=@request, " +
                    "[response]=@response,[Status]=@Status " +
                    "WHERE [MessageID]=@MessageID", con))
                {
                    command.Parameters.Add(new SqlParameter("textRequest", msg.textRequest));
                    command.Parameters.Add(new SqlParameter("textResponse", msg.textResponse));
                    command.Parameters.Add(new SqlParameter("request", msg.audioRequest));
                    command.Parameters.Add(new SqlParameter("response", msg.audioResponse));
                    command.Parameters.Add(new SqlParameter("Status", msg.status));
                    command.Parameters.Add(new SqlParameter("MessageID", msg.messageID));
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
