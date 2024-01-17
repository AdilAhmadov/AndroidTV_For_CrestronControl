using Crestron.SimplSharp;
using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;


namespace AndroidTvControl.Antroid_Control_Root
{
    public abstract class TVClientBase : IDisposable
    {
        private readonly string _ip;
        private readonly int _port;
        private AndroidTVConfiguraton _configuration = null;
        private TcpClient _client;
        protected SslStream _networkStream;
        private X509Certificate2 _clientCertificate = null;
        protected X509Certificate2 ClientCertificate { get { return _clientCertificate; } }
        public bool IsConnected()=>_client.Connected;
        private bool _disposedValue;
        protected bool DebugEnable { get; set; } = false;
        protected string GetIP() => this._ip;
        public TVClientBase(string ip, int port) : this(ip, port, null)
        { }
        public TVClientBase(string ip, int port, string clientCertificate)
        {
            if (string.IsNullOrWhiteSpace(ip))
                throw new ArgumentNullException(nameof(ip));

            this._ip = ip;
            this._port = port;
            
            if (clientCertificate != null)
            {
                SetClientCertificate(clientCertificate);
            }
        }       
        protected void SetClientCertificate(string clientCertificate)
        {
            if (this._clientCertificate != null)
                throw new InvalidOperationException("Client certificate already set!");

            this._clientCertificate = CertificateUtils.LoadCertificateFromPEM(clientCertificate); ;
        }              
        public void Disconnect()
        {
            _networkStream.Close();
        }
        protected SslStream GetNetworkStream()
        {
            try
            {
                if (this._networkStream != null)
                    return this._networkStream;

                if (this._clientCertificate == null)
                    throw new Exception($"Client certificate not set! Call {nameof(SetClientCertificate)} to set it before getting the stream.");

                this._client = new TcpClient();
                this._client.Connect(this._ip, this._port);
                if (IsConnected())
                {
                    var callback = new RemoteCertificateValidationCallback((s, c, ch, err) => { return true; }); // ignore certificate errors
                    this._networkStream = new SslStream(_client.GetStream(), false, callback, null);
                    this._networkStream.AuthenticateAsClient(GetIP(), new X509CertificateCollection() { this._clientCertificate }, SslProtocols.Tls, false);
                }                                
            }
            catch (Exception ex)
            {
                ErrorLog.Error($"Exception on GetNetworkStream: ", ex.Message);
                Dispose();
            }
            return this._networkStream;
        }       
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_networkStream != null)
                    {
                        _networkStream.Dispose();
                        _networkStream = null;
                    }

                    if (_client != null)
                    {
                        _client.Dispose();
                        _client = null;
                    }
                }
                _disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(disposing: true);
        }
    }
}
