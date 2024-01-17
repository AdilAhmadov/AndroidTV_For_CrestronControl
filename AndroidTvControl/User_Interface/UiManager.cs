using AndroidTvControl.Hellpers;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;

namespace AndroidTvControl.User_Interface
{
    public class UiManager : BaseUiManager
    {
        #region Panel Constractos and Registrations
        private ControlSystem _cs;
        private string sgdFilePath = Path.Combine(Directory.GetApplicationDirectory(), "AndroidTVUi_XPanel.sgd");
        private CCriticalSection PickerCritical;
        private CCriticalSection LoaderCritical;
        public string TvCode { get; set; } = String.Empty;
        public int TvSelect { get; set; }
        public UiManager(BasicTriListWithSmartObject _currentDevice, ControlSystem cs)
        {
            this.currentDevice = _currentDevice;
            this._cs = cs;
            this.Add();
            this.Register();
        }
        private void Add()
        {
            this.currentDevice.OnlineStatusChange += new OnlineStatusChangeEventHandler(myPanel_StatusChange);
            this.currentDevice.SigChange += new SigEventHandler(myPanel_SigChange);
        }
        private void Register()
        {
            if (currentDevice.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
            {
                ErrorLog.Error(string.Format("Falied to register the {0}, Reason {1}", currentDevice.Name, currentDevice.RegistrationFailureReason));
            }
            else
            {
                if (File.Exists(sgdFilePath))
                {
                    currentDevice.LoadSmartObjects(sgdFilePath);
                    foreach (KeyValuePair<uint, SmartObject> SO in currentDevice.SmartObjects)
                    {
                        SO.Value.SigChange += new SmartObjectSigChangeEventHandler(SO_ValueSigChange);
                        //Debug.PrintLine(string.Format("SGD File with Smart graphics ID {0} is Loaded", SO.Value.ID));
                    }
                }
                else
                {
                    ErrorLog.Error("Error while finding SGD File make shure if it is copied to application directory");
                }
                PickerCritical = new CCriticalSection();
            }
        }      
        #endregion

        #region Timer and Callback Functions
        private CTimer timer;
        private CTimer StartupTimer;
        int SaveTimeoutMs = 30000; //30000

        #endregion

        #region Panel Status and SigChange Events
        private void SO_ValueSigChange(GenericBase currentDevice, SmartObjectEventArgs args)
        {
            BasicTriListWithSmartObject activeTP = (BasicTriListWithSmartObject)currentDevice;
            switch (args.SmartObjectArgs.ID)
            {
                case (int)MyPanelSO.TVRoomSelectionBtnListSG: SG_TVRoomSelectionBtnListSG_SigChange(activeTP, args); break;
                case (int)MyPanelSO.TVDPadControlSG: SG_TVDPadControlSG_SigChange(activeTP, args); break;
                case (int)MyPanelSO.TVApplicationSelectionSG: SG_TVApplicationSelectionSG_SigChange(activeTP, args); break;
            }
        }
        private void myPanel_StatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
        {
            try
            {
                if (currentDevice.IsOnline)
                {
                    Debug.PrintLine("Touch Panel Is Registred");

                    var activeTP = this.currentDevice;

                    var SoID = (uint)MyPanelSO.TVRoomSelectionBtnListSG; //
                    activeTP.SmartObjects[SoID].UShortInput["Set Num of Items"].UShortValue = (ushort)_cs.tvConfig.TvData.Configurations.Count;
                    activeTP.SmartObjects[(uint)MyPanelSO.TVApplicationSelectionSG].UShortInput["Set Number of Items"].UShortValue = (ushort)_cs.tvConfig.TvData.Apps.Count;

                    uint tvCount = 1;
                    uint tvAppCount = 1;
                    foreach (var RoomTV in _cs.tvConfig.TvData.Configurations)
                    {
                        activeTP.SmartObjects[SoID].StringInput[string.Format($"Item {tvCount} Text")].StringValue = RoomTV.RoomName;
                        tvCount++;
                    }
                    foreach (var TvApp in _cs.tvConfig.TvData.Apps)
                    {
                        activeTP.SmartObjects[(uint)MyPanelSO.TVApplicationSelectionSG].StringInput[string.Format($"Set Item {tvAppCount} Text")].StringValue = TvApp.AppName;
                        tvAppCount++;
                    }
                }
            }
            catch (Exception ex) { ErrorLog.Exception("Error Ocured on Panel Status Change Event: ", ex); }
        }
        private void myPanel_SigChange(BasicTriList currentDevice, SigEventArgs args)
        {            
            try
            {
                var boolValue = args.Sig.BoolValue;
                var sigNumber = args.Sig.Number;
                switch (args.Sig.Type)
                {
                    case eSigType.Bool:
                        if (boolValue == true)
                        {
                            switch (sigNumber)
                            {
                                case (uint)MyDigitalJoins.AppLancher:
                                case (uint)MyDigitalJoins.TVPairring:
                                    Interlock((uint)MyDigitalJoins.AppLancher, (uint)MyDigitalJoins.TVPairring, sigNumber);
                                    PairComplateOperation();
                                    break;
                                case (uint)MyDigitalJoins.TVPowerBtn:
                                    _cs.tvCommand.DevicePowering(TvSelect);
                                    break;
                                case (uint)MyDigitalJoins.TVHomeBtn:
                                    _cs.tvCommand.DeviceMenuHomeOperation(TvSelect);
                                    break;
                                case (uint)MyDigitalJoins.TVBackBtn:
                                    _cs.tvCommand.DeviceMenuBackOperation(TvSelect);
                                    break;

                                case (uint)MyDigitalJoins.TVBackspase:
                                    _cs.tvCommand.DeviceBackspace(TvSelect);
                                    break;
                                case (uint)MyDigitalJoins.TvVolUpBtn:
                                    _cs.tvCommand.DeviceVolumeUpOperation(TvSelect);
                                    break;
                                case (uint)MyDigitalJoins.TvVolMuteBtn:
                                    _cs.tvCommand.DeviceVolumeMuteOperation(TvSelect);
                                    break;
                                case (uint)MyDigitalJoins.TvVolDnBtn:
                                    _cs.tvCommand.DeviceVolumeDownOperation(TvSelect);
                                    break;
                                case (uint)MyDigitalJoins.StartTVParring:
                                    _cs.tvCommand.DevicePairingStart(TvSelect);
                                    break;
                                case (uint)MyDigitalJoins.Pair:
                                    _cs.tvCommand.DevicePairingComplate(TvCode, TvSelect);
                                    PairComplateOperation();
                                    break;
                                case (uint)MyDigitalJoins.GetConfiguration:
                                    _cs.tvCommand.GetConfiguration(TvSelect);
                                    break;
                                case (uint)MyDigitalJoins.TVConnect:
                                    _cs.tvCommand.Connect(TvSelect);
                                    break;
                                case (uint)MyDigitalJoins.TVChUpBtn:
                                    _cs.tvCommand.DeviceChannelUpOperation(TvSelect);
                                    break;
                                case (uint)MyDigitalJoins.TVChDnBtn:
                                    _cs.tvCommand.DeviceChannelDownOperation(TvSelect);
                                    break;
                                case (uint)MyDigitalJoins.TVAPPLunchSubmit:
                                    Debug.PrintLine(GetSerialJoin(3));
                                    _cs.androidTvs[TvSelect - 1].AppStart(GetSerialJoin(3));
                                    break;
                                case (uint)MyDigitalJoins.TVCommandPress:
                                    _cs.androidTvs[TvSelect - 1].SendPress(Convert.ToInt16(GetSerialJoin(4)));
                                    break;
                                case (uint)MyDigitalJoins.TVCommandShort:
                                    _cs.androidTvs[TvSelect - 1].SendShort(Convert.ToInt16(GetSerialJoin(4)));
                                    break;
                            }
                        }
                        break;

                    case eSigType.UShort:

                        break;
                    case eSigType.String:
                        switch (args.Sig.Number)
                        {
                            case 1:
                                TvCode = GetSerialJoin((uint)MySerialJoins.TvPairCodeInput);
                                if (TvCode.Length > 0)
                                {
                                    Debug.PrintLine($"TV Code Is: {TvCode}");
                                }

                                break;
                        }
                        break;
                }
            }
            catch (Exception ex) { ErrorLog.Exception("Error in myPanel_SigChange Event: ", ex); }
        }

        private void PairComplateOperation()
        {
            TvCode = string.Empty;
            SetSerialJoin((uint)MySerialJoins.TvPairCodeInput, TvCode);
        }

        private void SG_TVApplicationSelectionSG_SigChange(BasicTriListWithSmartObject activeTP, SmartObjectEventArgs args)
        {
            try
            {
                if (args.Sig.Name == "Item Clicked" && args.Sig.BoolValue)
                {
                    uint appNumber = args.SmartObjectArgs.UShortOutput["Item Clicked"].UShortValue;
                    //SO_Interlock(1, 20, appNumber, (uint)MyPanelSO.TVApplicationSelectionSG);
                    _cs.tvCommand.ApplicationStart((int)appNumber - 1, TvSelect);
                }
            }
            catch (Exception ex)
            {
                ErrorLog.Exception("Exeption on SG_TVApplicationSelectionSG_SigChange", ex.InnerException);
            }
           
        }       
        private void SG_TVDPadControlSG_SigChange(BasicTriListWithSmartObject activeTP, SmartObjectEventArgs args)
        {
            if (args.Sig.BoolValue)
            {
                switch (args.Sig.Name)
                {
                    case "Up":
                        _cs.tvCommand.DeviceDPadKeyUp(TvSelect);                        
                        break;
                    case "Down":
                        _cs.tvCommand.DeviceDPadKeyDown(TvSelect);                      
                        break;
                    case "Left":
                        _cs.tvCommand.DeviceDPadKeyLeft(TvSelect);                       
                        break;
                    case "Right":
                        _cs.tvCommand.DeviceDPadKeyRight(TvSelect);                      
                        break;
                    case "Center":
                        _cs.tvCommand.DeviceDPadKeyCenter(TvSelect);                       
                        break;
                default:
                        break;
                }
            }
        }
        private void SG_TVRoomSelectionBtnListSG_SigChange(BasicTriListWithSmartObject activeTP, SmartObjectEventArgs args)
        {
            if (args.Sig.Name == "Item Clicked" && args.Sig.BoolValue)
            {
                uint numberTvCount = args.SmartObjectArgs.UShortOutput["Item Clicked"].UShortValue;

                this.currentDevice.SmartObjects[(uint)MyPanelSO.TVRoomSelectionBtnListSG].UShortInput["Select Item"].UShortValue = (ushort)numberTvCount;
                //Debug.PrintLine($"Room Number Selected: {numberTvCount}");
                TvSelect = (int)numberTvCount;
                var selectedRoomName = _cs.tvConfig.TvData.Configurations[(int)numberTvCount - 1].RoomName;
                SetSerialJoin(3, selectedRoomName);
            }
        }
        #endregion
    }
}
