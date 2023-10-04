using System;

namespace VseriesControllerLibrary_V1
{
    #region General

    public enum ReqCode
    {
        Calibration = 0x01,
        FirmwareUpdate = 0xBA,
        LoopbackPortNumber = 0x05,
    }

    public enum URBState
    {
        SUCCESS = 00,
        UNKNOWN = -1
    }

    public enum DUT_SRC_INFO
    {
        SRC_CAPS = 0x01,
        SINK_CAPS = 0x02,
        BC1p2_Status = 0x03,
        SRC_CAPS_EXT = 0x04,
        Active_CC_line = 0xF0,
        PDC_INFO = 0xF1,
        VDM_UVDM_Data = 0xF6,
        Current_Port_Role = 0xF7,
        Event_Logging_Information = 0xAA,
        Get_SOP1_Response = 0xF8,
    }

    public enum Temperature
    {
        DefaultRange = 00,
        LimitExceeded = 01,
        UnderLimit = 02,
        SensorsOpen = 03,
    }

    public enum BatteryTemperatureStatus
    {
        NotSupported = 0,
        Normal = 1,
        Warning = 2,
        OverTemperature = 3,
    }

    public enum BatteryChargingStatus
    {
        Charging = 0,
        NotCharging = 1,
    }
    public enum APIByte4
    {
        MiscellaneousOperations = 0xF2,
    }
    public enum APIByte3
    {
        PDSystemSettings = 0x03,
    }
    public enum APIByte2
    {
        TestFunctionCardCommands = 0x02,
    }
    public enum APIByte1
    {
        Set = 0x01,
        Programing = 0x02,
        Get = 0x07,
    }

    public enum MiscellaneousOperations
    {
        DEVICE_VERSION = 0x01,
        Clear_Log_Data = 0x02,
        Downgrade_Spec_Revision = 0x03,
        Cable_Tester_Mode = 0x04,
    }

    public enum HardReset
    {
        Send = 0x00,
        DontSend = 0x01,
    }

    public enum ControlCardFirmwareUpdate
    {
        SECOND_STAGE_PROGRAMINING_MODE = 0xF3,
        SECOND_STAGE_BOOT_MODE = 0xF2,
    }

    public enum ResetCommand
    {
        SoftReset = 0,
        HardReset = 1,
    }

    public enum LinkSpeed
    {
        NotConnected = 0x00,
        FullSpeed = 0x01,
        HighSpeed = 0x02,
        SuperSpeed = 0x03,
        Unknown
    }

    public enum USBSpeed
    {
        USB_3_0 = 0,
        USB_2_0 = 1
    }

    public enum SwingType
    {
        NoConfiguration = 0,
        FullSwing = 1,
        LowSwing = 2,
    }

    public enum TxConfigType
    {
        USB_3_0_PHY = 00,
        USB_2_0_PHY = 01,
    }

    public enum DeEmphasisType
    {
        Noconfiguration = 0,
        _3_5_db = 1,
        _6_db = 2
    }

    public enum PreEmphasisType
    {
        Noconfiguration = 0,
        Enable = 1,
        Disable = 2
    }

    public enum Command
    {
        Disable = 0,
        Enable = 1,
    }


    public enum PreEmphasisValue
    {
        Enable = 1,
        Disable = 2
    }
    public enum PortVerifyEnableDisable
    {
        Disable = 0,
        Enable = 1
    }
    public enum USBConnectionStatus
    {
        Unknown,
        Connected,
        Disconnected,
    }
    public enum Loopbackstatus
    {
        NotActive = 0x00,
        Active = 0x01,
    }

    public enum VDM_Type
    {
        UNSTRUCTURED = 0,
        STRUCTURED = 1,
    }

    public enum SOP_Type
    {
        SOP = 0,
        SOP_PRIME = 1,
        SOP_D_PRIME = 2,
        SOP_P_DEBUG = 3,
        SOP_DP_DEBUG = 4,
    }

    public enum FirmwareName
    {
        Controller = 0x01,
        TesterCard = 0x02,
        PD_Controller = 0x03,
        CCG4 = 0x04,
        Eload = 0x05,
        SSBL = 0x06,
        USB_C_Provider = 0x09,
        PPS = 0x08,
        Connectivity_Manager = 0x07,
        AllDeviceFW_V = 0x0F,

    }

    public enum SerialNumber
    {
        ControlCard = 0x01,
        TesterCard = 0x02,
        All_TesterCard = 0x03,
    }

    public enum PD_Controller_Fimware
    {
        CMD_PROGRAM_ROW = 0x39,
        CMD_SEND_DATA = 0x37,
        CMD_ENTER_BOOTLOADER = 0x38,
        CMD_EXIT_BOOTLOADER = 0x3B,
        CMD_START = 0x01,
        CMD_STOP = 0x17,
        CMD_ERASE_ROW = 0x34,
        BASE_CMD_SIZE = 0x07,
    }

    [System.Flags]
    public enum PortID
    {
        NONE = 0,
        Port1 = 1,
        Port2 = 2,
        Port3 = 3,
        Port4 = 4,
        Port5 = 5,
        Port6 = 6,
        Port7 = 7,
        Port8 = 8,
        Port9 = 9,
        Port10 = 10,
        PortAll = 0xF,

    }


    public enum FirmwareUpdateIndication
    {
        None,
        Started,
        Running,
        Stopped,
        Done,
    }

    public enum PD_QC_Mode
    {
        PD = 1,
        QC = 2,
    }
    public enum QC_ModeSwitch
    {
        None = 0,
        QC2p0 = 2,
        QC3p0 = 3,
    }
    public enum Qc_VBUS
    {
        _5V = 2,
        _9V = 3,
        _12V = 4,
        _20V = 5,
    }

    public enum ControllerID
    {
        NONE = 0,
        Controller1 = 1,
        Controller2 = 2,
        Controller3 = 3,
        Controller4 = 4,
        Controller5 = 5,
        Controller6 = 6,
        Controller7 = 7,
        Controller8 = 8,
        Controller9 = 9,
        Controller10 = 10,
    }

    public enum ControllerIndex
    {
        NONE = 0,
        Controller_1 = 1,
        Controller_2 = 2,
        Controller_3 = 3,
        Controller_4 = 4,
        Controller_5 = 5,
    }

    public enum ControllerMode
    {
        Sink = 0x03,
        Source = 0x04,
        DRP = 0x06,
    }

    public enum VCONNFinalTesting
    {
        _250_500 = 0,
        _250_1000 = 1,
        _100_500 = 2,
        _100_1000 = 3,
    }

    public enum CableType
    {
        Special_Cable = 0x01,
        TypeC_Cable = 0x02,
    }

    public enum FanControl
    {
        On = 0,
        Off = 1,
    }


    public enum TCFans
    {
        On = 1,
        Off = 0,
    }

    internal enum CommCommands
    {
        VBUS = 0x06,
        VCONN = 0x07,
        ADC = 0x08,
        HeatSinkTemp = 0x20,
        V_TE_HeatSinkTemp = 0x84,
        SecondStageBootloader = 0xA0,
        FunctionCardPresence = 0x0B,
        VUPState = 0x41,
    }

    public enum LEDControl
    {
        Off = 0x00,
        On = 0x01,
    }

    public enum LED_Color
    {
        none = -1,
        Gray = 0x00,
        Green = 0x01,
        Red = 0x02,

    }



    public enum Location
    {
        BANGALORE = 1,
        Taiwan = 2,
        USA = 3,
    }

    public enum TestExecution
    {
        Stop = 0x00,
        Start = 0x01,
    }

    internal enum VBUS_VCONN_Data
    {
        Current = 0,
        Voltage = 1,
    }

    internal enum Voltage_Current_Data
    {
        VBUS = 0x06,
        VCONN = 0x07,
        ADC = 0x08,
    }


    public enum Ra_Selection
    {
        RaDisable = 0x00,
        RaAssert_CC1 = 0x01,
        RaAssert_CC2 = 0x02,
        RaAssert_ActiveCC = 0x03,
    }

    public enum VCONN_Load_Switch
    {
        VCONN_Load_Disable = 0x00,
        VCONN_Load_CC1 = 0x01,
        VCONN_Load_CC2 = 0x02,
        VCONN_Load_ActiveCC = 0x03,
    }

    public enum PowerSwitch
    {
        Off = 0x00,
        On = 0x01
    }

    public enum APIErrorEnum
    {
        NoError = 0,
        UnknownError = -1,
        USBLinkError = -2,
    }


    #region GPIO Validation
    internal enum GPIOS
    {
        BiColor_LED_Main_link_Comm = 0x01,
        BiColor_LED_LinkSpeed = 0x02,
        BiColor_LED_PD_BC1p2_Status = 0x03,
        Bi_Color_LED_DataErrorIndication = 0x04,
        Green_LED_Data_Lock_Indicator = 0x05,
        Red_LED_PD_NEGOTIATION = 0x06,
        VBUS_SENSE_VOLT_EN = 0x07,
        VBUS_PRESENCE_LED = 0x08,
        VBUS_SHORT = 0x09,
        DP_AUX_4_Switch = 0x0A,
        DPLUS_DMINUS_CONTROL = 0x0B,
        PORT_VERIFICATION = 0xAA,
        Polling_Iteration = 0xAB,
        TC_FAN_CONTROL = 0x0C,
        ELOAD_GPIO_CONTROL = 0x0D,
    

    }

    internal enum LinkCommunication
    {
        Enumeration = 0x01,
        DataTxProgress = 0x02,
        NotConnected = 0x03,
    }


    internal enum PD_BC1p2_Status
    {
        HighSpeed = 0x01,
        FullSpeed = 0x02,
        SuperSpeed = 0x03,
    }

    internal enum DataErrorIndication
    {
        Off = 0,
        PD = 1,
        BC1 = 2,
        On = 3,
    }


    internal enum DataLockIndicator
    {
        LoopBackOn = 0,
        LoopBackOff = 1,
    }



    internal enum PD_NegotationDone
    {
        PDNegFail = 0,
        PDNegPass = 1,
    }

    public enum VBUS_SENSE_VOLT_EN
    {
        ExternalVbus = 0,
        TypeCVbus = 1,
    }

    internal enum VBUS_Presence_LED
    {
        Low = 0,
        High = 1,

    }

    public enum VBUS_SHORT
    {
        Open = 0,
        Shorted = 1,
    }
    internal enum DP_AUX_4_Switch
    {

        CC1 = 0,
        CC2 = 1,
    }



    internal enum LoopbackCommands
    {
        Start = 0x00,
        Stop = 0x01,
        USB_Soft_Disconnect = 0x02,
        USB_Soft_Connect = 0x04,
        USB_2p0_FallBack = 0x05,
        USB_3p0_FallBack = 0x06,
        Reset_Error_Count = 0x07,
    }

    #endregion

    public enum SOPType
    {
        SOP = 0x00,
        SOP_P = 0x01,
        SOP_PP = 0x02,
    }

    public enum MsgType
    {
        Reserved_MessageCode = 0x00,
        GoodCRC_Message = 0x01,
        GotoMin_Message = 0x02,
        Accept_Message = 0x03,
        Reject_Message = 0x04,
        Ping_Message = 0x05,
        PS_RDY_Message = 0x06,
        Get_Source_Cap_Message = 0x07,
        Get_Sink_Cap_Message = 0x08,
        DR_Swap_Message = 0x09,
        PR_Swap_Message = 0x0A,
        VCONN_Swap_Message = 0x0B,
        Wait_Message = 0x0C,
        Soft_Reset_Message = 0x0D,
        Not_SUpported_Message = 0x10,
        Get_Source_Cap_Extended_Message = 0x11,
        Get_Status_message = 0x12,
        FR_Swap_Message = 0x13,
        Get_PPS_Status_Message = 0x14,
        Get_Country_Codes_Message = 0x15,
        SetSoftReset = 0x16,
        SetHardReset = 0x17,

    }

    public enum MsgCategory
    {
        DataMsg = 0x00,
        CtrlMsg = 0x01,
        ExtendedMsg = 0x02,
        OtherMsg = 0x03,
    }

    public enum CmdType
    {
        DataMsg_Request = 0x02,
    }

    public enum PDOSupplyType
    {
        FixedSupply = 0,
        Battery = 1,
        VariableSupply = 2,
        Augmented = 3,
    }
    public enum Attach_Detach_Status_Enum
    {
        Attach = 0x01,
        Detach = 0x00,
    }

    public enum PDOIndex
    {
        Unknown = 0x00,
        PDO1 = 0x01,
        PDO2 = 0x02,
        PDO3 = 0x03,
        PDO4 = 0x04,
        PDO5 = 0x05,
        PDO6 = 0x06,
        PDO7 = 0x07,
    }

    public enum PDContractNegotation
    {
        Failed = 0,
        Successful = 1,
        Unknown
    }

    public enum DataRoleType
    {
        UFP = 0,
        DFP = 1,
        DRP = 2,
    }
    public enum PowerRoleType
    {
        Sink = 0,
        Source = 1,

    }
    public enum IntenalPowerRoleType
    {
        Sink = 0,
        Source = 1,
        Dual = 2,
    }


    public enum StartStop
    {
        Start = 0x01,
        Stop = 0x00,
    }

    public enum ProgrammingMode
    {
        BootModeSelection = 0x01,
        ProgrammingMode = 0x02,
        Directi2cEEPROMWriteFromControlCard = 0x03,
        SecondStageBootloader = 0x04,
    }

    public enum PD_ControllerProgrammingModeSelection
    {
        SWD_Programming = 0x01,
        CCLine_Programming = 0x02,

    }

    public enum ControllerType
    {
        ControlEndPoint = 0,
        BulkEndPoint = 1,
    }


    public enum BulkEndPoint
    {
        Out = 0x01,
        In = 0x81,
        Polling = 0x82,
    }
    public enum OCP_Switch
    {
        Enable = 0xF8,
        Disable = 0xf9,
    }


    #endregion


    #region Calibration
    public enum Card
    {
        Default = 0,
        Control = 0x01,
        Tester = 0x02,
    }

    #endregion

    #region Eload

    public enum EloadProgramingPort
    {
        USB = 0x01,
        UART = 0x02,
    }


    public enum ApplicationType
    {
        V_UP = 0x01,
        V_TE = 0x02,
    }

    internal enum Communication_Phy
    {
        Eth = 1,
        USB = 2,
        UART = 3,
    }

    public enum EloadProgramingMode
    {
        ProgramMode = 0x01,
        BootMode = 0x00
    }
    public enum PPSProgramingMode
    {
        ProgramMode = 0x00,
        BootMode = 0x01,
    }


    public enum StepIncDec
    {
        Inc = 0x01,
        Dec = 0x02,
    }

    public enum StepSelection
    {
        VBUS = 0x01,
        VCONN = 0x04,
    }

    public enum VbusVconnSelection
    {
        None = 0x00,
        VBUS = 0x0F,
        VCONN = 0xFF,

    }

    public enum CCline
    {
        NONE = -1,
        CC1 = 0,
        CC2 = 1,
    }

    public enum VbusEload
    {
        Off = 0,
        On = 1,
    }

    public enum VconnEload
    {
        Off = 0,
        On = 1
    }

    public enum EloadChannels
    {
        None = -1,
        VBUS_Voltage = 0,
        VBUS_Current = 1,
        VBUS_EXT_Voltage = 2,
        VCONN_CC1_Voltage = 3,
        VCONN_CC2_Voltage = 4,
        VCONN_Current = 5,
    }

    public enum PPSChannels
    {
        NONE = -1,
        VBUS_Voltage = 0,
        VBUS_Current = 1,
        VBUS_DAC = 2,
    }

    public enum VBUSModeConfig
    {
        CCMode = 0,
        CRMode = 1,
        //CPMode = 2,
        //CVMode = 3,
    }

    public enum VCONNModeConfig
    {
        CCMode = 0,
        CRMode = 1,
        //CPMode = 2,
        //CVMode = 3,
    }

    public enum TypeCVbusEload
    {
        Off = 0,
        On = 1,
    }

    #endregion

    #region VDM     


    public enum RESPONSE_TYPE
    {
        IGNORE = 0,
        ACK = 1,
        NAK = 2,
    }

    public enum GRL_UID_PID
    {
        MANUFACTURE_VID = 0x227F,
        MANUFACTURE_PID = 0x4,
        LOOPBACK_PID = 0x5,
        CABLE_PID = 0x6,
    }
    #endregion

    #region Source Mode
    /// <summary>
    /// Enum for setting Peak Current Capability in Source PDO 
    /// </summary>
    [Serializable()]
    public enum PeakCurrentCapability
    {
        /// <summary>
        ///  For setting to default peak current 
        /// <BR> Python syntax
        ///  \code {.py}
        ///  GrlPdLib.PeakCurrentCapability.Default_Ioc
        ///  \endcode
        /// </summary>
        Default_Ioc = 0,

        /// <summary>
        ///  For setting to OverloadCapability1
        /// <BR> Python syntax
        ///  \code {.py}
        ///  GrlPdLib.PeakCurrentCapability.OverloadCapability1
        ///  \endcode
        /// </summary>
        OverloadCapability1 = 1,
        /// <summary>
        ///  For setting to OverloadCapability2
        /// <BR> Python syntax
        ///  \code {.py}
        ///  GrlPdLib.PeakCurrentCapability.OverloadCapability2
        ///  \endcode
        /// </summary>

        OverloadCapability2 = 2,

        /// <summary>
        ///  For setting to OverloadCapability3
        /// <BR> Python syntax
        ///  \code {.py}
        ///  GrlPdLib.PeakCurrentCapability.OverloadCapability3
        ///  \endcode
        /// </summary>
        OverloadCapability3 = 3,
    }
    #endregion

    #region PortVerification Channels
    public enum VBUS_Voltage
    {
        _5V = 5,
        _9V = 9,
        _12V = 12,
        _15V = 15,
        _20V = 20,
    }
    public enum VBUS_Current
    {
        _1A = 1,
        _2A = 2,
        _3A = 3,
        _4A = 4,
        _5A = 5,
    }
    public enum VCONN_Voltage
    {
        _1V = 1,
        _2V = 2,
        _3V = 3,
        _4V = 4,
        _5V = 5,
    }
    public enum VCONN_Current
    {
        _0A = 0,
        _250mA = 250,
        _500mA = 500,
        _750mA = 750,
        _1A = 1000,
    }

    #endregion
}
