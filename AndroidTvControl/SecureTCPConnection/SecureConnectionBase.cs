using Crestron.SimplSharp.CrestronSockets;
using Crestron.SimplSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crestron.SimplSharpPro.CrestronThread;
using System.Data;
using Crestron.SimplSharp.Cryptography.X509Certificates;
using Crestron.SimplSharp.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using Crestron.SimplSharp.CrestronLogger;
using AndroidTvControl.Hellpers;

namespace AndroidTvControl.SecureTCPConnection
{
    public class SecureConnectionBase
    {
        private int Port;
        private string ClientIPAddress;
        private int BufferSize;
        public SecureTCPClient Client;
        private Thread myClientThread;
        private bool RunThread = false;
        public event EventHandler<SecureTcpClientEventArgs> myClientEventArgs;
        private string LastRX = "";
        private string temp = "";
        private CrestronQueue txQueue;
        private X509Certificate2 _clientCertificate = null;
        protected X509Certificate2 ClientCertificate { get { return _clientCertificate; } }
        public int ConnectionStatus { get { return (int)Client.ClientStatus; } }
        public string RX { get { return LastRX; } }
        public string TX { set { txQueue.Enqueue(value); } }
        public SocketStatus SocketStatus { get; set; }
        public bool IsConnected
        {
            get
            {
                if (Client != null && Client.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED)
                    return true;
                else
                    return false;
            }
        }
        public SocketStatus ClientStatus
        {
            get
            {
                if (Client == null)
                    return SocketStatus.SOCKET_STATUS_NO_CONNECT;
                return Client.ClientStatus;
            }
        }
        public SecureConnectionBase(string ipAddress, int port, int bufferSize)
        {
            txQueue = new CrestronQueue();
            this.ClientIPAddress = ipAddress;
            this.Port = port;
            this.BufferSize = bufferSize;
        }
        public void Connect()
        {
            try
            {
                this.Client = new SecureTCPClient(ClientIPAddress, Port, BufferSize);
                this.Client.SocketStatusChange += Client_SocketStatusChange;
                RunThread = true;
                myClientThread = new Thread(ClientThreadMethod, null, Thread.eThreadStartOptions.Running);
               
            }
            catch (Exception ex) { ErrorLog.Exception("Error on LighningConnectionClient : ", ex); }
        }
        public void clientReceiveCallback(SecureTCPClient client, int bytes_recvd)
        {
            if (bytes_recvd <= 0) // 0 or negative byte count indicates the connection has been closed
            {
                Debug.PrintLine("clientReceiveCallback: Could not receive message- connection closed");
            }
            else
            {
                try
                {
                    CrestronConsole.PrintLine("Received " + bytes_recvd + " bytes from " + client.AddressClientConnectedTo + " port " + client.PortNumber);
                    string received = ASCIIEncoding.ASCII.GetString(client.IncomingDataBuffer, 0, client.IncomingDataBuffer.Length);
                    CrestronConsole.PrintLine("Server says: " + received);
                }
                catch (Exception e)
                {
                    Debug.PrintLine("Exception in clientReceiveCallback: " + e.Message);
                }
                // Wait for another message
                client.ReceiveDataAsync(clientReceiveCallback);
            }
        }

        protected SecureTCPClient GetSecureNetworkStream()
        {
            try
            {
                if (this.Client != null)
                    return this.Client;

                if (this._clientCertificate == null)
                    throw new Exception($"Client certificate not set! Call to set it before getting the stream.");

                Client = new SecureTCPClient(ClientIPAddress, Port, BufferSize);
                Client.SetClientCertificate(this._clientCertificate);
                //Client.SetClientPrivateKey();
                
                Client.ConnectToServerAsync(SecureConnectionClientCallBack);
                Client.ReceiveDataAsync(clientReceiveCallback);


                //var callback = new RemoteCertificateValidationCallback((s, c, ch, err) => { return true; }); // ignore certificate errors

                //this._networkStream = new SslStream(_client.GetStream(), false, callback, null);

                //this._networkStream.AuthenticateAsClient(GetIP(), new X509CertificateCollection() { this._clientCertificate }, SslProtocols.Tls, false);
               
            }
            catch (Exception ex)
            {
                Debug.PrintLine($"Exception on GetNetworkStream: {ex.InnerException}");
            }
            return this.Client;
        }

        private void SecureConnectionClientCallBack(SecureTCPClient myTCPClient)
        {
            throw new NotImplementedException();
        }

        private object ClientThreadMethod(object userSpecific)
        {
            try
            {
                if (Client.ConnectToServer() == 0)
                {
                    while (RunThread)
                    {
                        if (Client.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED)
                        {
                            if (!txQueue.IsEmpty)
                            {
                                temp = txQueue.Dequeue().ToString();
                                byte[] myData = Encoding.ASCII.GetBytes(temp);
                                Client.SendData(myData, myData.Length);
                            }

                            if (Client.DataAvailable)
                            {
                                Client.ReceiveData();
                                var lenght = Client.IncomingDataBuffer.Length;
                                LastRX = Encoding.ASCII.GetString(Client.IncomingDataBuffer, 0, lenght);
                                OnRiseEvent(new SecureTcpClientEventArgs("Success") { Received = LastRX });
                            }
                        }
                        else
                            RunThread = false;
                    }
                }
                else
                {
                    Disconnect();
                    OnRiseEvent(new SecureTcpClientEventArgs("Fail") { Status = ConnectionStatus });
                }
            }
            catch (Exception ex) { ErrorLog.Exception("Error on Running TCP Client Thread : ", ex); }
            return null;

        }
            

        private void Client_SocketStatusChange(SecureTCPClient myTCPClient, SocketStatus clientSocketStatus)
        {
            SocketStatus = clientSocketStatus;
            OnRiseEvent(new SecureTcpClientEventArgs("STATUS") { Connected = IsConnected, Status = ConnectionStatus });
        }
       
        public void Disconnect()
        {
            RunThread = false;
            Client.DisconnectFromServer();
            txQueue.Clear();
            LastRX = "";
            Client.Dispose();
        }
        protected virtual void OnRiseEvent(SecureTcpClientEventArgs args)
        {
            EventHandler<SecureTcpClientEventArgs> clientEvent = myClientEventArgs;
            if (clientEvent != null)
            {
                clientEvent(this, args);
            }
        }
    }
}
