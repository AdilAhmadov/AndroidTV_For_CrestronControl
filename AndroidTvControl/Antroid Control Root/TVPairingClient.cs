using AndroidTvControl.Hellpers;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AndroidTvControl.Antroid_Control_Root
{
    /// <summary>
    /// Android TV pairing client. 
    /// </summary>
    /// <remarks>Pairing algorithm described here: https://github.com/Aymkdn/assistant-freebox-cloud/wiki/Google-TV-(aka-Android-TV)-Remote-Control-(v2)</remarks>
    public class TVPairingClient : TVClientBase
    {
        private const int PAIRING_PORT = 6467;
        private string _clientCertificatePem = null;
        private X509Certificate2 _serverCertificate;
        private bool _isPairingInProgress = false;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        public TVPairingClient(string ip, string clientCertificate = null) : base(ip, PAIRING_PORT)
        {
            if (clientCertificate != null)
            {
                this._clientCertificatePem = clientCertificate;
            }
        }
        public async Task InitiatePairingAsync()
        {
            if (this.ClientCertificate == null)
            {
                this._clientCertificatePem = CertificateUtils.GenerateCertificate("atvremote", "US", "California", "Mountain View", "Google Inc.", "Android", "example@google.com");

                SetClientCertificate(this._clientCertificatePem);
            }
            if (_networkStream == null)
                GetNetworkStream();
            var cancellationToken = _cancellationTokenSource.Token;
            byte[] response;
            await SendPairingMessageAsync(_networkStream, cancellationToken);
            response = await _networkStream.ReadMessageAsync(cancellationToken);
            VerifyResult(response);
            await SendOptionMessageAsync(_networkStream, cancellationToken);
            response = await _networkStream.ReadMessageAsync(cancellationToken);
            VerifyResult(response);
            await SendConfigurationMessageAsync(_networkStream, cancellationToken);
            response = await _networkStream.ReadMessageAsync(cancellationToken);
            VerifyResult(response);
            this._isPairingInProgress = true;
        }
        private async Task SendMessageSize(Stream networkStream, CancellationToken token, List<byte> bytes)
        {
            byte[] messageCount = new byte[1];
            messageCount[0] = (byte)(bytes.Count);
            await networkStream.SendMessageAsync(messageCount, token);
        }
        private async Task SendPairingMessageAsync(Stream networkStream, CancellationToken token)
        {
            List<byte> message = new List<byte>() { 8, 2, 16, 200, 1, 82, 43, 10 };
            
            byte[] serviceName = Encoding.ASCII.GetBytes("info.kodono.assistant");
            message.Add((byte)serviceName.Length);
            message.AddRange(serviceName);

            message.Add(18); // tag device name

            byte[] clientName = Encoding.ASCII.GetBytes("interface web");
            message.Add((byte)clientName.Length);
            message.AddRange(clientName);

            //// length of the message minus version length
            message[6] = (byte)(message.Count - 2);  ///????
            //await SendMessageSize(networkStream, token, message);

            await networkStream.SendMessageAsync(message.ToArray(), token);
        }
        private async Task SendOptionMessageAsync(Stream networkStream, CancellationToken token)
        {
            List<byte> message = new List<byte>() { 8, 2, 16, 200, 1, 162, 1, 8, 10, 4, 8, 3, 16, 6, 24, 1 };

            //await SendMessageSize(networkStream, token, message);

            await networkStream.SendMessageAsync(message.ToArray(), token);
        }
        private async Task SendConfigurationMessageAsync(Stream networkStream, CancellationToken token)
        {
            List<byte> message = new List<byte>() { 8, 2, 16, 200, 1, 242, 1, 8, 10, 4, 8, 3, 16, 6, 16, 1 };

            //await SendMessageSize(networkStream, token, message);

            await networkStream.SendMessageAsync(message.ToArray(), token);
        }
        public async Task<string> CompletePairingAsync(string code)
        {
            try
            {
                if (!this._isPairingInProgress)
                    throw new InvalidOperationException($"You must first start pairing by calling {nameof(InitiatePairingAsync)}!");

                if (string.IsNullOrWhiteSpace(code) || code.Length != 6)
                    throw new ArgumentException("Invalid code! Expected 6 letters.");

                //var networkStream = GetNetworkStream();
                if (_networkStream == null)
                    GetNetworkStream();

                if (this._serverCertificate == null)
                {
                    _serverCertificate = new X509Certificate2(_networkStream.RemoteCertificate);
                    Debug.PrintLine($"Remote Server Certificate: {_serverCertificate.ToString()}");
                }
                //_serverCertificate = new X509Certificate2(networkStream.RemoteCertificate);
                await SendSecretMessageAsync(_networkStream, code, this.ClientCertificate, this._serverCertificate, _cancellationTokenSource.Token);
                byte[] response = await _networkStream.ReadMessageAsync(_cancellationTokenSource.Token);
                VerifyResult(response);
            }
            catch (Exception ex)
            {
                Debug.PrintLine($"Exception on CompletePairingAsync method: {ex.InnerException}");
            }
            
            this._isPairingInProgress = false;
            return this._clientCertificatePem;
        }
        private static byte[] GetAlphaValue(string code, X509Certificate2 clientCertificate, X509Certificate2 serverCertificate)
        {
            // nonce are the last 4 characters of the code displayed on the TV
            byte[] nonce = FromHexString(code.Substring(2)).ToArray();

            var client = DotNetUtilities.FromX509Certificate(clientCertificate);
            var server = DotNetUtilities.FromX509Certificate(serverCertificate);

            var publicKey = client.GetPublicKey();
            var publicKey2 = server.GetPublicKey();

            var rSAPublicKey = (RsaKeyParameters)publicKey;
            var rSAPublicKey2 = (RsaKeyParameters)publicKey2;

            var instance = new Sha256Digest();

            byte[] clientModulus = RemoveLeadingZeroBytes(rSAPublicKey.Modulus.Abs().ToByteArray());
            byte[] clientExponent = RemoveLeadingZeroBytes(rSAPublicKey.Exponent.Abs().ToByteArray());
            byte[] serverModulus = RemoveLeadingZeroBytes(rSAPublicKey2.Modulus.Abs().ToByteArray());
            byte[] serverExponent = RemoveLeadingZeroBytes(rSAPublicKey2.Exponent.Abs().ToByteArray());

            Debug.PrintLine("Hash inputs: ");
            Debug.PrintLine("client modulus: " + BitConverter.ToString(clientModulus));
            Debug.PrintLine("client exponent: " + BitConverter.ToString(clientExponent));
            Debug.PrintLine("server modulus: " + BitConverter.ToString(serverModulus));
            Debug.PrintLine("server exponent: " + BitConverter.ToString(serverExponent));
            Debug.PrintLine("nonce: " + BitConverter.ToString(nonce));

            instance.BlockUpdate(clientModulus, 0, clientModulus.Length);
            instance.BlockUpdate(clientExponent, 0, clientExponent.Length);
            instance.BlockUpdate(serverModulus, 0, serverModulus.Length);
            instance.BlockUpdate(serverExponent, 0, serverExponent.Length);
            instance.BlockUpdate(nonce, 0, nonce.Length);

           

            byte[] hash = new byte[instance.GetDigestSize()];
            instance.DoFinal(hash, 0);

            Debug.PrintLine("hash: " + BitConverter.ToString(hash));

            return hash;
        }
        private static async Task SendSecretMessageAsync(Stream networkStream, string code, X509Certificate2 clientCertificate, X509Certificate2 serverCertificate, CancellationToken token)
        {
            List<byte> message = new List<byte>() { 8, 2, 16, 200, 1, 194, 2, 34, 10, 32 };
            message.AddRange(GetAlphaValue(code, clientCertificate, serverCertificate));

            if (message.Count != 42)
                throw new InvalidOperationException("Invalid pairing message!");
            
            await networkStream.SendMessageAsync(message.ToArray(), token);
        }        
        private static void VerifyResult(byte[] response)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            if (response[0] != 8 || response[1] != 2)
                throw new Exception("Invalid protocol version!");

            if (response[2] != 16 || response[3] != 200 || response[4] != 1)
            {
                if (response[4] == 3)
                {
                    if (response[3] == 144)
                        throw new Exception("ERROR");
                    else if (response[3] == 145)
                        throw new Exception("BAD CONFIGURATION");
                    else
                        throw new Exception("UNKNOWN ERROR");
                }
                else
                {
                    throw new Exception("UNKNOWN");
                }
            }
        }
        private static X509Certificate2 GetPublicCertificate(string host, int port)
        {
            X509Certificate2 cert = null;

            using (TcpClient client = new TcpClient())
            {
                client.Connect(host, port);

                SslStream ssl = new SslStream(
                    client.GetStream(),
                    false,
                    new RemoteCertificateValidationCallback((s, c, ch, err) => { return true; }), null);

                try
                {
                    ssl.AuthenticateAsClient(host);
                }
                catch (AuthenticationException e)
                {
                    Debug.PrintLine(e.Message);
                    ssl.Close();
                    client.Close();
                    return cert;
                }
                catch (Exception e)
                {
                    Debug.PrintLine(e.Message);
                    ssl.Close();
                    client.Close();
                    return cert;
                }

                cert = new X509Certificate2(ssl.RemoteCertificate);
                ssl.Close();
                client.Close();

                return cert;
            }
        }
        private static byte[] RemoveLeadingZeroBytes(byte[] array)
        {
            int skip = 0;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] != 0)
                {
                    skip = i;
                    break;
                }
            }

            return array.Skip(skip).ToArray();
        }
        private static byte[] FromHexString(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
        protected override void Dispose(bool disposing)
        {
            try
            {
                _cancellationTokenSource.Cancel();
            }
            catch (Exception ex)
            {
                Debug.PrintLine($"Exeption in Dispose Method: {ex.InnerException}");
            }

            base.Dispose(disposing);
        }
    }
}
