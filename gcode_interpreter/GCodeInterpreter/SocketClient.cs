using System;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Windows;

namespace GCodeInterpreter
{
    // State object for receiving data from remote device.  
    // Reading data from a client socket requires a state object that passes values between asynchronous calls. 
    // The following class is an example state object for receiving data from a client socket. 
    // It contains a field for the client socket, a buffer for the received data, and a StringBuilder to hold the incoming data string. Placing these fields in the state object allows their values to be preserved across multiple calls to read data from the client socket. 
    public class StateObject
    {
        // Client socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 1024;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // Received data string.  
        public StringBuilder sb = new StringBuilder();
    }

    public class SocketClient
    {
        //Event to handle the new read line 
        public event ReadLineEventHandler ReadLineEvent;
        public event CommunicationFailureEventHandler CommunicationFailureEvent;

        // The port number for the remote device [manually set 1313]
        private const int port_ = 1313;
        private Socket mClient;
        private const int port2_ = 1315;
        private Socket mClient2;
        public bool isConnected;

        // ManualResetEvent instances signal completion.  
        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);

        // The response from the remote device.  
        private String message = String.Empty;
        private String response = String.Empty;

        public bool StartClient()
        {
            if (isConnected)
            {
                CloseClient();
            }

            // Connect to a remote device.  
            try
            {
                // Establish the remote endpoint for the socket.  
                // The name of the host device: Dns.GetHostName()
                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                foreach (var ip in ipHostInfo.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ipAddress = ip;
                    }
                }
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port_);
                IPEndPoint remoteEP2 = new IPEndPoint(ipAddress, port2_);

                // Create a TCP/IP socket.  
                mClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                mClient2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.  
                isConnected = true;
                mClient.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), mClient);
                isConnected &= connectDone.WaitOne(1000); //1 secs timeout

                if (isConnected)
                {
                    mClient2.BeginConnect(remoteEP2, new AsyncCallback(ConnectCallback), mClient2);
                    isConnected &= connectDone.WaitOne(5000); //5 secs timeout
                }
                
                if (isConnected)
                {
                    Receive();
                }
                return isConnected;
            }
            catch (Exception e)
            {
                //Trigget Communication Failure Event
                if (CommunicationFailureEvent != null)
                {
                    string tip = "Error launching client communication.";
                    CommunicationFailureEvent(this, e.Message, tip);
                }
                return false;
            }
        }

        public bool CloseClient()
        {
            if(isConnected)
            {
                try
                {
                    isConnected = false;

                    // Release the sockets  
                    mClient.Shutdown(SocketShutdown.Both);
                    mClient.Close();

                    mClient2.Shutdown(SocketShutdown.Both);
                    mClient2.Close();
                }
                catch (Exception e)
                {
                    string error_title = "Error in " + e.Source;
                    MessageBox.Show(e.Message, error_title, MessageBoxButton.OK, MessageBoxImage.Exclamation);

                    return false;
                }
            }

            return !isConnected;
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.  
                client.EndConnect(ar);

                if (client.Connected)
                {
                    isConnected = true;
                    Console.WriteLine("Socket connected to {0}", client.RemoteEndPoint.ToString());
                }
                else
                {
                    isConnected = false;
                    Console.WriteLine("Cannot connect to remote host at {0}", client.RemoteEndPoint.ToString());
                }

                // Signal that the connection has been made.  
                connectDone.Set();
            }
            catch (Exception e)
            {
                isConnected = false;

                //Trigget Communication Failure Event
                if (CommunicationFailureEvent != null)
                {
                    string tip = "Check if simulation is running.";
                    CommunicationFailureEvent(this, e.Message, tip);
                }
            }
        }

        public void Receive()
        {
            try
            {
                // Create the state object.  
                StateObject state = new StateObject();
                state.workSocket = mClient2;

                // Begin receiving the data from the remote device.  
                mClient2.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                //Trigget Communication Failure Event
                if (CommunicationFailureEvent != null)
                {
                    string tip = "Try to re-establish connection.";
                    CommunicationFailureEvent(this, e.Message, tip);
                }
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            if (isConnected)
            {
                try
                {
                    // Retrieve the state object and the client socket   
                    // from the asynchronous state object.  
                    StateObject state = (StateObject)ar.AsyncState;
                    Socket client = state.workSocket;

                    // Read data from the remote device.  
                    int bytesRead = client.EndReceive(ar);

                    if (bytesRead > 0)
                    {
                        response = Encoding.ASCII.GetString(state.buffer, 0, bytesRead);

                        string subresponse = Translator.GetUntilOrEmpty(response, "}");

                        //Subresponse is not empty means it found a JSON message
                        if (Translator.IsValidJson(subresponse))
                        {
                            // Signal that all bytes have been received.  
                            receiveDone.Set();

                            Console.WriteLine(subresponse);

                            Tuple<int, bool> line_info = Translator.GetLine(subresponse);

                            if (ReadLineEvent != null)
                            {
                                //Display the line number and if the line has collisions
                                ReadLineEvent(this, line_info.Item1, line_info.Item2);
                            }
                        }
                    }

                    // Continuously receive
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                            new AsyncCallback(ReceiveCallback), state);
                }
                catch (Exception e)
                {
                    //Trigget Communication Failure Event
                    if (CommunicationFailureEvent != null)
                    {
                        string tip = "Error receive communication callback.";
                        CommunicationFailureEvent(this, e.Message, tip);
                    }
                }
            }
        }

        public void Send(String data)
        {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            mClient2.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), mClient2);
        }

        // Sends the preprared stream of data
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.  
                sendDone.Set();
            }
            catch (Exception e)
            {
                string error_title = "Error in " + e.Source;
                MessageBox.Show(e.Message, error_title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        //Public accessors
        public bool SendBlocking(String data)
        {
            bool success = false;
            try { 
                // Convert the string data to byte data using ASCII encoding.  
                byte[] byteData = Encoding.ASCII.GetBytes(data);

                //Timeout 2s
                mClient.SendTimeout = 2000;
                mClient.ReceiveTimeout = 2000;
                // Send the data through the socket.           
                int bytesSent = mClient.Send(byteData);
                // Receive the response from the remote device. 
                byte[] bytes = new byte[1024];
                int bytesRec = mClient.Receive(bytes);

                success = (bytesSent == bytesRec);
            }
            catch (Exception e)
            {
                //Trigget Communication Failure Event
                if(CommunicationFailureEvent != null)
                {
                    string tip = "Try to stop and relaunch simulation. Disconnect from the server.";
                    CommunicationFailureEvent(this, e.Message, tip);
                }
            }

            return success;
        }

    }
}
