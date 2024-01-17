using Crestron.SimplSharp;

namespace AndroidTvControl.Hellpers
{
    public class ProcessorInfo
    {
        public static string GetProcessorIP()
        {
            var ipAddress = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, 0).ToString();
            return ipAddress;
        }
        public static string GetProcessorMac()
        {
            var macAddress = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_MAC_ADDRESS, 0).ToUpper();
            return macAddress;
        }
        public static string GetFirmwareVersion()
        {
            return CrestronEnvironment.OSVersion.Firmware.ToString();
        }

        public string GetMacAddress()
        {
            var macAddress = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_MAC_ADDRESS, 0).ToUpper();
            int IndexChar;
            while ((IndexChar = macAddress.IndexOf(':')) >= 0)
            {
                macAddress = macAddress.Remove(IndexChar, 1);
            }
            return macAddress;
        }
    }
}
