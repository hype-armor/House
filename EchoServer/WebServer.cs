// The code in this file is from http://www.codeproject.com/Articles/742260/Simple-Web-Server-in-csharp
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
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace EchoServer
{
    // State object for reading client data asynchronously
    public class StateObject
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
        // store data as byte array
        public byte[] dataBuffer = new byte[0];
        // Guid 
        public Guid guid = Guid.Empty;
    }

    public class WebServer
    {
        // Thread signal.
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        public static bool run = true;
        private static MessageSystem messageSystem = new MessageSystem();
        private Program program = new Program(messageSystem);

        public void StartListening()
        {
            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.
            // The DNS name of the computer
            IPHostEntry ipHostInfo = Dns.GetHostEntry("sky.ibang.us");
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 8080);

            // Create a TCP/IP socket.
            Socket listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (run)
                {
                    // Set the event to nonsignaled state.
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.
                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);

                    // Wait until a connection is made before continuing.
                    allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }

        public void StopListenting()
        {
            run = false;
        }

        public void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            allDone.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        public void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket. 
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.
                state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read 
                // more data.
                content = state.sb.ToString();
                state.dataBuffer = Combine(state.dataBuffer, state.buffer);

                if (content.IndexOf("<EOF>") > -1)
                {
                    byte[] tGuid = state.dataBuffer.Take(36).ToArray();
                    string _guid = Encoding.ASCII.GetString(tGuid);
                    state.guid = Guid.Parse(_guid);
                    state.dataBuffer = state.dataBuffer.Skip(36).Take(content.IndexOf("<EOF>")).ToArray();
                    Send(handler, state.guid, state.dataBuffer);
                }
                else
                {
                    // Not all data received. Get more.
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
            }
        }

        private void Send(Socket handler, Guid guid, byte[] data)
        {
            //check if there is the update tag.
            if (data.Length > 44) // guid + update + EOF
            {
                data = data.Take(data.Length - 5).ToArray(); // remove <EOF>
                messageSystem.CreateRequest(guid, data);
                data = Encoding.ASCII.GetBytes("Query was posted at " + DateTime.Now.ToShortTimeString());
            }
            else
            {
                Message m = messageSystem.GetResponse(guid);

                if (m != null)
                {
                    data = Encoding.ASCII.GetBytes(m.textResponse);
                }
                else
                {
                    data = Encoding.ASCII.GetBytes("Total messages: " + messageSystem.messageCount + " at " + DateTime.Now.ToShortTimeString());
                }
            }

            // Begin sending the data to the remote device.
            handler.BeginSend(data, 0, data.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        static public byte[] Combine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }
    } 
}