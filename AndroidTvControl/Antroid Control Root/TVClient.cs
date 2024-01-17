using AndroidTvControl.Hellpers;
using AndroidTvControl.Wol;
using Crestron.SimplSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AndroidTvControl.Antroid_Control_Root
{
    /// <summary>
    /// Android TV client. 
    /// </summary>
    /// <remarks>https://github.com/Aymkdn/assistant-freebox-cloud/wiki/Google-TV-(aka-Android-TV)-Remote-Control-(v2)</remarks>
    public class TVClient : TVClientBase
    {
        private const int REMOTE_PORT = 6466;
        private AndroidTVConfiguraton _configuration = null;
        private Task _keepAlive = null;
        public bool ClientConnected = false;       
        private CancellationTokenSource cts = new CancellationTokenSource();
        public event EventHandler<ConfigurationChangedEventArgs> ConfigurationChanged;
        private WakeOnLan _wakeOnLan;
        public TVClient(string ip, string clientCertificate) : base(ip, REMOTE_PORT, clientCertificate)
        {
            if (clientCertificate == null)
                throw new ArgumentNullException(nameof(clientCertificate));

            //_wakeOnLan = new WakeOnLan();
            //_wakeOnLan.EnableServer();
        }
        public async Task StartAsync()
        {
            try
            {
                if (_keepAlive != null)
                    return;

                if (_networkStream == null || cts.IsCancellationRequested || !IsConnected())
                    GetNetworkStream();

                await InitialConnectionHandshake();
                _keepAlive = KeepAliveAsync();
                ClientConnected = IsConnected();
                //Debug.PrintLine($"Running ConnectAsync");
            }
            catch (Exception ex) { ErrorLog.Exception($"Exception on ConnectAsync Method:", ex.InnerException); }
        }
        private async Task<byte[]> InitialConnectionHandshake()
        {
            var response = await ReadMessageAsync();
            var serverConfig = InitialConfigurationMessage.FromBytes(response);
            _configuration = new AndroidTVConfiguraton()
            {
                ModelName = serverConfig.ModelName,
                VendorName = serverConfig.VendorName,
                Version = serverConfig.Version,
                AppName = serverConfig.AppName,
                AppVersion = serverConfig.AppVersion,
            };
            var clientConfig = new InitialConfigurationMessage("Assistant Cloud", "Kodono", "10", "info.kodono.assistant", "1.0.0").ToBytes();
            await _networkStream.SendMessageAsync(clientConfig, cts.Token);
            response = await ReadMessageAsync();
            if (response[0] != 18 || response[1] != 0)
                throw new Exception("Unknown error!");
            await _networkStream.SendMessageAsync(new byte[] { 18, 3, 8, 238, 4 }, cts.Token);
            for (int i = 0; i < 3; i++)
            {
                response = await _networkStream.ReadMessageAsync(cts.Token);
                UpdateConfiguration(_configuration, response);
            }
            ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs(new AndroidTVConfiguraton(_configuration)));
            return response;
        }
        private Task KeepAliveAsync()
        {
            return Task.Run(async () =>
            {
                while (!cts.IsCancellationRequested)
                {
                    var bytes = await ReadMessageAsync();
                    //Debug.PrintLine($"Message received on KeepAliveAsync: {BitConverter.ToString(bytes)}");
                    if (bytes.Length > 0)
                    {
                        if (bytes[0] == 66) // if we've received a ping
                        {
                            await SendMessageAsync(new byte[] { 74, 2, 8, 25 });
                        }
                        else if (bytes[0] == 32)
                        {
                            byte currentVolume = bytes[7];
                            _configuration.CurrentVolume = currentVolume;
                            ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs(new AndroidTVConfiguraton(_configuration)));
                        }
                    }
                }
            });
        }
        private void UpdateConfiguration(AndroidTVConfiguraton configuration, byte[] message)
        {
            try
            {
                switch (message[0])
                {
                    case 146:
                        {
                            byte currentVolume = message[message.Length - 3];
                            configuration.CurrentVolume = currentVolume;
                            if (DebugEnable)
                                Debug.PrintLine($"TV Current Volume: {configuration.CurrentVolume}");
                        }
                        break;
                    case 162:
                        {
                            int length = message[6];
                            configuration.CurrentApplication = Encoding.ASCII.GetString(message.Skip(7).Take(length).ToArray());
                            if (DebugEnable)
                                Debug.PrintLine($"TV Current Application: {configuration.CurrentApplication}");
                        }
                        break;
                    case 194:
                        {
                            configuration.IsOn = message[4] == 1 ? true : false;
                            if(DebugEnable)
                                Debug.PrintLine($"TV Power Is: {configuration.IsOn}");
                        }
                        break;
                    default:
                        throw new NotSupportedException($"Unknown message {message[0]} received");
                }
            }
            catch (Exception ex)
            {
                ErrorLog.Exception("Exception On UpdateConfiguration Method: ", ex.InnerException);
            }
        }
        public AndroidTVConfiguraton GetConfiguration()
        {
            if (_keepAlive == null)
                throw new Exception("Not connected!");
            return _configuration;
        }
        public async Task PressKeyAsync(KeyCodes code, KeyAction action)
        {
            if (DebugEnable)
                Debug.PrintLine($"KeyPress: {typeof(KeyAction).GetEnumName(action)}");
            await StartAsync();

            if (action != KeyAction.SHORT)
            {
                await SendMessageAsync(new byte[] { 82, 4, 8, (byte)code, 16, (byte)KeyAction.START_LONG });
                await SendMessageAsync(new byte[] { 82, 4, 8, (byte)code, 16, (byte)KeyAction.END_LONG });
            }
            else
                await SendMessageAsync(new byte[] { 82, 5, 8, (byte)code, 1, 16, (byte)action });
        }
        public async Task SendMessageAsync(byte[] data)
        {
            try
            {
                await StartAsync();
                await _networkStream.SendMessageAsync(data, cts.Token);
                if (DebugEnable)
                    Debug.PrintLine($"Message Send: {BitConverter.ToString(data)}");
            }
            catch (Exception ex)
            {
                if (cts.Token.IsCancellationRequested)
                    if (DebugEnable)
                        Debug.PrintLine("Token Cancellation Requested on SendMessageAsync Method");
                ErrorLog.Error("Exception On SendMessageAsync: ", ex.Message);
                ClientConnected = false;
            }
        }
        public async Task<byte[]> ReadMessageAsync()
        {
            byte[] data = null;
            try
            {
                data = await _networkStream.ReadMessageAsync(cts.Token);
                if(DebugEnable)
                    Debug.PrintLine($"Message received: {BitConverter.ToString(data)}");
            }
            catch (Exception ex)
            {
                if (cts.Token.IsCancellationRequested)
                    //Debug.PrintLine("Token Cancellation Requested on ReadMessageAsync Method");
                ErrorLog.Error($"Exception On ReadMessageAsync: ", ex.Message);
                ClientConnected = false;
            }
            return data;
        }
        public async Task StartApplicationAsync(string content)
        {
            List<byte> message = new List<byte>() { 210, 5, 0, 10 };
            byte[] contentBytes = Encoding.ASCII.GetBytes(content);
            message.Add((byte)contentBytes.Length);
            message.AddRange(contentBytes);
            message[2] = (byte)(message.Count - 3);
            await SendMessageAsync(message.ToArray());
        }
        public void TurnOnAsync(string mac)
        {

            if (string.IsNullOrWhiteSpace(mac))
            {
                throw new NotSupportedException("Unable to retrieve MAC address, please provide a valid MAC address when creating the client.");
            }
            else
                _wakeOnLan.SendWoL(mac);
        }
        protected override void Dispose(bool disposing)
        {
            try
            {
                cts.Cancel();
            }
            catch (Exception ex)
            {
                ErrorLog.Error($"Exeption on Dispose Method {ex.Message}");
            }
            cts.Dispose();
            base.Dispose(disposing);
        }
    }
}
