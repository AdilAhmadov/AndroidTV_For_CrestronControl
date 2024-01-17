using Crestron.SimplSharpPro;

namespace AndroidTvControl.Ethernet_Communication
{
    public class EiscEventArgs
    {
        // Added an overload for when joins need to be sent
        public EiscEventArgs(string message, SigEventArgs args)
        {
            Message = message;
            Args = args;
        }
        public EiscEventArgs(string message) : this(message, null) { }
        public string Message { get; set; }
        public SigEventArgs Args { get; set; }
        public bool Online { get; set; }
        public uint ID { get; set; }
        public string IpAddress { get; set; }
    }
}
