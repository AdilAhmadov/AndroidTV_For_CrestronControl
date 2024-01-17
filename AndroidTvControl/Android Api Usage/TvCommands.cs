using AndroidTvControl.Hellpers;
using System;
using System.Threading;

namespace AndroidTvControl.Android_Api_Usage
{
    public class TvCommands
    {
        private ControlSystem _cs;
        public string TvCode { get; set; } = String.Empty;
        public bool DebugEnabled { get; set; } = false;
        public TvCommands(ControlSystem cs, bool debug)
        {
            this._cs = cs;
            this.DebugEnabled = debug;
        }
        public  void DeviceDPadKeyCenter(int tvSelected)
        {
            _cs.androidTvs[tvSelected - 1].DpadCenter();
            if (DebugEnabled)
                Debug.PrintLine($"TV {tvSelected} Dpad Center Pressed");
        }
        public void Connect(int tvSelected)
        {
            var certificate = _cs.tvConfig.GetCertificate(tvSelected);
            var ipAddress = _cs.tvConfig.GetTvIp(tvSelected);
            _cs.androidTvs[tvSelected - 1].Connect(certificate, ipAddress);
        }
        public  void DeviceDPadKeyRight(int tvSelected)
        {
            _cs.androidTvs[tvSelected - 1].DpadRight();
            if (DebugEnabled)
                Debug.PrintLine($"TV {tvSelected} Dpad Right Pressed");
        }

        public void DeviceDPadKeyLeft(int tvSelected)
        {
            _cs.androidTvs[tvSelected - 1].DpadLeft();
            if (DebugEnabled)
                Debug.PrintLine($"TV {tvSelected} Dpad Left Pressed");
        }

        public void DeviceDPadKeyDown(int tvSelected)
        {
            _cs.androidTvs[tvSelected - 1].DpadDown();
            if (DebugEnabled)
                Debug.PrintLine($"TV {tvSelected} Dpad Down Pressed");
        }

        public void DeviceDPadKeyUp(int tvSelected)
        {
            _cs.androidTvs[tvSelected - 1].DpadUp();
            if (DebugEnabled)
                Debug.PrintLine($"TV {tvSelected} Dpad Up Pressed");
        }

        public void ApplicationStart(int appIndex, int tvSelected)
        {
            var result = _cs.tvConfig.GetTvAppUrl(appIndex);
            _cs.androidTvs[tvSelected - 1].AppStart(result);
            if (DebugEnabled)
                Debug.PrintLine($"TV {tvSelected} App Start Is Pressed: {result}");

        }
        public void DevicePairingBackspace(int tvSelected)
        {

        }
        public void DeviceBackspace(int tvSelected)
        {
            _cs.androidTvs[tvSelected - 1].Backspace();
        }

        public void DeviceVolumeMuteOperation(int tvSelected)
        {
            _cs.androidTvs[tvSelected - 1].VolumeMute();
            if (DebugEnabled)
                Debug.PrintLine($"TV {tvSelected} Volume Mute Button Is Pressed:");
        }

        public void DeviceVolumeDownOperation(int tvSelected)
        {
            _cs.androidTvs[tvSelected - 1].VolumeDown();
            if (DebugEnabled)
                Debug.PrintLine($"TV {tvSelected} Volume Down Button Is Pressed:");
        }

        public void DeviceVolumeUpOperation(int tvSelected)
        {
            _cs.androidTvs[tvSelected - 1].VolumeUp();
            if (DebugEnabled)
                Debug.PrintLine($"TV {tvSelected} Volume Up Button Is Pressed:");
        }

        public void DeviceChannelDownOperation(int tvSelected)
        {
            _cs.androidTvs[tvSelected - 1].ChannelDown();
            if(DebugEnabled)
                Debug.PrintLine($"TV {tvSelected} Channel Down Button Is Pressed:");
        }

        public void DeviceChannelUpOperation(int tvSelected)
        {
            _cs.androidTvs[tvSelected - 1].ChannelUp();
            if(DebugEnabled)
                Debug.PrintLine($"TV {tvSelected} Channel Up Button Is Pressed:");
        }

        public void DeviceMenuBackOperation(int tvSelected)
        {
            _cs.androidTvs[tvSelected - 1].KeyBack();
            if(DebugEnabled)
                Debug.PrintLine($"TV {tvSelected} Back Button Is Pressed:");
        }

        public void DeviceMenuHomeOperation(int tvSelected)
        {
            _cs.androidTvs[tvSelected - 1].MenuHome();
            if(DebugEnabled)
                Debug.PrintLine($"TV {tvSelected} Home Button Is Pressed:");
        }

        public void DeviceMenuInputOperation(int tvSelected)
        {
            _cs.androidTvs[tvSelected - 1].MenuInput();
            if(DebugEnabled)
                Debug.PrintLine($"TV {tvSelected} Power Input Is Pressed:");
        }
        public void GetConfiguration(int tvSelected)
        {
            _cs.androidTvs[tvSelected - 1].GetConfiguration();
        }
        public void DeviceMenuOperation(int tvSelected)
        {
            _cs.androidTvs[tvSelected - 1].Menu();
            if(DebugEnabled)
                Debug.PrintLine($"TV {tvSelected} Menu Button Is Pressed:");
        }

        public void DevicePowering(int tvSelected)
        {
            _cs.androidTvs[tvSelected - 1].KeyPower();
            if (DebugEnabled)
                Debug.PrintLine($"TV {tvSelected} Power Button Is Pressed:");
        }

        public void DevicePairingStart(int tvSelected)
        {
            _ = _cs.androidTvs[tvSelected - 1].StatTVPairingAsync();
            Debug.PrintLine($"Device Pairing Selected TV {tvSelected}");
        }

        public void DevicePairingComplate(string tvCode, int selectedTV)
        {
            Debug.PrintLine($"Device Pairing Selected TV {selectedTV} with TV Code: {tvCode}");
            if (tvCode.Length == 6)
            {
                //_cs.androidTvs[selectedTV - 1].Code = tvCode;
                //_cs.androidTvs[selectedTV-1].IsPaired = true;
                _ = _cs.androidTvs[selectedTV - 1].CompletePairing(tvCode);
                Thread.Sleep(1000);
                _cs.tvConfig.UpdateTvCertificate(selectedTV, _cs.androidTvs[selectedTV - 1].Certificate, true);
                Thread.Sleep(1000);
                _cs.androidTvs[selectedTV - 1].Connect();
                //if successful paired code 
            }
        }
    }
}
