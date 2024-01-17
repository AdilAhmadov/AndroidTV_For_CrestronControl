using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace AndroidTvControl.User_Interface
{
    public class UserInterface : IJoinHellpers
    {
        private BasicTriListWithSmartObject _currentDevice;

        public delegate void DigitalChangeEventHandler(SigEventArgs args);

        public event DigitalChangeEventHandler DigitalChangeEvent;

        public delegate void AnalogChangeEventHandler(SigEventArgs args);

        public event AnalogChangeEventHandler AnalogChangeEvent;

        public delegate void SerialChangeEventHandler(SigEventArgs args);

        public event SerialChangeEventHandler SerialChangeEvent;

        public UserInterface(BasicTriListWithSmartObject currentDevice)
        {
            _currentDevice = currentDevice;
        }

        public void SetDigitalJoin(uint number, bool value)
        {
            _currentDevice.BooleanInput[number].BoolValue = value;
        }
        public bool GetDigitalJoin(uint join)
        {
            return _currentDevice.BooleanOutput[join].BoolValue;
        }
        public void SetSerialJoin(uint number, string value)
        {
            _currentDevice.StringInput[number].StringValue = value;
        }
        public string GetSerialJoin(uint join)
        {
            return _currentDevice.StringOutput[join].StringValue;
        }
        public void SetAnalogJoin(uint number, ushort value)
        {
            _currentDevice.UShortInput[number].UShortValue = value;
        }
        public ushort GetAnalogJoin(uint join)
        {
            return _currentDevice.UShortOutput[join].UShortValue;
        }
        public void PulseDigitalJoin(uint number)
        {
            _currentDevice.BooleanInput[number].Pulse();
        }
        public void ToggleDigitalJoin(uint number)
        {
            _currentDevice.BooleanInput[number].BoolValue = !_currentDevice.BooleanInput[number].BoolValue;
        }
        public static void Interlock(DeviceBooleanInputCollection input, uint from, uint to, uint selected)
        {
            if (selected >= from && selected <= to)
            {
                for (uint i = from; i <= to; i++)
                {
                    input[i].BoolValue = false;
                    if (i == selected)
                        input[i].BoolValue = true;
                }
            }
        }
    }
}
