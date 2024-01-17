using AndroidTvControl.Hellpers;
using AndroidTvControl.User_Interface;
using Crestron.RAD.DeviceTypes.SecuritySystem;
using Crestron.RAD.Drivers.SecuritySystem;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.EthernetCommunication;
using RADSecuritySystem;
using System;


namespace AndroidTvControl.Ethernet_Communication
{
    public class EiscClient
    {
        
        private uint _ID;
        private string _ipAddress;
        public string Description { get; set; }
        private EthernetIntersystemCommunications myEisc; // Container for the EISC
        DscPowerSeriesNeoTcp dscPowerSeriesNeoTcp = new DscPowerSeriesNeoTcp();
        

        public int TvSelect { get; set; }
        public string TvCode { get; set; } = String.Empty;

        private ControlSystem _cs;
        // Setup our event handler with our custom event arguments
        public event EventHandler<EiscEventArgs> myEvent;

        public delegate void DigitalChangeEventHandler(SigEventArgs args);

        public event DigitalChangeEventHandler DigitalChangeEvent;

        public delegate void AnalogChangeEventHandler(SigEventArgs args);

        public event AnalogChangeEventHandler AnalogChangeEvent;

        public delegate void SerialChangeEventHandler(SigEventArgs args);

        public event SerialChangeEventHandler SerialChangeEvent;
        public EiscClient(uint ID, ControlSystem cs) // Default constructor
        {
            _ID = ID; // Store the ID we were set to
            _cs = cs;

            myEisc = new EthernetIntersystemCommunications(ID, "127.0.0.2", cs); //127.0.0.2

            myEisc.Description = Description;

            //Subscribe to the events


            if (myEisc.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
            {
                ErrorLog.Error(string.Format("Falied to register the {0}, Reason {1}", myEisc.Name, myEisc.RegistrationFailureReason));
            }
            else
            {
                myEisc.OnlineStatusChange += MyEisc_OnlineStatusChange;
                myEisc.SigChange += MyEisc_SigChange;
                ErrorLog.Notice(string.Format("Eisc Client was sussesfully registred the {0}", myEisc.Description));
            }
        }
        public bool Online() => myEisc.IsOnline;
        public void SetDigital(uint Join, bool Value) => myEisc.BooleanInput[Join].BoolValue = Value;
        public bool GetDigtal(uint Join) => myEisc.BooleanOutput[Join].BoolValue;
        public void SetAnalog(uint Join, int Value) => myEisc.UShortInput[Join].UShortValue = (ushort)Value;
        public uint GetAnalog(uint Join) => myEisc.UShortOutput[Join].UShortValue;
        public uint GetAnalog(MyEiscAnalogJoins Join) => myEisc.UShortOutput[(uint)Join].UShortValue;
        public void SetSerial(uint Join, string Value) => myEisc.StringInput[Join].StringValue = Value;
        public string GetSerial(uint Join) => myEisc.StringOutput[Join].StringValue;

        public int SelectedTV() => (int)GetAnalog((uint)MyEiscAnalogJoins.TV_GetSelectedTV);

        private void MyEisc_SigChange(BasicTriList currentDevice, SigEventArgs args)
        {
            try
            {
                
                var boolValue = args.Sig.BoolValue;
                var number = args.Sig.Number;
                switch (args.Sig.Type)
                {
                    case eSigType.Bool:
                        DigitalChangeEvent(args);
                        if (boolValue == true)
                        {
                            switch (number)
                            {
                                case (uint)MyEiscDigitalJoins.TV_AppPageOperation:
                                case (uint)MyEiscDigitalJoins.TV_PairPageOperation:
                                    Interlock((uint)MyEiscDigitalJoins.TV_AppPageOperation, (uint)MyEiscDigitalJoins.TV_PairPageOperation, number);
                                    PairComplateOperation();
                                    break;
                                case (uint)MyEiscDigitalJoins.TV_PowerToggle:
                                    _cs.tvCommand.DevicePowering(TvSelect);
                                    break;

                                case (uint)MyEiscDigitalJoins.TV_MenuHome:
                                    _cs.tvCommand.DeviceMenuHomeOperation(TvSelect);
                                    break;

                                case (uint)MyEiscDigitalJoins.TV_MenuBack:
                                    _cs.tvCommand.DeviceMenuBackOperation(TvSelect);
                                    break;
                                case (uint)MyEiscDigitalJoins.TV_VolumeUp:
                                    _cs.tvCommand.DeviceVolumeUpOperation(TvSelect);
                                    break;

                                case (uint)MyEiscDigitalJoins.TV_VolumeMute:
                                    _cs.tvCommand.DeviceVolumeMuteOperation(TvSelect);
                                    break;

                                case (uint)MyEiscDigitalJoins.TV_VolumeDown:
                                    _cs.tvCommand.DeviceVolumeDownOperation(TvSelect);
                                    break;

                                case (uint)MyEiscDigitalJoins.TV_PairSubmitCode:
                                    Debug.PrintLine($"TV Pairing Code is: {TvCode}, for the TV Number {TvSelect}");
                                    _cs.tvCommand.DevicePairingComplate(TvCode, (int)TvSelect);
                                    break;

                                case (uint)MyEiscDigitalJoins.TV_StartPairing:
                                    _cs.tvCommand.DevicePairingStart(TvSelect);
                                    break;

                                case (uint)MyEiscDigitalJoins.TV_Backspace:
                                    _cs.tvCommand.DeviceBackspace(TvSelect);
                                    break;

                                case (uint)MyEiscDigitalJoins.TV_DPadKeyUp:
                                    _cs.tvCommand.DeviceDPadKeyUp(TvSelect);
                                    break;

                                case (uint)MyEiscDigitalJoins.TV_DPadKeyDown:
                                    _cs.tvCommand.DeviceDPadKeyDown(TvSelect);
                                    break;

                                case (uint)MyEiscDigitalJoins.TV_DPadKeyLeft:
                                    _cs.tvCommand.DeviceDPadKeyLeft(TvSelect);
                                    break;

                                case (uint)MyEiscDigitalJoins.TV_DPadKeyRight:
                                    _cs.tvCommand.DeviceDPadKeyRight(TvSelect);
                                    break;

                                case (uint)MyEiscDigitalJoins.TV_DPadKeyCenter:
                                    _cs.tvCommand.DeviceDPadKeyCenter(TvSelect);
                                    break;
                                case (uint)MyEiscDigitalJoins.TV_Connect:
                                    _cs.tvCommand.Connect(TvSelect);
                                    break;
                                default:
                                    break;
                            }
                        }

                        break;
                    case eSigType.UShort:
                        AnalogChangeEvent(args);
                        if (args.Sig.BoolValue == true)
                        {
                            switch (number)
                            {
                                case (uint)MyEiscAnalogJoins.TV_GetSelectedTV:

                                    TvSelect = SelectedTV();
                                    //Debug.PrintLine($"Selected TV: {TvSelect}");
                                    SetAnalog((uint)MyEiscAnalogJoins.TV_SetSelectedTV, TvSelect);
                                    var selectedRoomName = _cs.tvConfig.TvData.Configurations[TvSelect - 1].RoomName;
                                    SetSerial((uint)MyEiscSerialJoins.TV_Set_SelectedTV, selectedRoomName);
                                    break;
                                case (uint)MyEiscAnalogJoins.TV_GetTvAppItemClicked:
                                    uint appNumber = GetAnalog(MyEiscAnalogJoins.TV_GetTvAppItemClicked);
                                    _cs.tvCommand.ApplicationStart((int)appNumber - 1, TvSelect);
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    case eSigType.String:
                        SerialChangeEvent(args);
                        switch (number)
                        {
                            //Joins Cascading
                            case (uint)MyEiscSerialJoins.TV_GetTextEntryInput:
                                TvCode = GetSerial(number);
                                //Debug.PrintLine($"TV Code Is: {TvCode}");
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex )
            {
                ErrorLog.Exception("Exeption on MyEisc_SigChange: ", ex);
            } 
        }

        private void MyEisc_OnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
        {
            try
            {
                if (currentDevice.IsOnline)
                {
                    Debug.PrintLine("Eisc Is Registred");
                    SetAnalog((uint)MyEiscAnalogJoins.TV_SetSelectedTV, 0);
                    uint roomStart = 3;
                    uint tvAppJoinStart = 15;
                    var conf = _cs.tvConfig.TvData;
                    SetAnalog((uint)MyEiscAnalogJoins.TV_SetTvCount, conf.Configurations.Count);
                    SetAnalog((uint)MyEiscAnalogJoins.TV_SetTvAppItem, conf.Apps.Count);
                    
                    foreach (var RoomTV in conf.Configurations)
                    {
                        //var text = string.Format($"<FONT size=\"26\" face=\"Crestron Sans Pro\" color=\"#ffffff\"><cips>{RoomTV.RoomName}</cips></FONT>");
                        SetSerial(roomStart, RoomTV.RoomName);
                        roomStart++;
                    }
                    foreach (var TvApp in conf.Apps)
                    {
                        SetSerial(tvAppJoinStart, TvApp.AppName);
                        tvAppJoinStart++;
                    }
                }
            }
            catch (Exception ex) { ErrorLog.Exception("Error Ocured on Eisc Status Change Event: ", ex.InnerException); }
        }
        public void Interlock(uint from, uint to, uint selected)
        {
            if (selected >= from && selected <= to)
            {
                for (uint i = from; i <= to; i++)
                {
                    myEisc.BooleanInput[i].BoolValue = false;
                    if (i == selected)
                        myEisc.BooleanInput[i].BoolValue = true;
                }
            }
        }
        private void PairComplateOperation()
        {
            TvCode = string.Empty;
            SetSerial((uint)MyEiscSerialJoins.TV_GetTextEntryInput, TvCode);
        }
        // Event Handler
        protected virtual void OnRaiseEvent(EiscEventArgs e)
        {
            EventHandler<EiscEventArgs> raiseEvent = myEvent; // make a copy
            if (raiseEvent != null) // do we have subscribers?
            {
                e.Online = Online(); // Set any event variables
                e.ID = _ID;
                e.IpAddress = _ipAddress;

                raiseEvent(this, e);  // Fire the event
            }
        }
    }
}
