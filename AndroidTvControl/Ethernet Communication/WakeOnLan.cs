using AndroidTvControl.Hellpers;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndroidTvControl.Wol
{
    public class WakeOnLan
    {
        public bool IsConnected { get; private set; }
        public SocketErrorCodes Status { get; private set; }

        private UDPServer Server;
        private int Port = 9;
        public void DisableServer()
        {
            if (Server != null)
                Server.DisableUDPServer();
            IsConnected = false;
        }
        public void EnableServer()
        {
            try
            {
                if (Server == null)
                {
                    Server = new UDPServer();
                }
                Status = this.Server.EnableUDPServer(IPAddress.Any, Port);
                if (Status == SocketErrorCodes.SOCKET_OK)
                {
                    IsConnected = true;
                }
                SocketErrorCodes err = Server.ReceiveDataAsync(new UDPServerReceiveCallback(OnUDPServerReceiveCallback));
            }
            catch (Exception ex) { ErrorLog.Exception("Error On GameUDPBroadcast Server Connection: ", ex.InnerException); }
        }
        private void OnUDPServerReceiveCallback(UDPServer myUDPServer, int numberOfBytesReceived)
        {
            try
            {
                if (myUDPServer.IPAddressLastMessageReceivedFrom != ProcessorInfo.GetProcessorIP())
                {
                    var LastRX = Encoding.GetEncoding(1252).GetString(myUDPServer.IncomingDataBuffer, 0, numberOfBytesReceived);
                    
                }
            }
            catch (Exception ex) { ErrorLog.Exception("Error in OnUDPServerReceiveCallback ", ex.InnerException); }
           
        }
        public void SendWoL(string mac)
        {          
            try
            {
                byte[] magicPacket = CreateMagicPacket(mac);
                if (IsConnected && Server != null)
                {
                    var serverIpEndpoint = new IPEndPoint(IPAddress.Broadcast, Port);
                    //byte[] buffer = Encoding.GetEncoding(1252).GetBytes(message);
                    Server.SendData(magicPacket, magicPacket.Length, serverIpEndpoint);
                }
            }
            catch (Exception ex) { ErrorLog.Exception("UDP Send Exception: ", ex.InnerException); }
        }

        private static byte[] CreateMagicPacket(string mac)
        {
            byte[] macBytes = FromHexString(mac);
            IEnumerable<byte> header = Enumerable.Repeat((byte)0xff, 6); // 6 times 0xFF
            IEnumerable<byte> data = Enumerable.Repeat(macBytes, 16).SelectMany(m => m); // then 16 times mac address
            return header.Concat(data).ToArray();
        }
        private static byte[] FromHexString(string hex)
        {
            hex = hex.Replace("-", "").Replace(" ", "").Replace(":", "");
            byte[] raw = new byte[hex.Length / 2];
            for (int i = 0; i < raw.Length; i++)
            {
                raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return raw;
        }
        public void Send(string message)
        {
            try
            {
                if (IsConnected && Server != null)
                {
                    var serverIpEndpoint = new IPEndPoint(IPAddress.Broadcast, Port);
                    byte[] buffer = Encoding.GetEncoding(1252).GetBytes(message);
                    Server.SendData(buffer, buffer.Length, serverIpEndpoint);
                }
            }
            catch (Exception ex) { ErrorLog.Exception("UDP Send Exception: ", ex.InnerException); }
        }
        private static void SendWakeOnLan(IPAddress localIpAddress, IPAddress multicastIpAddress, byte[] magicPacket)
        {
            using (UDPServer client = new UDPServer())
            {
                client.SendData(magicPacket, magicPacket.Length, new IPEndPoint(multicastIpAddress, 9));
            }
        }
    }
}
