using System;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Timers;
using System.Net.Sockets;
using System.Threading;

namespace EchoClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Guid guid;

        public MainWindow()
        {
            InitializeComponent();

            guid = Guid.NewGuid();
        }

        private void startBtn_Click(object sender, RoutedEventArgs e)
        {
            lblUpdateTime.Content = StartClient();
        }

        public static string StartClient()
        {
            // Data buffer for incoming data.
            byte[] bytes = new byte[1024];

            // Connect to a remote device.
            try
            {
                // Establish the remote endpoint for the socket.
                // This example uses port 11000 on the local computer.
                IPHostEntry ipHostInfo = Dns.GetHostEntry("sky.ibang.us");
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 8080);

                // Create a TCP/IP  socket.
                Socket sender = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.
                try
                {
                    sender.Connect(remoteEP);

                    Console.WriteLine("Socket connected to {0}",
                        sender.RemoteEndPoint.ToString());

                    // Encode the data string into a byte array.
                    string query = guid.ToString() + "what is the time" + "<EOF>";
                    byte[] msg = Encoding.ASCII.GetBytes(query);

                    // Send the data through the socket.
                    int bytesSent = sender.Send(msg);

                    // Receive the response from the remote device.
                    int bytesRec = sender.Receive(bytes);
                    string response = Encoding.ASCII.GetString(bytes, 0, bytesRec);

                    // Release the socket.
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();

                    return response;

                }
                catch (ArgumentNullException ane)
                {
                    return "ArgumentNullException : " + ane.ToString();
                }
                catch (SocketException se)
                {
                    return "SocketException : " + se.ToString();
                }
                catch (Exception e)
                {
                    return "Unexpected exception : " + e.ToString();
                }

            }
            catch (Exception e)
            {
                return e.ToString();
            }
            return "ERROR";
        }
    }

}
