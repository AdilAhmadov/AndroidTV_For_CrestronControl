using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace AndroidTvControl.User_Interface
{
    public abstract class BaseUiManager : IJoinHellpers
    {
        public BasicTriListWithSmartObject currentDevice { get; set; }
       
        public void SetDigitalJoin(uint number, bool value)
        {
            currentDevice.BooleanInput[number].BoolValue = value;
        }
        public bool GetDigitalJoin(uint join)
        {
            return currentDevice.BooleanOutput[join].BoolValue;
        }
        public void SetSerialJoin(uint number, string value)
        {
            currentDevice.StringInput[number].StringValue = value;
        }
        public string GetSerialJoin(uint join)
        {
            return currentDevice.StringOutput[join].StringValue;
        }
        public void SetAnalogJoin(uint number, ushort value)
        {
            currentDevice.UShortInput[number].UShortValue = value;
        }
        public ushort GetAnalogJoin(uint join)
        {
            return currentDevice.UShortOutput[join].UShortValue;
        }
        public void PulseDigitalJoin(uint number)
        {
            currentDevice.BooleanInput[number].Pulse();
        }
        public void SO_InterlockTab(uint from, uint to, uint selected)
        {
            if (selected >= from && selected <= to)
            {
                for (uint i = from; i <= to; i++)
                {
                    currentDevice.SmartObjects[3].BooleanInput[string.Format($"Tab Button {i} Select")].BoolValue = false;
                    if (i == selected)
                        currentDevice.SmartObjects[3].BooleanInput[string.Format($"Tab Button {i} Select")].BoolValue = true;
                }
            }
        }
        public void SO_Interlock(uint from, uint to, uint selected, uint SoId)
        {
            if (selected >= from && selected <= to)
            {
                for (uint i = from; i <= to; i++)
                {
                    currentDevice.SmartObjects[SoId].BooleanInput[string.Format($"Item {i} Selected")].BoolValue = false;
                    if (i == selected)
                        currentDevice.SmartObjects[SoId].BooleanInput[string.Format($"Item {i} Selected")].BoolValue = true;
                }
            }
        }
        public void ToggleDigitalJoin(uint number)
        {
            currentDevice.BooleanInput[number].BoolValue = !currentDevice.BooleanInput[number].BoolValue;
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
        public void Interlock(uint from, uint to, uint selected)
        {
            if (selected >= from && selected <= to)
            {
                for (uint i = from; i <= to; i++)
                {
                    currentDevice.BooleanInput[i].BoolValue = false;
                    if (i == selected)
                        currentDevice.BooleanInput[i].BoolValue = true;
                }
            }
        }
        public uint SelectedPage { get; set; } = 1;
        private void NextPage()
        {
            if (SelectedPage < 7)
            {
                SetDigitalJoin(SelectedPage, false);
                SelectedPage++;
                SetDigitalJoin(SelectedPage, true);
            }

        }
        private void PreviousPage()
        {

            if (SelectedPage > 3)
            {
                SetDigitalJoin(SelectedPage, false);
                SelectedPage--;
                SetDigitalJoin(SelectedPage, true);
            }
        }
    }
}
