using AndroidTvControl.Android_Api_Usage;
using AndroidTvControl.Configuration_Manager;
using AndroidTvControl.Ethernet_Communication;
using AndroidTvControl.Hellpers;
using AndroidTvControl.Program_Root;
using AndroidTvControl.User_Interface;
using Crestron.SimplSharp;                          	// For Basic SIMPL# Classes
using Crestron.SimplSharpPro;                       	// For Basic SIMPL#Pro classes
using Crestron.SimplSharpPro.CrestronThread;            // For Threading
using Crestron.SimplSharpPro.UI;
using System;
using System.Collections.Generic;
using VirtualConsoleApp;

namespace AndroidTvControl
{
    public class ControlSystem : CrestronControlSystem
    {
        public static UiManager UI;
        public XpanelForSmartGraphics myPanel;
        public EiscClient myEisc;
        public List<AndroidControl> androidTvs;
        private UserInterface userInterface;
        public ConfigurationManager tvConfig;
        public TvCommands tvCommand;
        public bool DebugEnabled { get; set; } = false;
        public ControlSystem() : base()
        {
            try
            {
                Thread.MaxNumberOfUserThreads = 300;

                //Subscribe to the controller events (System, Program, and Ethernet)
                CrestronEnvironment.SystemEventHandler += new SystemEventHandler(_ControllerSystemEventHandler);
                CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(_ControllerProgramEventHandler);
                CrestronEnvironment.EthernetEventHandler += new EthernetEventHandler(_ControllerEthernetEventHandler);
                ChekForVC4();
            }
            catch (Exception e) { ErrorLog.Error("Error in the constructor: {0}", e.Message); }
        }

        private void ChekForVC4()
        {
            if (InitialParametersClass.ControllerPromptName == "VC-4")
            {
                VirtualConsole.Start(45000);
                this.AddConsoleCommand();
            }
        }
        private void AddConsoleCommand()
        {
            VirtualConsole.AddNewConsoleCommand(Config, "Config", "Loading New Configuration File");
        }
        private string Config(string name)
        {
            tvConfig.GetConfig();
            return "Command Executed";
        }
        public override void InitializeSystem()
        {
            try
            {
                myPanel = new XpanelForSmartGraphics(0x40, this);
                UI = new UiManager(myPanel, this);
                tvCommand = new TvCommands(this, this.DebugEnabled);
                userInterface = new UserInterface(myPanel);
                userInterface.DigitalChangeEvent += UserInterfaceDigitalChangeEvent;
                userInterface.SerialChangeEvent += UserInterfaceSerialChangeEvent;
                userInterface.AnalogChangeEvent += UserInterfaceAnalogChangeEvent;

                if (!InitialParametersClass.ControllerPromptName.Equals("VC-4"))
                {
                    myEisc = new EiscClient(0x14, this);
                    myEisc.Description = "Eisc Server to Program 1";
                    myEisc.AnalogChangeEvent += MyEisc_AnalogChangeEvent;
                    myEisc.DigitalChangeEvent += MyEisc_DigitalChangeEvent;
                    myEisc.SerialChangeEvent += MyEisc_SerialChangeEvent;
                }

                tvConfig = new ConfigurationManager();
                tvConfig.OnConfigCreated += TvConfig_OnConfigCreated;
                tvConfig.GetConfig();
            }
            catch (Exception e) { ErrorLog.Error("Error in InitializeSystem: {0}", e.Message); }
        }

        private void TvConfig_OnConfigCreated(object sender, LoadConfigEventArgs args)
        {
            try
            {
                uint appJoin = Factory.AppStartJoins;
                uint roomName = Factory.RoomNameStartJoins;
                if (args.ErrorMessage == "Success")
                {
                    androidTvs = new List<AndroidControl>();
                    foreach (var config in args.Configurations)
                    {
                        if (!string.IsNullOrEmpty(config.IpAddress) && !string.IsNullOrEmpty(config.Mac))
                        {
                            androidTvs.Add(new AndroidControl(config.IpAddress, config.Mac));
                            //Debug.PrintLine($"RoomName: {config.RoomName} with Ip address is added to List {config.IpAddress} \n");
                        }
                    }
                    for (int i = 0; i < androidTvs.Count; i++)
                    {
                        var certificate = args.Configurations[i].Certificate;

                        if (certificate.Length > 0)
                        {
                            androidTvs[i].Certificate = certificate;
                            androidTvs[i].Connect(certificate, args.Configurations[i].IpAddress);
                            //Debug.PrintLine($"TV with Room Name:{args.Configurations[i].RoomName} has been Connected with Certificate {certificate} \n");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLog.Exception($"Exeption on TvConfig_OnConfigCreated", ex);
            }
        }
        void UserInterfaceAnalogChangeEvent(SigEventArgs args)
        {

        }
        void UserInterfaceSerialChangeEvent(SigEventArgs args)
        {

        }
        void UserInterfaceDigitalChangeEvent(SigEventArgs args)
        {

        }
        void MyEisc_SerialChangeEvent(SigEventArgs args)
        {

        }
        void MyEisc_DigitalChangeEvent(SigEventArgs args)
        {

        }
        void MyEisc_AnalogChangeEvent(SigEventArgs args)
        {

        }
        void _ControllerEthernetEventHandler(EthernetEventArgs ethernetEventArgs)
        {
            switch (ethernetEventArgs.EthernetEventType)
            {
                case (eEthernetEventType.LinkDown):
                    if (ethernetEventArgs.EthernetAdapter == EthernetAdapterType.EthernetLANAdapter)
                    {
                    }
                    break;
                case (eEthernetEventType.LinkUp):
                    if (ethernetEventArgs.EthernetAdapter == EthernetAdapterType.EthernetLANAdapter)
                    {

                    }
                    break;
            }
        }
        void _ControllerProgramEventHandler(eProgramStatusEventType programStatusEventType)
        {
            switch (programStatusEventType)
            {
                case (eProgramStatusEventType.Paused):
                    break;
                case (eProgramStatusEventType.Resumed):
                    break;
                case (eProgramStatusEventType.Stopping):
                    androidTvs.ForEach(x => x.Dispose());
                    break;
            }

        }
        void _ControllerSystemEventHandler(eSystemEventType systemEventType)
        {
            switch (systemEventType)
            {
                case (eSystemEventType.DiskInserted):
                    //Removable media was detected on the system
                    break;
                case (eSystemEventType.DiskRemoved):
                    //Removable media was detached from the system
                    break;
                case (eSystemEventType.Rebooting):
                    //The system is rebooting. 
                    //Very limited time to preform clean up and save any settings to disk.
                    break;
            }
        }
    }
}