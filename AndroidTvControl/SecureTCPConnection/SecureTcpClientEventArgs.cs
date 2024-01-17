using System;

namespace AndroidTvControl.SecureTCPConnection
{
    public class SecureTcpClientEventArgs :EventArgs
    {
        public SecureTcpClientEventArgs(string message)
        {
            Message = message;
        }
        public string Message { get; set; }
        public bool Connected { get; set; }
        public int Status { get; set; }
        public string Received { get; set; }
    }
}