namespace AndroidTvControl.User_Interface
{
    public enum MyEiscDigitalJoins : uint
    {
        TV_PowerToggle = 1,
        TV_MenuHome,
        TV_MenuBack,
        TV_Backspace,
        TV_VolumeUp,
        TV_VolumeMute,
        TV_VolumeDown,
        TV_DPadKeyUp,
        TV_DPadKeyDown,
        TV_DPadKeyLeft,
        TV_DPadKeyRight,
        TV_DPadKeyCenter,
        TV_StartPairing,
        TV_Connect,
        TV_PairSubmitCode,
        TV_AppPageOperation,
        TV_PairPageOperation,
    }
    public enum MyEiscSerialJoins : uint
    {
        TV_GetTextEntryInput = 1,

        TV_Set_RoomName_01 = 3,
        TV_Set_RoomName_02,
        TV_Set_RoomName_03,
        TV_Set_RoomName_04,
        TV_Set_RoomName_05,
        TV_Set_RoomName_06,
        TV_Set_RoomName_07,

        TV_Set_AppName_01 = 15,
        TV_Set_AppName_02,
        TV_Set_AppName_03,
        TV_Set_AppName_04,
        TV_Set_AppName_05,
        TV_Set_AppName_06,
        TV_Set_AppName_07,
        TV_Set_AppName_08,
        TV_Set_AppName_09,
        TV_Set_AppName_10,
        TV_Set_AppName_11,
        TV_Set_AppName_12,
        TV_Set_AppName_13,
        TV_Set_AppName_14,
        TV_Set_AppName_15,
        TV_Set_AppName_16,
        TV_Set_AppName_17,
        TV_Set_AppName_18,
        TV_Set_AppName_19,
        TV_Set_AppName_20,
        TV_Set_AppName_21,
        TV_Set_AppName_22,
        TV_Set_AppName_23,
        TV_Set_AppName_24,
        TV_Set_AppName_25,
        TV_Set_AppName_26,
        TV_Set_AppName_27,
        TV_Set_AppName_28,
        TV_Set_AppName_29,
        TV_Set_AppName_30,
        TV_Set_AppName_31,
        TV_Set_AppName_32,
        TV_Set_AppName_33,
        TV_Set_AppName_34,
        TV_Set_AppName_35,
        TV_Set_AppName_36,
        TV_Set_AppName_37,
        TV_Set_AppName_38,
        TV_Set_AppName_39,
        TV_Set_AppName_40,

        TV_Set_ConnectionStatus = 60,
        TV_Set_SelectedTV = 61,
    }
    public enum MyEiscAnalogJoins:uint
    {
        TV_SetTvCount = 1,
        TV_SetSelectedTV = 2,
        TV_GetSelectedTV = 3,
        TV_SetTvAppItem = 4,
        TV_GetTvAppItemClicked = 5,
    }
    public enum MyPanelSO
    {
        TVRoomSelectionBtnListSG = 1,
        TVApplicationSelectionSG = 2,
        TVDPadControlSG = 3
    }
    public enum MyDigitalJoins
    {
        AppLancher = 1,
        TVPairring,
        StartTVParring,
        Backspase,
        Pair,
        GetConfiguration,
        TVConnect,
        TVPowerBtn = 11,
        TVHomeBtn,
        TVBackBtn,
        TVBackspase,
        TvVolUpBtn,
        TvVolMuteBtn,
        TvVolDnBtn,
        TVChUpBtn,
        TVChDnBtn,
        TVAPPLunchSubmit=22,
        TVCommandPress,
        TVCommandShort,    
    }
    public enum MySerialJoins
    {
        TvPairCodeInput = 1,
        TvStatusFeedback,
    }
}
