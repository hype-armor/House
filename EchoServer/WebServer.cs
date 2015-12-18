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

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Extensions;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Forms;
using System.ServiceProcess;
using System.Diagnostics;

namespace EchoServer
{
    public class WebServer
    {
        // check for already running
        private bool _running = false;
        private int _timeout = 5;
        private Encoding _charEncoder = Encoding.UTF8;
        private Socket _serverSocket;
        private static MessageSystem messageSystem = new MessageSystem();

        // Directory to host our contents
        private string _contentPath;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>

        [STAThread]
        static void Main()
        {
            if (Debugger.IsAttached)
            {
                // there is a file in c temp for testing

                //for (int i = 0; i < 2; i++)
                //{
                //    Guid TestClientGuid = Guid.NewGuid();
                //    messageSystem.CreateRequest(TestClientGuid, "forcast");
                //
                //    Program program = new Program(messageSystem);
                //    while (true)
                //    {
                //        Message response = messageSystem.GetResponse(TestClientGuid);
                //
                //        if (response == null)
                //        {
                //            continue;
                //        }
                //        else if (response.status == Message.Status.closed)
                //        {
                //            //Debugger.Break();
                //            break;
                //        }
                //        else if (response.status == Message.Status.processing)
                //        {
                //            Debugger.Break();
                //        }
                //        else if (response.status == Message.Status.ready)
                //        {
                //            Debugger.Break();
                //        }
                //    } 
                //}
            }
            else if (Environment.UserInteractive)
            {

                MessageBox.Show("This application is a service. Please install it.", "OpenEcho", MessageBoxButtons.OK, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                //ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
                //ManagedInstallerClass.InstallHelper(new string[] {Assembly.GetExecutingAssembly().Location });
            }
            else
            {
                ServiceBase.Run(new Echo());
            }
        }

        //create socket and initialization
        private void InitializeSocket(IPAddress ipAddress, int port, string contentPath) //create socket
        {
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _serverSocket.Bind(new IPEndPoint(ipAddress, port));
            _serverSocket.Listen(10);    //no of request in queue
            _serverSocket.ReceiveTimeout = _timeout;
            _serverSocket.SendTimeout = _timeout;
            _running = true; //socket created
            _contentPath = contentPath;
        }
        public void Start(IPAddress ipAddress, int port, string contentPath)
        {
            InitializeSocket(ipAddress, port, contentPath);

            while (_running)
            {
                var requestHandler = new RequestHandler(_serverSocket, contentPath);
                requestHandler.messageSystem = messageSystem;
                requestHandler.AcceptRequest();
            }

        }
        public void Stop()
        {
            _running = false;
            try
            {
                _serverSocket.Close();
            }
            catch
            {
                MessageBox.Show("Error in closing server or server already closed");

            }
            _serverSocket = null;
        }

    }

    class RequestHandler
    {
        private Socket _serverSocket;
        private int _timeout;
        private string _contentPath;
        private Encoding _charEncoder = Encoding.UTF8;
        public MessageSystem messageSystem;

        public RequestHandler(Socket serverSocket, String contentPath)
        {
            _serverSocket = serverSocket;
            _timeout = 5;
            _contentPath = contentPath;
        }

        public void AcceptRequest()
        {
            Socket clientSocket = null;

            // Create new thread to handle the request and continue to listen the socket.
            clientSocket = _serverSocket.Accept();

            var requestHandler = new Thread(() =>
            {
                clientSocket.ReceiveTimeout = _timeout;
                clientSocket.SendTimeout = _timeout;
                HandleTheRequest(clientSocket);
            });
            requestHandler.Start();
        }

        private void HandleTheRequest(Socket clientSocket)
        {
            var requestParser = new RequestParser();
            string requestString = DecodeRequest(clientSocket);
            requestParser.Parser(requestString);

            if (requestParser.HttpMethod != null && requestParser.HttpMethod.Equals("get", StringComparison.InvariantCultureIgnoreCase))
            {
                var createResponse = new CreateResponse(clientSocket, _contentPath);
                createResponse.messageSystem = messageSystem;
                createResponse.Get(requestParser.queryString);
            }
            else if (requestParser.HttpMethod != null && requestParser.HttpMethod.Equals("post", StringComparison.InvariantCultureIgnoreCase))
            {
                var createResponse = new CreateResponse(clientSocket, _contentPath);
                createResponse.messageSystem = messageSystem;
                createResponse.Post(requestParser.queryString);
            }
            StopClientSocket(clientSocket);
        }

        public void StopClientSocket(Socket clientSocket)
        {
            if (clientSocket != null)
                clientSocket.Close();
        }

        private string DecodeRequest(Socket clientSocket)
        {
            var receivedBufferlen = 0;
            var buffer = new byte[10240];
            receivedBufferlen = clientSocket.Receive(buffer);
            return _charEncoder.GetString(buffer, 0, receivedBufferlen);
        }
    }

    public class RequestParser
    {
        private Encoding _charEncoder = Encoding.UTF8;
        public string HttpMethod;
        public string[] queryString;
        public string HttpProtocolVersion;


        public void Parser(string requestString)
        {
            string[] tokens = requestString.Split(' ');

            tokens[1] = tokens[1].Replace("/", "\\");
            HttpMethod = tokens[0].ToUpper();
            queryString = tokens[1].Split(new char[] {'&'}, StringSplitOptions.RemoveEmptyEntries);
            HttpProtocolVersion = tokens[2];
        }
    }

    public class CreateResponse
    {
        RegistryKey registryKey = Registry.ClassesRoot;
        public Socket ClientSocket = null;
        private Encoding _charEncoder = Encoding.UTF8;
        private string _contentPath;
        public FileHandler FileHandler;
        public MessageSystem messageSystem;

        public CreateResponse(Socket clientSocket, string contentPath)
        {
            _contentPath = contentPath;
            ClientSocket = clientSocket;
            FileHandler = new FileHandler(_contentPath);
        }

        public void Get(string[] queryString)
        {
            
            Guid ClientGuid;
            if (queryString != null && queryString.Count() == 2)
            {
                string _guid = queryString[0].Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries)
                    .Last().CleanText();
                bool isGuidValid = Guid.TryParse(_guid, out ClientGuid);

                //"query=%3CUPDATE%3E"
                string query = queryString[1].Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries)
                    .Last();

                if (isGuidValid)
                {
                    string update = "updateupdate";
                    if (query == update)
                    {
                        Message message = messageSystem.GetResponse(ClientGuid);
                        if (message == null)
                        {
                            SendResponse(ClientSocket, update.GetBytes(), "200 Ok", "text/html");
                        }
                        else
                        {
                            SendResponse(ClientSocket, message.response, "200 Ok", "audio/wav");
                        }
                    }
                    else
                    {
                        
                    }
                }
            }

            else
            {
                SendErrorResponce(ClientSocket, new Exception("ERROR: PLEASE USE ECHOCLIENT!"));
            }
        }

        public void Post(string[] queryString)
        {
            Guid ClientGuid;
            if (queryString != null && queryString.Count() == 1)
            {
                string _guid = queryString[0].Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries)
                    .Last().CleanText();
                bool isGuidValid = Guid.TryParse(_guid, out ClientGuid);

                if (isGuidValid)
                {
                    int bytesRead = 0;
                    Stream stream = new MemoryStream();
                    ClientSocket.ReceiveTimeout = 50;
                    try
                    {
                        
                        do
                        {
                            byte[] buffer = new byte[1024];
                            bytesRead = ClientSocket.Receive(buffer, buffer.Length, SocketFlags.None);
                            
                            stream.Write(buffer, 0, buffer.Length);
                        } while (bytesRead > 0);
                        stream.Flush();
                        stream.Position = 0;
                        messageSystem.CreateRequest(ClientGuid, stream);
                        stream.Close();
                    }
                    catch (SocketException se)
                    {
                        Debugger.Launch();
                    }
                    finally
                    {
                        stream.Dispose();
                    }
                    

                    
                }
            }
            else
            {
                SendErrorResponce(ClientSocket, new Exception("ERROR: PLEASE USE ECHOCLIENT!"));
            }
        }

        private string GetTypeOfFile(RegistryKey registryKey, string fileName)
        {
            RegistryKey fileClass = registryKey.OpenSubKey(Path.GetExtension(fileName));
            return fileClass.GetValue("Content Type").ToString();
        }

        private void SendErrorResponce(Socket clientSocket, Exception e)
        {
            string Error = e.Message;
            
            SendResponse(clientSocket, Error.GetBytes(), "404 Not Found", "text/html");
        }


        private void SendResponse(Socket clientSocket, byte[] byteContent, string responseCode, string contentType)
        {
            byte[] byteHeader = CreateHeader(responseCode, byteContent.Length, contentType);
            clientSocket.Send(byteHeader);
            clientSocket.Send(byteContent);
            clientSocket.Close();
        }

        private byte[] CreateHeader(string responseCode, int contentLength, string contentType)
        {
            return _charEncoder.GetBytes("HTTP/1.1 " + responseCode + "\r\n"
                                  + "Server: Simple Web Server\r\n"
                                  + "Content-Length: " + contentLength + "\r\n"
                                  + "Connection: close\r\n"
                                  + "Content-Type: " + contentType + "\r\n\r\n");
        }
    }

    public class FileHandler
    {
        private string _contentPath;

        public FileHandler(string contentPath)
        {
            _contentPath = contentPath;
        }

        internal bool DoesFileExists(string directory)
        {
            return File.Exists(_contentPath + directory);
        }

        internal byte[] ReadFile(string path)
        {
            //return File.ReadAllBytes(path);
            if (ServerCache.Contains(_contentPath + path))
            {
                MessageBox.Show("cache hit");
                return ServerCache.Get(_contentPath + path);
            }
            else
            {
                byte[] content = File.ReadAllBytes(_contentPath + path);
                ServerCache.Insert(_contentPath + path, content);
                return content;
            }

        }
    }

    class ServerCache
    {
        public struct Content
        {
            internal byte[] ResponseContent;
            internal int RequestCount;
        };
        private static readonly object SyncRoot = new object();
        private static int _capacity = 15;
        private static Dictionary<string, Content> _cache = new Dictionary<string,Content>(StringComparer.OrdinalIgnoreCase) { };

        public static bool Insert(string url, byte[] body)
        {
            lock (SyncRoot)
            {
                if (IsFull())
                    CreateEmptySpace();

                var content = new Content {RequestCount = 0, ResponseContent = new byte[body.Length]};
                Buffer.BlockCopy(body, 0, content.ResponseContent, 0, body.Length);
                if (!_cache.ContainsKey(url))
                {
                    _cache.Add(url, content);
                    return false;
                }

                return true;
            }

        }

        public static bool IsFull()
        {
            return _cache.Count >= _capacity;
        }

        public static byte[] Get(string url)
        {
            if (_cache.ContainsKey(url))
            {
                Content content = _cache[url];
                content.RequestCount++;
                _cache[url] = content;
                return content.ResponseContent;
            }

            return null;
        }

        public static bool Contains(string url)
        {
            return _cache.ContainsKey(url);
        }

        private static void CreateEmptySpace()
        {
            var minRequestCount = Int32.MaxValue;
            var url = String.Empty;
            foreach (var entry in _cache)
            {
                Content content = entry.Value;
                if (content.RequestCount < minRequestCount)
                {
                    minRequestCount = content.RequestCount;
                    url = entry.Key;
                }
            }

            _cache.Remove(url);
        }

        public static int CacheCount()
        {
            return _cache.Count;
        }
    }
}
