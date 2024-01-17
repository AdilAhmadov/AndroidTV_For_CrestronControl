using AndroidTvControl.Antroid_Control_Root;
using System.Threading.Tasks;

namespace AndroidTvControl.Android_Api_Usage
{
    public class AndroidControl
    {
        private string _IpAddress;
        public string Certificate { get; set; }
        public string _MacAddress;
        public bool IsPaired { get; set; } = false;
        private TVPairingClient tVPairingClient;
        private TVClient androidTV;
        public bool IsPowerOn { get; set; }       
        public AndroidControl(string ipAddress, string macAddress)
        {
            this._IpAddress = ipAddress;
            this._MacAddress = macAddress; 
            //androidTV.DebugEnable = debug;
        }
        public void Dispose() => androidTV?.Dispose();
        public void Connect() => androidTV = new TVClient(_IpAddress, Certificate);
        public void Connect(string cert, string ipAddress)
        {
            androidTV = new TVClient(ipAddress, cert);
            androidTV.ConfigurationChanged += AndroidTV_ConfigurationChanged;            
        }
        private void AndroidTV_ConfigurationChanged(object sender, ConfigurationChangedEventArgs e)
        {
            //Debug.PrintLine($"TV Initial Configuration: ModelName: {e.Configuration.ModelName}, Vendor: {e.Configuration.VendorName}" +
            //$"AppName: {e.Configuration.AppName}, Version: {e.Configuration.Version}, CurrentApp: {e.Configuration.CurrentApplication}, IsPowerOn?: {e.Configuration.IsOn}");
            IsPowerOn = e.Configuration.IsOn;
        }
        public async Task StatTVPairingAsync()
        {
            if (!IsPaired)
            {
                tVPairingClient = new TVPairingClient(_IpAddress);
            }
            await tVPairingClient.InitiatePairingAsync();
        }
        public async Task CompletePairing(string code)
        {
            if (code.Length > 0 && code.Length == 6)
            {
                Certificate = await tVPairingClient.CompletePairingAsync(code);
            }
        }
        public void TurnOnTV() => androidTV.TurnOnAsync(_MacAddress); // TV Mac Address
        public void KeyPower()
        {
            _ = androidTV.PressKeyAsync(KeyCodes.KEYCODE_POWER, KeyAction.START_LONG);
        }
        public void VolumeUp()
        {
            _ = androidTV.PressKeyAsync(KeyCodes.KEYCODE_VOLUME_UP, KeyAction.START_LONG);
        }
        public void VolumeMute()
        {
            _ = androidTV.PressKeyAsync(KeyCodes.KEYCODE_VOLUME_MUTE, KeyAction.SHORT);
        }
        public void VolumeDown()
        {
            _ = androidTV.PressKeyAsync(KeyCodes.KEYCODE_VOLUME_DOWN, KeyAction.START_LONG);
        }
        public void MenuInput()
        {
            _ = androidTV.PressKeyAsync(KeyCodes.KEYCODE_TV_INPUT_HDMI_1, KeyAction.START_LONG);
        }
        public void Menu()
        {
            _ = androidTV.PressKeyAsync(KeyCodes.KEYCODE_MENU, KeyAction.START_LONG);
        }
        public void MenuHome()
        {
            _ = androidTV.PressKeyAsync(KeyCodes.KEYCODE_HOME, KeyAction.START_LONG);
        }
        public void KeyBack()
        {
            _ = androidTV.PressKeyAsync(KeyCodes.KEYCODE_BACK, KeyAction.START_LONG);
        }
        public void ChannelUp()
        {
            _ = androidTV.PressKeyAsync(KeyCodes.KEYCODE_CHANNEL_UP, KeyAction.SHORT);
        }
        public void Backspace()
        {
            _ = androidTV.PressKeyAsync(KeyCodes.KEYCODE_DEL, KeyAction.START_LONG);
        }
        public void ChannelDown()
        {
            _ = androidTV.PressKeyAsync(KeyCodes.KEYCODE_CHANNEL_DOWN, KeyAction.SHORT);
        }
        public void SendPress(int code)
        {
            _ = androidTV.PressKeyAsync((KeyCodes)code, KeyAction.START_LONG);
        }
        public void SendShort(int code)
        {
            _ = androidTV.PressKeyAsync((KeyCodes)code, KeyAction.SHORT);
        }
        public void DpadUp()
        {
            _ = androidTV.PressKeyAsync(KeyCodes.KEYCODE_DPAD_UP, KeyAction.START_LONG);
        }
        public void DpadDown()
        {
            _ = androidTV.PressKeyAsync(KeyCodes.KEYCODE_DPAD_DOWN, KeyAction.START_LONG);
        }
        public void DpadLeft()
        {
            _ = androidTV.PressKeyAsync(KeyCodes.KEYCODE_DPAD_LEFT, KeyAction.START_LONG);
        }
        public void DpadRight()
        {
            _ = androidTV.PressKeyAsync(KeyCodes.KEYCODE_DPAD_RIGHT, KeyAction.START_LONG);
        }
        public void DpadCenter()
        {
            _ = androidTV.PressKeyAsync(KeyCodes.KEYCODE_DPAD_CENTER, KeyAction.START_LONG);
        }

        //TV Number Controls
        void Key_0() => _ = androidTV.PressKeyAsync(KeyCodes.KEYCODE_0, KeyAction.SHORT);
        void Key_1() => _ = androidTV.PressKeyAsync(KeyCodes.KEYCODE_1, KeyAction.SHORT);
        void Key_2() => _ = androidTV.PressKeyAsync(KeyCodes.KEYCODE_2, KeyAction.SHORT);
        void Key_3() => _ = androidTV.PressKeyAsync(KeyCodes.KEYCODE_3, KeyAction.SHORT);
        void Key_4() => _ = androidTV.PressKeyAsync(KeyCodes.KEYCODE_4, KeyAction.SHORT);
        void Key_5() => _ = androidTV.PressKeyAsync(KeyCodes.KEYCODE_5, KeyAction.SHORT);
        void Key_6() => _ = androidTV.PressKeyAsync(KeyCodes.KEYCODE_6, KeyAction.SHORT);
        void Key_7() => _ = androidTV.PressKeyAsync(KeyCodes.KEYCODE_7, KeyAction.SHORT);
        void Key_8() => _ = androidTV.PressKeyAsync(KeyCodes.KEYCODE_8, KeyAction.SHORT);
        void Key_9() => _ = androidTV.PressKeyAsync(KeyCodes.KEYCODE_9, KeyAction.SHORT);
        void KeyStar() => _ = androidTV.PressKeyAsync(KeyCodes.KEYCODE_STAR, KeyAction.SHORT);
        void KeyPOUND() => _ = androidTV.PressKeyAsync(KeyCodes.KEYCODE_POUND, KeyAction.SHORT);

        public void AppStart(string url)
        {
            _ = androidTV.StartApplicationAsync(url);
            //"https://www.netflix.com/title.*"
        }
        public void GetConfiguration()
        {
            androidTV.GetConfiguration();
        }    
    }
}
