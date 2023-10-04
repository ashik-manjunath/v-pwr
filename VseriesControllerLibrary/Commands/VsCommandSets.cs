using System.Reflection;
using VseriesControllerLibrary_V1.CommunicationModule;
using VseriesControllerLibrary_V1.HelperModules.PDO_Decoder;
using VseriesControllerLibrary_V1.SysData;
using VseriesControllerLibrary_V1.UnStructure_VDM_Message;
using VseriesControllerLibrary_V1.VDM_Message;

namespace VseriesControllerLibrary_V1.Commands.HelperModules.PollingModules
{
    internal class VsCommandSets
    {
        #region Private Properties
        private const int MAX_BUFFER_SIZE = 768;
        private const int DATA_OFFSET = 5;
        private const byte _byteSet = (byte)APIByte1.Set;
        private const byte _byteGet = (byte)APIByte1.Get;
        private const byte _byteProgramming = (byte)APIByte1.Programing;
        private const byte CYRET_ERR_DATA = 0x04;
        private const byte CYRET_SUCCESS = 0x00;
        private const byte CYRET_ERR_LENGTH = 0x03;
        private const byte CYRET_ERR_CMD = 0x05;
        private const ushort MIN_SIZE = 6;
        private const byte CONTROLLER_RESET = 0xF5;
        private const int PRIMARY_FW_START = 0x8000;
        private const int CONFM_CCG3P_KEYWORD = 0xFA;
        private const int SOP_1_Rx = 0xD9;
        private bool cableFlip = false;
        private Comm_Read_Write comm_Read_Write;
        private byte arrayID = 0;
        private byte checkSum = 0;
        private ushort rowNumber = 0;
        private ushort size = 0;
        private readonly byte[] rowData = new byte[128];
        private Dictionary<PortID, Attach_Detach_Status_Enum> _listAttachDetachStatus;
        private Dictionary<PortID, Attach_Detach_Status_Enum> _testerCardStatus;
        //private static bool flushDataOnce = false;
        private static readonly object padlockSystemStatus = new object();
        private LEDSystemStatus _ledSystemStatus;
        private bool isPollingRunning;

        #endregion

        #region Public Properties

        /// <summary>
        /// This holds the number of bytes transferd during firmware update
        /// </summary>
        public double DataSentBytes { get; private set; }

        /// <summary>
        /// This will stop the firmware execution 
        /// </summary>
        public bool StopFWUpdate { get; set; }

        /// <summary>
        /// this is to limit the number of DR swap retires 
        /// </summary>
        public int Initiate_CCLine_DR_Swap_Once { get; set; }

        /// <summary>
        /// this is to limit the number of retry in th CCLine firmware update retires 
        /// </summary>
        public int Initiate_CCline_Boot_Once { get; set; }

        /// <summary>
        /// This holds the error for 
        /// </summary>
        public string API_Error { get; set; } = "No Error";

        public string IpAddress
        {
            get
            {
                return comm_Read_Write.SocketConnection.IpAddress;
            }
        }
        public bool IsOpen
        {
            get
            {
                return comm_Read_Write.SocketConnection.IsOpen;
            }
        }
        public bool IsRetry
        {
            get
            {
                return comm_Read_Write.SocketConnection.IsRetry;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Default Constructor
        /// </summary>
        public VsCommandSets()
        {
            _listAttachDetachStatus = new Dictionary<PortID, Attach_Detach_Status_Enum>();
            _ledSystemStatus = new LEDSystemStatus();
            comm_Read_Write = new Comm_Read_Write();
            HelperModule.SelectedCalbleType = CableType.TypeC_Cable;
        }
        #endregion

        #region Public Modules

        #region Communication Modules 
        ///// <summary>
        ///// Initilize USB device  
        ///// </summary>
        ///// <param name="cyFX3Device"></param>
        ///// <returns></returns>
        //public bool InitilizeController(CyFX3Device cyFX3Device = null)
        //{
        //    try
        //    {
        //        cyFX3Device.Reset();
        //        var retVal = comm_Read_Write.USBLinkComm.USBCommunication.Connect(cyFX3Device);

        //        if (!flushDataOnce)
        //        {

        //            if (cyFX3Device != null && cyFX3Device.FriendlyName.Contains("Manufacture Tester"))
        //            {
        //                ClearReadBuffer();
        //                flushDataOnce = true;
        //            }
        //        }

        //        return retVal;
        //    }
        //    catch (Exception ex)
        //    {
        //        HelperModule.Debug(MethodBase.GetCurrentMethod().Name, ex);
        //    }
        //    return false;
        //}

        /// <summary>
        /// Initialize Ethernet device 
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public bool InitilizeController(string ipAddress = "192.168.255.1")
        {
            try
            {
                comm_Read_Write = new Comm_Read_Write();
                var retVal = comm_Read_Write.SocketConnection.Open(ipAddress);

                return retVal;
            }
            catch (Exception ex)
            {
                Debug(DebugType.DEBUG, MethodBase.GetCurrentMethod().Name, ex);
            }
            return false;
        }

        public bool DisconnectContoller()
        {
            try
            {
                if (comm_Read_Write != null)
                {
                    return comm_Read_Write.SocketConnection.Close();
                }
            }
            catch (Exception ex)
            {
                Debug(DebugType.DEBUG, MethodBase.GetCurrentMethod().Name, ex);
            }
            return false;
        }

        public DateTime GetLastCommandTime()
        {
            return comm_Read_Write.SocketConnection.GetLastCommadTime();
        }

        /// <summary>
        /// Initialize UART device
        /// </summary>
        /// <param name="baudRate"></param>
        /// <param name="comPort"></param>
        /// <returns></returns>
        public bool InitilizeController(int baudRate = 115200, string comPort = "")
        {
            try
            {
                if (comm_Read_Write == null)
                {
                    comm_Read_Write = new Comm_Read_Write();
                }
                return false;
                //var retVal = comm_Read_Write.UartConnection.Open(baudRate, comPort);

                //return retVal;
            }
            catch (Exception ex)
            {
                DebugLogger.Instance.WriteToDebugLogger(DebugType.DEBUG, MethodBase.GetCurrentMethod().Name, ex);
            }
            return false;
        }

        public USBConnectionStatus USBConnectionStatus
        {
            get
            {
                return USBConnectionStatus.Disconnected;
                //return comm_Read_Write.USBLinkComm.USBCommunication.USBConnectionStatus;
            }
        }
        //public USBDeviceList GetUSBDevices()
        //{
        //    return new USBDeviceList(CyConst.DEVICES_CYUSB);
        //}

        #endregion

        #region Generic 

        #region Set Commands 

        #region Sink API 
        public int Detach(PortID port)
        {
            int retValue = -1;
            try
            {

                byte[] dataBuffer = new byte[] { 0x00, 0x00, 0x02, 0x03, 0x01 };

                int iByte0 = (((int)port) << 4) | _byteSet;

                dataBuffer[0] = (byte)(iByte0);
                dataBuffer[1] = (byte)(dataBuffer.Length - 2);
                bool status = comm_Read_Write.Write(dataBuffer, MethodBase.GetCurrentMethod().Name);
                if (status)
                    retValue = (int)APIErrorEnum.NoError;
                else
                    retValue = (int)APIErrorEnum.UnknownError;
            }
            catch (Exception ex)
            {
                Debug(DebugType.DEBUG, MethodBase.GetCurrentMethod().Name, ex);
            }
            return retValue;
        }
        public int Attach(PortID port)
        {
            int retValue = -1;
            try
            {

                byte[] dataBuffer = new byte[] { 0x00, 0x00, 0x02, 0x03, 0x02 };

                int iByte0 = (((int)port) << 4) | _byteSet;

                dataBuffer[0] = (byte)(iByte0);
                dataBuffer[1] = (byte)(dataBuffer.Length - 2);
                bool status = comm_Read_Write.Write(dataBuffer, MethodBase.GetCurrentMethod().Name);
                if (status)
                    retValue = (int)APIErrorEnum.NoError;
                else
                    retValue = (int)APIErrorEnum.UnknownError;
            }
            catch (Exception ex)
            {
                Debug(DebugType.DEBUG, MethodBase.GetCurrentMethod().Name, ex);
            }
            return retValue;
        }
        public int PDTesterModeSourceSetting(PortID port)
        {
            byte[] dataBuffer = new byte[] { 0x00, 0x00, 0x02, 0x03, 0x04 };

            int iByte0 = (((int)port) << 4) | _byteSet;

            dataBuffer[0] = (byte)(iByte0);
            dataBuffer[1] = (byte)(dataBuffer.Length - 2);
            bool status = comm_Read_Write.Write(dataBuffer, MethodBase.GetCurrentMethod().Name);
            int retValue;
            if (status)
                retValue = (int)APIErrorEnum.NoError;
            else
                retValue = (int)APIErrorEnum.UnknownError;

            return retValue;
        }
        public int PDMessageConfig(PortID port, PDOIndex PdoIndex = PDOIndex.Unknown, double MaxOp_Current_Power_Voltage = 0, double Op_current_power = 0)
        {
            int retValue = (int)APIErrorEnum.UnknownError;
            try
            {
                int count;
                SourceCapabilities sourceCaps = DecoderSourceCaps(port);
                if (sourceCaps == null || sourceCaps.PDOlist.Count == 0)
                    return -1;

                byte[] dataBuffer = new byte[] { 0x00, 0x00, 0x02, 0x03, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };


                dataBuffer[0] = (byte)((((int)port) << 4) | _byteSet);
                dataBuffer[1] = (byte)(dataBuffer.Length - 2);
                dataBuffer[5] = ((byte)SOPType.SOP << 4) | 0x02;

                if (sourceCaps.PDOlist.Count > (int)(PdoIndex - 1))
                {
                    PDO pDO = sourceCaps.PDOlist.Find(x => x.PDO_Index == (int)PdoIndex);
                    if (pDO != null)
                    {
                        dataBuffer[6] = (byte)((byte)pDO.PdoType << 4 | (byte)PdoIndex);
                        if (pDO.PdoType == PDOSupplyType.FixedSupply || pDO.PdoType == PDOSupplyType.VariableSupply)
                        {
                            count = (int)((MaxOp_Current_Power_Voltage * 1000.0) / 10);
                            dataBuffer[7] = (byte)(count);
                            dataBuffer[8] = (byte)(count >> 8);

                            count = (int)((Op_current_power * 1000.0) / 10);
                            dataBuffer[9] = (byte)(count);
                            dataBuffer[10] = (byte)(count >> 8);
                        }
                        else if (pDO.PdoType == PDOSupplyType.Battery)
                        {
                            count = (int)((MaxOp_Current_Power_Voltage * 1000.0) / 250);
                            dataBuffer[7] = (byte)(count);
                            dataBuffer[8] = (byte)(count >> 8);

                            count = (int)((Op_current_power * 1000.0) / 250);
                            dataBuffer[9] = (byte)(count);
                            dataBuffer[10] = (byte)(count >> 8);
                        }
                        else if (pDO.PdoType == PDOSupplyType.Augmented)
                        {
                            count = (int)((MaxOp_Current_Power_Voltage * 1000.0) / 20);
                            dataBuffer[7] = (byte)(count);
                            dataBuffer[8] = (byte)(count >> 8);

                            count = (int)((Op_current_power * 1000.0) / 50);
                            dataBuffer[9] = (byte)(count);
                            dataBuffer[10] = (byte)(count >> 8);
                        }
                    }
                    bool status = comm_Read_Write.Write(dataBuffer, MethodBase.GetCurrentMethod().Name);
                    if (status)
                        retValue = (int)APIErrorEnum.NoError;
                    else
                        retValue = (int)APIErrorEnum.UnknownError;
                }
            }
            catch (Exception ex)
            {
                Debug(DebugType.DEBUG, MethodBase.GetCurrentMethod().Name, ex);
            }
            return retValue;
        }
        public int PDCommandInit(PortID port, MsgCategory msgCategory, SOPType Sop, MsgType msgType)
        {

            byte[] dataBuffer = new byte[] { 0x00, 0x00, 0x02, 0x03, 0x10, 0x00, 0x00 };

            int iByte0 = (((int)port) << 4) | _byteSet;

            dataBuffer[0] = (byte)(iByte0);
            dataBuffer[1] = (byte)(dataBuffer.Length - 2);
            dataBuffer[5] = (byte)((((int)msgCategory) << 4) | (int)Sop);
            dataBuffer[6] = (byte)(msgType);

            bool status = comm_Read_Write.Write(dataBuffer, MethodBase.GetCurrentMethod().Name);
            int retValue;
            if (status)
                retValue = (int)APIErrorEnum.NoError;
            else
                retValue = (int)APIErrorEnum.UnknownError;

            return retValue;
        }
        public int TesterModeSetting(PortID port)
        {
            byte[] dataBuffer = new byte[] { 0x00, 0x00, 0x02, 0x04, 0x01 };

            int iByte0 = (((int)port) << 4) | _byteSet;

            dataBuffer[0] = (byte)(iByte0);
            dataBuffer[1] = (byte)(dataBuffer.Length - 2);
            bool status = comm_Read_Write.Write(dataBuffer, MethodBase.GetCurrentMethod().Name);
            int retValue;
            if (status)
                retValue = (int)APIErrorEnum.NoError;
            else
                retValue = (int)APIErrorEnum.UnknownError;

            return retValue;
        }
        public int PowerControlCommand(PortID port, PowerSwitch switchOnOff)
        {
            //Tester card power switch
            List<byte> dataBuffer = new List<byte>
            {
                 // Byte 0 - Default value to set data app commands 
                (byte)((((int)port) << 4) | _byteSet),

                // Byte 1 - PayLoad length
                0x03,

                // Byte 2 - Test Function Card Commands
                0x01,
                      // Byte 2 - Power control card
                0x01,

                (byte)switchOnOff,
        };

            if (comm_Read_Write.Write(dataBuffer.ToArray(), MethodBase.GetCurrentMethod().Name))
                return (int)APIErrorEnum.NoError;
            else
                return (int)APIErrorEnum.UnknownError;

        }
        public int RaSelection(PortID port, Ra_Selection ra_Selection)
        {

            List<byte> dataBuffer = new List<byte>
            {
                 //Byte 0 - Default value to set data app commands 
                (byte)((((int)port) << 4) | _byteSet),

                //Byte 1 - PayLoad length
                0x03,

                //Byte 2 - Test Function Card Commands
                0x02,

                //Byte 3 - Ra Command
                0xF1,

                //Byte 4 - Ra Selection
                (byte)ra_Selection,

            };

            bool status = comm_Read_Write.Write(dataBuffer.ToArray(), MethodBase.GetCurrentMethod().Name);
            int retValue;
            if (status)
                retValue = (int)APIErrorEnum.NoError;
            else
                retValue = (int)APIErrorEnum.UnknownError;
            return retValue;
        }
        public int VCONNLoadSwitch(PortID port, VCONN_Load_Switch vCONN_Load_Switch)
        {
            // 0x11, 0x03, 0x02, 0xF2, 0x02
            List<byte> dataBuffer = new List<byte>
            {
                 //Byte 0 - Default value to set data app commands 
                (byte)((((int)port) << 4) | _byteSet),

                //Byte 1 - PayLoad length
                0x03,

                //Byte 2 - Test Function Card Commands
                0x02,

                //Byte 3 - VCONN Load Command
                0xF2,

                //Byte 4 - VCONN Load Selection
                (byte)vCONN_Load_Switch,

            };

            bool status = comm_Read_Write.Write(dataBuffer.ToArray(), MethodBase.GetCurrentMethod().Name);
            int retValue;
            if (status)
                retValue = (int)APIErrorEnum.NoError;
            else
                retValue = (int)APIErrorEnum.UnknownError;
            return retValue;
        }
        public int CableSelection(PortID portID, CableType cableType)
        {
            //11 03 02 F5 02
            byte[] dataBuffer = new byte[]
            {
                 //Byte 0 - Default value to set data app commands 
                (byte)((((int)portID) << 4) | _byteSet),

                //Byte 1 - PayLoad length
                0x03,

                //Byte 2 - Test Function Card Commands
                0x02,

                //Byte 3 - Cable Type Selection
                0xF5,

                //Byte 4 - VCONN Load Selection
                (byte)cableType,

            };
            bool status = comm_Read_Write.Write(dataBuffer.ToArray(), MethodBase.GetCurrentMethod().Name);
            int retValue;
            if (status)
            {
                HelperModule.SelectedCalbleType = cableType;
                retValue = (int)APIErrorEnum.NoError;
            }
            else
                retValue = (int)APIErrorEnum.UnknownError;
            return retValue;

        }
        public int InitiatePollingTimer(StartStop startStop)
        {
            if (HelperModule.AppType == ApplicationType.V_UP)
            {

                byte[] dataBuffer = new byte[]
               {
                 //Byte 0 - Default value to set data app commands 
                _byteSet,

                //Byte 1 - PayLoad length
                0x03,

                //Byte 2 - Test Function Card Commands
                0x01,

                //Byte 3 - Polling Timer Control
                0x03,

                //Byte 3 - start stop timer
                (byte)startStop,

               };


                bool status = comm_Read_Write.Write(dataBuffer, MethodBase.GetCurrentMethod().Name);
                int retValue;
                if (status)
                    retValue = (int)APIErrorEnum.NoError;
                else
                    retValue = (int)APIErrorEnum.UnknownError;
                return retValue;
            }
            else
            {
                return 0;
            }
        }

        public int BiColor_LED_Main_Link_Comm(PortID portID, LinkCommunication comm)
        {
            return GPIOSValidation(portID, GPIOS.BiColor_LED_Main_link_Comm, (byte)comm);
        }
        public int BiColor_LED_LinkSpeed(PortID portID, LinkSpeed comm)
        {
            return GPIOSValidation(portID, GPIOS.BiColor_LED_LinkSpeed, (byte)comm);
        }
        public int BiColor_LED_PD_BC1p2_Status(PortID portID, PD_BC1p2_Status comm)
        {
            return GPIOSValidation(portID, GPIOS.BiColor_LED_PD_BC1p2_Status, (byte)comm);
        }
        public int Bi_Color_LED_DataErrorIndication(PortID portID, DataErrorIndication comm)
        {
            return GPIOSValidation(portID, GPIOS.Bi_Color_LED_DataErrorIndication, (byte)comm);
        }
        public int Green_LED_Data_Lock_Indicator(PortID portID, DataLockIndicator comm)
        {
            return GPIOSValidation(portID, GPIOS.Green_LED_Data_Lock_Indicator, (byte)comm);
        }
        public int Red_LED_PD_NEGOTIATION(PortID portID, PD_NegotationDone comm)
        {
            return GPIOSValidation(portID, GPIOS.Red_LED_PD_NEGOTIATION, (byte)comm);
        }
        public int VBUS_SENSE_VOLT_EN(PortID portID, VBUS_SENSE_VOLT_EN comm)
        {
            return GPIOSValidation(portID, GPIOS.VBUS_SENSE_VOLT_EN, (byte)comm);
        }
        public int VBUS_PRESENCE_LED(PortID portID, VBUS_Presence_LED comm)
        {
            return GPIOSValidation(portID, GPIOS.VBUS_PRESENCE_LED, (byte)comm);
        }
        public int VBUS_SHORT(PortID portID, VBUS_SHORT comm)
        {
            return GPIOSValidation(portID, GPIOS.VBUS_SHORT, (byte)comm);
        }
        public int DP_AUX_4_Switch(PortID portID, DP_AUX_4_Switch comm)
        {
            return GPIOSValidation(portID, GPIOS.DP_AUX_4_Switch, (byte)comm);
        }

        public bool Polling_Iteration_Control(PortID portID, bool GetBatteryStatus, uint pollingTimeinMilliSeconds)
        {
            try
            {
                uint value6 = 1;
                if (pollingTimeinMilliSeconds > 100)
                {
                    value6 = (pollingTimeinMilliSeconds / 100);
                }


                byte value5 = 0;
                if (GetBatteryStatus)
                    value5 = 1;

                byte[] dataBuffer = new byte[]
                {
                //Byte 0 - Default value to get data app commands 
                 (byte)((((int)portID) << 4) | _byteSet),

                 //byte 1 -  Length of the payload
                 0x04,

                 //byte 2 - Test function card commands
                 0x02,

                 //byte 3 - GPIO's Validating
                 0xF3,

                 // byte 4 - GPIO selection bit
                 (byte)GPIOS.Polling_Iteration,
                 

                 // byte 5 - Based on 4th byte selection this will change
                 value5,

                 // number iteration depends on polling data
                 (byte)value6,

                     };

                return comm_Read_Write.Write(dataBuffer, MethodBase.GetCurrentMethod().Name);
            }
            catch (Exception ex)
            {
                HelperModule.Debug("Polling_Iteration_Control", ex);
            }

            return false;

        }
        public bool ControllerReset()
        {
            bool retVal = false;
            try
            {
                if (HelperModule.AppType == ApplicationType.V_UP)
                {
                    //ControllerType tempStore = comm_Read_Write.USBLinkComm.USBCommunication.ControllerType;
                    //comm_Read_Write.USBLinkComm.USBCommunication.ControllerType = ControllerType.ControlEndPoint;
                    //byte[] dataBuffer = new byte[1];
                    //comm_Read_Write.Write(dataBuffer, MethodBase.GetCurrentMethod().Name, CONTROLLER_RESET);
                    //comm_Read_Write.USBLinkComm.USBCommunication.ControllerType = tempStore;
                    //retVal = true;
                    retVal = false;
                }
                else if (HelperModule.AppType == ApplicationType.V_TE)
                {
                    byte[] dataBuffer = new byte[] { 0x11, 0x02, 0x01, 0x04 };
                    dataBuffer[1] = (byte)(dataBuffer.Length - 2);
                    retVal = comm_Read_Write.Write(dataBuffer, "CONTROLLER_RESET");
                }
            }
            catch (Exception ex)
            {
                retVal = false;
                Debug(DebugType.DEBUG, MethodBase.GetCurrentMethod().Name, ex);
            }

            return retVal;
        }
        public bool ControllerProgramingMode(ControlCardFirmwareUpdate controlCardFirmwareUpdate)
        {
            //ControllerType tempStore = comm_Read_Write.USBLinkComm.USBCommunication.ControllerType;
            //comm_Read_Write.USBLinkComm.USBCommunication.ControllerType = ControllerType.ControlEndPoint;
            //byte[] dataBuffer = new byte[1];
            //bool retVal = comm_Read_Write.Write(dataBuffer, MethodBase.GetCurrentMethod().Name, (byte)controlCardFirmwareUpdate);
            //comm_Read_Write.USBLinkComm.USBCommunication.ControllerType = tempStore;
            //return retVal;
            return false;
        }
        public int USBLoopBackCommands(PortID port, LoopbackCommands loopbackCommands)
        {
            byte[] dataBuffer = new byte[] { 0x01, 0x00, 0x02, 0x01, 0x00 };
            int iByte0 = (((int)port) << 4) | _byteSet;
            dataBuffer[0] = (byte)(iByte0);
            dataBuffer[1] = (byte)(dataBuffer.Length - 2);
            dataBuffer[4] = (byte)loopbackCommands;
            bool status = comm_Read_Write.Write(dataBuffer, MethodBase.GetCurrentMethod().Name);
            int retValue;
            if (status)
                retValue = (int)APIErrorEnum.NoError;
            else
                retValue = (int)APIErrorEnum.UnknownError;
            return retValue;
        }
        public bool ConfigureUSBswing_deEmphasis(PortID port, USBSpeed uSBSpeed, SwingType swingType, int swingValue,
           DeEmphasisType deEmphasisType, int deEmphasisValue, PreEmphasisType preEmphasisType, PreEmphasisValue preEmphasisValue)
        {
            //byte 6 - swing value(Should be less that 128 and the default value is 90)
            if (swingValue > 128)
                swingValue = 90;
            //DE - emphasis value(should be less that 0XIF and the default is 0x11)
            if (deEmphasisValue > 0x1F)
                deEmphasisValue = 0x11;
            byte[] dataBuffer = new byte[] { 0x11, 0x09, 0x02, 0x0C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            int iByte0 = (((int)port) << 4) | _byteSet;
            dataBuffer[0] = (byte)(iByte0);
            dataBuffer[1] = (byte)(dataBuffer.Length - 2);
            dataBuffer[4] = (byte)uSBSpeed;
            dataBuffer[5] = (byte)swingType;
            dataBuffer[6] = (byte)swingValue;
            dataBuffer[7] = (byte)deEmphasisType;
            dataBuffer[8] = (byte)deEmphasisValue;
            dataBuffer[9] = (byte)preEmphasisType;
            dataBuffer[10] = (byte)preEmphasisValue;
            bool retVal = comm_Read_Write.Write(dataBuffer, MethodBase.GetCurrentMethod().Name);
            return retVal;
        }
        public bool SetTemperatureLimit(int temperatureLimit)
        {
            if (temperatureLimit < 60)
                temperatureLimit = 60;
            if (temperatureLimit > 90)
                temperatureLimit = 90;
            byte[] dataBuffer = new byte[] { _byteSet, 0x03, 0x01, 0x05, 0x00 };
            dataBuffer[4] = (byte)temperatureLimit;
            bool status = comm_Read_Write.Write(dataBuffer, MethodBase.GetCurrentMethod().Name);
            return status;
        }
        public bool SetEloadAutomaticaly(PortID port, Command autoEload)
        {
            bool retValue;
            try
            {
                byte[] dataBuffer = new byte[] { 0x00, 0x00, 0x02, 0x0A, 0x00 };
                int iByte0 = (((int)port) << 4) | _byteSet;
                dataBuffer[0] = (byte)(iByte0);
                dataBuffer[1] = (byte)(dataBuffer.Length - 2);
                dataBuffer[4] = (byte)autoEload;
                retValue = comm_Read_Write.Write(dataBuffer, MethodBase.GetCurrentMethod().Name);
            }
            catch (Exception ex)
            {
                Debug(DebugType.DEBUG, MethodBase.GetCurrentMethod().Name, ex);
                retValue = false;
            }
            return retValue;
        }
        public bool ClearPDControllerLogData(PortID portID)
        {
            bool retValue;
            try
            {
                byte[] dataBuffer = new byte[] { 0x00, 0x00, (byte)APIByte2.TestFunctionCardCommands, (byte)APIByte3.PDSystemSettings, (byte)APIByte4.MiscellaneousOperations, 0x00 };
                int iByte0 = (((int)portID) << 4) | _byteSet;
                dataBuffer[0] = (byte)(iByte0);
                dataBuffer[1] = (byte)(dataBuffer.Length - 2);
                dataBuffer[5] = (byte)MiscellaneousOperations.Clear_Log_Data;
                retValue = comm_Read_Write.Write(dataBuffer, MethodBase.GetCurrentMethod().Name);
                if (retValue)
                    HelperModule.Debug(dataBuffer, $"{MethodBase.GetCurrentMethod().Name} -  {DateTime.Now}");
            }
            catch (Exception ex)
            {
                Debug(DebugType.DEBUG, MethodBase.GetCurrentMethod().Name, ex);
                retValue = false;
            }
            return retValue;
        }
        public bool ConfigureVDMResponse(PortID portID, VendorDefinedMessage msg, RESPONSE_TYPE rESPONSE_TYPE)
        {
            bool retValue = false;
            try
            {
                List<byte> totalPayLoad = new List<byte>();
                byte _6thByte = (byte)rESPONSE_TYPE;
                if (msg != null && msg.GetByteData().Count > 0)
                {
                    totalPayLoad = HelperModule.ConvertUNITtoBYTE(msg.GetByteData().GetRange(0, 1), 2);
                    List<byte> payloadByte = HelperModule.ConvertUNITtoBYTE(msg.GetByteData().GetRange(1, msg.GetByteData().Count - 1), 4);
                    totalPayLoad.AddRange(payloadByte);
                    HelperModule.Debug(totalPayLoad.ToArray(), "PayLoadByte");
                    _6thByte |= (byte)((payloadByte.Count / 4) << 4);
                }
                List<byte> dataBuffer = new List<byte>()
                {
                    // Port number and set byte
                    (byte)((((int)portID) << 4) | _byteSet), 
                    
                    // Indicate Tester card command
                    0x02,
                    
                    // PD System Command 
                    0x03,
                    
                    // PD Message Config
                    0x05,
                    // Data Message ACK
                    0x0F1,
                    // number of VDM and SOP interrupt
                    _6thByte,
                     };
                dataBuffer.AddRange(totalPayLoad);
                dataBuffer.Insert(1, (byte)dataBuffer.Count);
                HelperModule.Debug(dataBuffer.ToArray(), "PayLoadByte");
                retValue = comm_Read_Write.Write(dataBuffer.ToArray(), MethodBase.GetCurrentMethod().Name);
            }
            catch (Exception ex)
            {
                HelperModule.Debug(MethodBase.GetCurrentMethod().Name, ex);
            }
            return retValue;
        }
        public int PortVerification(PortID portID, PortVerifyEnableDisable portVerifyEnableDisable, CCline cCline, PowerRoleType powerRoleType)
        {
            byte byteValue = (byte)(((int)portVerifyEnableDisable & 1) | (((int)cCline & 1) << 1) | (((int)powerRoleType & 1) << 2));
            return GPIOSValidation(portID, GPIOS.PORT_VERIFICATION, byteValue);
        }
        public bool ConfigureCableTester(PortID port, Command command)
        {
            byte[] dataBuffer = new byte[] { 0x00, 0x00, (byte)APIByte2.TestFunctionCardCommands, (byte)APIByte3.PDSystemSettings, (byte)APIByte4.MiscellaneousOperations, 0x00, 0x00 };
            int iByte0 = (((int)port) << 4) | _byteSet;
            dataBuffer[0] = (byte)iByte0;
            dataBuffer[1] = (byte)(dataBuffer.Length - 2);
            dataBuffer[5] = (byte)MiscellaneousOperations.Cable_Tester_Mode;
            dataBuffer[6] = (byte)command;
            return comm_Read_Write.Write(dataBuffer, MethodBase.GetCurrentMethod().Name);
        }

        #endregion

        #region Source API / V-TE PWR
        public bool Set_Controller_Mode(ControllerMode controllerMode)
        {
            try
            {
                // Last byte is for firmware validation since payload length should be 4 as per the FW team requirment
                if (PD_QC_SwtichMode(PortID.Port1, controllerMode, PD_QC_Mode.PD, QC_ModeSwitch.None))
                {
                    Sleep(3500);

                    DataPowerRole dataPowerRole = Get_Data_Power_Role(PortID.Port1);
                    if (dataPowerRole.IntenalPowerRole == IntenalPowerRoleType.Source && controllerMode == ControllerMode.Source)
                    {
                        HelperModule.Debug("Controller mode is set to source");
                        return true;
                    }
                    else if (dataPowerRole.IntenalPowerRole == IntenalPowerRoleType.Sink && controllerMode == ControllerMode.Sink)
                    {
                        HelperModule.Debug("Controller mode is set to sink");
                        return true;

                    }
                    else if (dataPowerRole.IntenalPowerRole == IntenalPowerRoleType.Dual && controllerMode == ControllerMode.DRP)
                    {
                        HelperModule.Debug("Controller mode is set to DRP");
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
            }
            catch (Exception ex)
            {
                Debug(DebugType.DEBUG, MethodBase.GetCurrentMethod().Name, ex);
            }
            return false;
        }
        public bool Set_Voltage_Pps(double Vbus_in_Volt)
        {
            return Set_Voltage_Pps(Vbus_in_Volt, PPSChannels.NONE);
        }
        public bool Set_Voltage_Pps(double Vbus_in_Volt, PPSChannels isCalib)
        {
            try
            {
                uint voltMV = (uint)(Vbus_in_Volt * 1000);
                byte lsb = (byte)(voltMV & 0xFF);
                byte msb = (byte)((voltMV >> 8) & 0xFF);

                byte[] dataBuffer = new byte[] { 0x01, 0x00, 0x02, 0xB1, 0x02, 0x01, 0x0A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                int iByte0 = (((int)PortID.Port1) << 4) | _byteSet;
                dataBuffer[0] = (byte)(iByte0);
                dataBuffer[1] = (byte)(dataBuffer.Length - 2);
                dataBuffer[8] = lsb;
                dataBuffer[9] = msb;
                if (isCalib == PPSChannels.NONE)
                {
                    dataBuffer[7] = 0x00;
                }
                else if (isCalib == PPSChannels.VBUS_Voltage || isCalib == PPSChannels.VBUS_DAC)
                {
                    dataBuffer[7] = 0xCA;
                }
                else if (isCalib == PPSChannels.VBUS_Current)
                {
                    dataBuffer[7] = 0xCB;
                }
                else
                {
                    dataBuffer[7] = 0x00;
                }
                return comm_Read_Write.Write(dataBuffer, MethodBase.GetCurrentMethod().Name);
            }
            catch (Exception ex)
            {
                Debug(DebugType.DEBUG, MethodBase.GetCurrentMethod().Name, ex);
            }
            return false;
        }
        public bool SetSourceCapability(PortID port, ConfigureSourceCapability objSourceCapability)
        {
            try
            {
                List<uint> listPayload = objSourceCapability.GetByteData();
                byte value1 = (byte)((((int)port) << 4) | _byteSet);


                //Define the byte array(dataBuffer) consist of Header/payload/code word for config SrcCap
                List<byte> dataBuffer = new List<byte>()
                {
                    // Port number and Set byte value 
                    value1,

                    0x02,

                    0x03,

                    // Keyword for source update 
                    0x20,

                    // Number of PDO 
                    (byte)listPayload.Count,
                };

                //Update payload into dataBuffer
                for (int i = 0; i < listPayload.Count; i++)
                {
                    dataBuffer.Add((byte)((listPayload[i]) & 0xFF));
                    dataBuffer.Add((byte)((listPayload[i] >> 8) & 0xFF));
                    dataBuffer.Add((byte)((listPayload[i] >> 16) & 0xFF));
                    dataBuffer.Add((byte)((listPayload[i] >> 24) & 0xFF));
                }

                dataBuffer.Insert(1, (byte)dataBuffer.Count()); //payload byte count

                return comm_Read_Write.Write(dataBuffer.ToArray(), MethodBase.GetCurrentMethod().Name);
            }
            catch (Exception ex)
            {
                Debug(DebugType.DEBUG, MethodBase.GetCurrentMethod().Name, ex);
            }
            return false;
        }

        public bool SetSinkCapability(PortID port, ConfigureSinkCapability objSinkCapability)
        {
            try
            {
                List<uint> listPayload = objSinkCapability.GetByteData();
                byte value1 = (byte)((((int)port) << 4) | _byteSet);

                //Define the byte array(dataBuffer) consist of Header/payload/code word for configure SrcCap
                List<byte> dataBuffer = new List<byte>()
                {
                    // Port number and Set byte value  // PD Message Configuration 
                    value1, 0x00, 0x02, 0x03, 0x05,
                    // DATA_MSG_SNK_CAP // Number of PDO 
                    0x04,  (byte)listPayload.Count,
                };

                //Update payload into dataBuffer
                for (int i = 0; i < listPayload.Count; i++)
                {
                    dataBuffer.Add((byte)((listPayload[i]) & 0xFF));
                    dataBuffer.Add((byte)((listPayload[i] >> 8) & 0xFF));
                    dataBuffer.Add((byte)((listPayload[i] >> 16) & 0xFF));
                    dataBuffer.Add((byte)((listPayload[i] >> 24) & 0xFF));
                }

                dataBuffer[1] = (byte)dataBuffer.Count(); //payload byte count
                return comm_Read_Write.Write(dataBuffer.ToArray(), MethodBase.GetCurrentMethod().Name);
            }
            catch (Exception ex)
            {
                Debug(DebugType.DEBUG, MethodBase.GetCurrentMethod().Name, ex);
            }
            return false;
        }

        #endregion


        #region QC 2.0 / 3.0

        public bool PD_QC_SwtichMode(PortID port, ControllerMode controllerMode, PD_QC_Mode pD_QC_SwtichModes, QC_ModeSwitch qC_ModeSwitch = QC_ModeSwitch.None)
        {

            try
            {
                byte[] dataBuffer = new byte[] { 0x11, 0x05, 0x02, 0x03, (byte)controllerMode, (byte)pD_QC_SwtichModes, (byte)qC_ModeSwitch };
                int iByte0 = (((int)port) << 4) | _byteSet;
                dataBuffer[0] = (byte)(iByte0);
                dataBuffer[1] = (byte)(dataBuffer.Length - 2);
                if (comm_Read_Write.Write(dataBuffer, MethodBase.GetCurrentMethod().Name))
                {
                    if (qC_ModeSwitch == QC_ModeSwitch.QC2p0)
                    {
                        //HelperModule.AddStatusUpdate("please wait 2.5 seconds for enable the QC2.0");
                        // if user selection QC2.0 it wait for 2.5 Seconds
                        Sleep(2500);
                    }
                    else if (qC_ModeSwitch == QC_ModeSwitch.QC3p0)
                    {
                        // HelperModule.AddStatusUpdate("please wait 3.5 Seconds for  enable the QC3.0");
                        // if user selection QC3.0 it wait for 3.5 Seconds
                        Sleep(3500);
                    }
                    else if (qC_ModeSwitch == QC_ModeSwitch.None)
                    {

                        // if user  is None it wait for 1.5 Seconds
                        Sleep(1500);
                    }
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug(DebugType.DEBUG, MethodBase.GetCurrentMethod().Name, ex);
            }
            return false;
        }

        public bool Set_VBUS_Voltage_QC_Mode_2p0(PortID port, Qc_VBUS qc_VBUS)
        {
            try
            {
                byte[] dataBuffer = new byte[] { 0x11, 0x06, 0x02, 0x03, 0xF2, (byte)GPIOS.DPLUS_DMINUS_CONTROL, 0x01, (byte)qc_VBUS };
                int iByte0 = (((int)port) << 4) | _byteSet;
                dataBuffer[0] = (byte)(iByte0);
                dataBuffer[1] = (byte)(dataBuffer.Length - 2);
                return comm_Read_Write.Write(dataBuffer, MethodBase.GetCurrentMethod().Name);
            }
            catch (Exception ex)
            {
                Debug(DebugType.DEBUG, MethodBase.GetCurrentMethod().Name, ex);
            }
            return false;
        }
        public bool Set_VBUS_Voltage_QC_Mode_3p0(PortID port, double voltage)
        {
            try
            {
                byte lsbValue = 0;
                byte msbValue = 0;

                if (voltage > 20 || voltage < 3.7)
                    return false;

                voltage *= 1000;// voltage = voltage *1000;
                lsbValue = (byte)((int)voltage & 0xFF);//lsb 
                msbValue = (byte)(((int)voltage >> 8) & 0xFF);// msb 

                byte[] dataBuffer = new byte[] { 0x21, 0x07, 0x02, 0x03, 0xF2, (byte)GPIOS.DPLUS_DMINUS_CONTROL, 0x01, lsbValue, msbValue };
                int iByte0 = (((int)port) << 4) | _byteSet;
                dataBuffer[0] = (byte)(iByte0);
                dataBuffer[1] = (byte)(dataBuffer.Length - 2);
                return comm_Read_Write.Write(dataBuffer, MethodBase.GetCurrentMethod().Name);

            }
            catch (Exception ex)
            {
                Debug(DebugType.DEBUG, MethodBase.GetCurrentMethod().Name, ex);
            }
            return false;
        }

        // OCP API
        public bool Set_OCP_Trigger_Value(PortID portID, uint ocpValueInPercentage, OCP_Switch oCP_Switch)
        {
            try
            {
                byte lsbValue = 0;
                byte msbValue = 0;

                ocpValueInPercentage += 100;

                if (oCP_Switch == OCP_Switch.Enable)
                {

                    if (ocpValueInPercentage > 100 && ocpValueInPercentage < 200)
                    {
                        lsbValue = (byte)((int)ocpValueInPercentage & 0xFF);//lsb 
                        msbValue = (byte)(((int)ocpValueInPercentage >> 8) & 0xFF);// msb 
                    }
                    else
                    {
                        HelperModule.Debug("OCP value is incorrect" + ocpValueInPercentage);
                        return false;
                    }
                }

                byte[] dataBuffer = new byte[] { 0x11, 0x08, 0x02, 0xB1, 0x02, 0x01, (byte)oCP_Switch, msbValue, lsbValue };
                int iByte0 = (((int)portID) << 4) | _byteSet;
                dataBuffer[0] = (byte)(iByte0);
                dataBuffer[1] = (byte)(dataBuffer.Length - 2);
                return comm_Read_Write.Write(dataBuffer, MethodBase.GetCurrentMethod().Name);

            }
            catch (Exception ex)
            {
                Debug(DebugType.DEBUG, MethodBase.GetCurrentMethod().Name, ex);
            }
            return false;


        }
        #endregion

        #endregion

        #region Get Commands

        public string GetSystemSerialNumber()
        {
            string serialNumber = "";
            try
            {
                byte[] byte1 = new byte[] { _byteGet, 0x01, 0x02 };
                bool status = comm_Read_Write.Read(ref byte1, MethodBase.GetCurrentMethod().Name);
                for (int i = 1; i <= byte1[0]; i++)
                {
                    if ((byte1[i] != 0) && status)
                    {
                        serialNumber += Convert.ToChar(byte1[i]).ToString();
                    }
                }
                HelperModule.Debug(serialNumber);
            }
            catch (Exception ex)
            {
                HelperModule.Debug(MethodBase.GetCurrentMethod().Name, ex);
            }
            return serialNumber;
        }
        public Dictionary<PortID, Attach_Detach_Status_Enum> AttachDetachStatus()
        {
            _listAttachDetachStatus = new Dictionary<PortID, Attach_Detach_Status_Enum>();
            byte[] dataBuffer = new byte[] { 0x07, 0x01, 0x09, 0x00, 0x00 };
            bool status = comm_Read_Write.Read(ref dataBuffer, MethodBase.GetCurrentMethod().Name);
            if (status)
            {
                int orVal = 1;
                for (int i = 0; i < 8; i++)
                {
                    var sdStatus = (dataBuffer[3] & orVal) >> i;
                    orVal *= 2;
                    _listAttachDetachStatus.Add((PortID)(i + 1), (Attach_Detach_Status_Enum)sdStatus);
                }

                var port9 = (dataBuffer[4] & 0x01);
                _listAttachDetachStatus.Add(PortID.Port9, (Attach_Detach_Status_Enum)port9);
                var port10 = ((dataBuffer[4] & 0x02) >> 1);
                _listAttachDetachStatus.Add(PortID.Port10, (Attach_Detach_Status_Enum)port10);
            }

            return _listAttachDetachStatus;
        }
        public Dictionary<PortID, Attach_Detach_Status_Enum> PhysicalTesterCardStatus()
        {
            _testerCardStatus = new Dictionary<PortID, Attach_Detach_Status_Enum>();
            byte[] dataBuffer = new byte[] { 0x07, 0x01, 0x0B, 0x00, 0x00 };
            bool status = comm_Read_Write.Read(ref dataBuffer, MethodBase.GetCurrentMethod().Name);
            if (status)
            {
                int orVal = 1;
                for (int i = 0; i < 8; i++)
                {
                    var sdStatus = ((dataBuffer[3] & orVal) >> i);
                    orVal *= 2;
                    _testerCardStatus.Add((PortID)(i + 1), (Attach_Detach_Status_Enum)sdStatus);
                }

                var port9 = (dataBuffer[4] & 0x01);
                _testerCardStatus.Add(PortID.Port9, (Attach_Detach_Status_Enum)port9);
                var port10 = ((dataBuffer[4] & 0x02) >> 1);
                _testerCardStatus.Add(PortID.Port10, (Attach_Detach_Status_Enum)port10);
            }

            return _testerCardStatus;
        }
        public int FWFunctionCard(PortID port)
        {
            byte[] dataBuffer = new byte[] { 0x00, 0x00, 0x01, 0x02 };
            int iByte0 = (((int)port) << 4) | _byteGet;
            dataBuffer[0] = (byte)(iByte0);
            dataBuffer[1] = (byte)(dataBuffer.Length - 2);
            bool status = comm_Read_Write.Read(ref dataBuffer, MethodBase.GetCurrentMethod().Name);
            int retValue;
            if (status)
                retValue = (int)APIErrorEnum.NoError;
            else
                retValue = (int)APIErrorEnum.UnknownError;

            return retValue;
        }
        public int FWPD_Controller(PortID port)
        {
            byte[] refBuffer = new byte[] { 0x00, 0x00, 0x01, 0x03 };

            int iByte0 = (((int)port) << 4) | _byteGet;

            refBuffer[0] = (byte)(iByte0);
            refBuffer[1] = (byte)(refBuffer.Length - 2);
            bool status = comm_Read_Write.Read(ref refBuffer, MethodBase.GetCurrentMethod().Name);
            int retValue;
            if (status)
                retValue = (int)APIErrorEnum.NoError;
            else
                retValue = (int)APIErrorEnum.UnknownError;

            return retValue;
        }
        public int GetADCData(PortID portID)
        {
            bool status = false;
            byte[] refBuffer = CommanCommands(portID, CommCommands.ADC, ref status, MethodBase.GetCurrentMethod().Name);
            if (status)
                return ((refBuffer[3] << 8) | refBuffer[2]);
            else
                return -1;
        }
        public HeatSinkValues GetHeatSinkTemprature()
        {
            HeatSinkValues values = new HeatSinkValues();
            bool status = false;
            CommCommands commCommands = CommCommands.HeatSinkTemp;
            if (HelperModule.AppType == ApplicationType.V_TE)
            {
                commCommands = CommCommands.V_TE_HeatSinkTemp;
            }
            byte[] refBuffer = CommanCommands(PortID.NONE, commCommands, ref status, MethodBase.GetCurrentMethod().Name);
            if (status && HelperModule.AppType == ApplicationType.V_TE)
            {
                values.TempratureValue1 = refBuffer[3];
                values.TempratureValue2 = values.TempratureValue1;
                values.TempratureValue3 = values.TempratureValue1;
                values.MaxTemperature = GetMaxTemperatureValue(values.TempratureValue1, values.TempratureValue2, values.TempratureValue3);

            }
            else if (status && refBuffer[3] == 0xAB && HelperModule.AppType == ApplicationType.V_UP)
            {
                values.TempratureValue1 = refBuffer[4];
                values.TempratureValue2 = refBuffer[5];
                values.TempratureValue3 = refBuffer[6];
                values.MaxTemperature = GetMaxTemperatureValue(values.TempratureValue1, values.TempratureValue2, values.TempratureValue3);
                values.TemperatureStatus = (Temperature)refBuffer[7];
            }
            return values;
        }
        public int ModeSecondStageBootLoader(PortID portID)
        {
            bool status = false;
            byte[] refBuffer = CommanCommands(portID, CommCommands.HeatSinkTemp, ref status, MethodBase.GetCurrentMethod().Name);
            if (status)
                return ((refBuffer[3] << 8) | refBuffer[2]);
            else
                return -1;
        }
        public int FunctionCardPresence()
        {
            bool status = false;
            byte[] refBuffer = CommanCommands(PortID.NONE, CommCommands.HeatSinkTemp, ref status, MethodBase.GetCurrentMethod().Name);
            if (status)
                return ((refBuffer[3] << 8) | refBuffer[2]);
            else
                return -1;
        }
        public int FWCCG4(PortID port)
        {
            byte[] refBuffer = new byte[] { 0x00, 0x00, 0x01, 0x04 };

            int iByte0 = (((int)port) << 4) | _byteGet;

            refBuffer[0] = (byte)(iByte0);
            refBuffer[1] = (byte)(refBuffer.Length - 2);
            bool status = comm_Read_Write.Read(ref refBuffer, MethodBase.GetCurrentMethod().Name);
            int retValue;
            if (status)
                retValue = (int)APIErrorEnum.NoError;
            else
                retValue = (int)APIErrorEnum.UnknownError;

            return retValue;
        }
        public int PDTestData(PortID port)
        {
            byte[] refBuffer = new byte[] { 0x00, 0x00, 0x03 };

            int iByte0 = (((int)port) << 4) | _byteGet;

            refBuffer[0] = (byte)(iByte0);
            refBuffer[1] = (byte)(refBuffer.Length - 2);
            bool status = comm_Read_Write.Read(ref refBuffer, MethodBase.GetCurrentMethod().Name);
            int retValue;

            // TODO : Decode Test data 
            // TODO : Display the data 

            if (status)
                retValue = (int)APIErrorEnum.NoError;
            else
                retValue = (int)APIErrorEnum.UnknownError;

            return retValue;
        }
        public int GetActiveCCline(PortID port)
        {
            byte[] refBuffer = new byte[] { 0x00, 0x00, 0x05, (int)DUT_SRC_INFO.Active_CC_line };

            int iByte0 = (((int)port) << 4) | _byteGet;

            refBuffer[0] = (byte)(iByte0);
            refBuffer[1] = (byte)(refBuffer.Length - 2);
            bool status = comm_Read_Write.Read(ref refBuffer, MethodBase.GetCurrentMethod().Name);
            int retValue;
            if (status)
                retValue = (int)APIErrorEnum.NoError;
            else
                retValue = (int)APIErrorEnum.UnknownError;

            return retValue;
        }
        public int FanTurnOnOff(FanControl fanControl = FanControl.On)
        {
            byte[] dataBuffer = new byte[] { 01, 0x0A, 01, 02, 00, 0x6C, 04, 0xAB, (byte)fanControl, 00, 00 };

            bool status = comm_Read_Write.Write(dataBuffer, MethodBase.GetCurrentMethod().Name);
            int retValue;
            if (status)
                retValue = (int)APIErrorEnum.NoError;
            else
                retValue = (int)APIErrorEnum.UnknownError;

            return retValue;

        }
        public int TestercardFanControl(PortID portID, TCFans fanControl = TCFans.Off)
        {
            return GPIOSValidation(portID, GPIOS.TC_FAN_CONTROL, (byte)fanControl);
        }
        public int EloadGPIOControl(PortID portID, Command autoEload)
        {
            return GPIOSValidation(portID, GPIOS.ELOAD_GPIO_CONTROL, (byte)autoEload);
        }
        public bool GetData(PortID port, out int vbusVoltage, out int vbusCurrent)
        {
            // Methoed3
            bool status = false;
            vbusVoltage = 0;
            vbusCurrent = 0;
            byte[] refBuffer = CommanCommands(port, CommCommands.VBUS, ref status, MethodBase.GetCurrentMethod().Name);
            if (status)
            {
                vbusVoltage = (refBuffer[4] << 8) | refBuffer[3];
                vbusCurrent = (refBuffer[6] << 8) | refBuffer[5];
            }

            return status;
        }
        public int GetData(PortID port, Voltage_Current_Data vcData = Voltage_Current_Data.VBUS, VBUS_VCONN_Data data = VBUS_VCONN_Data.Voltage)
        {

            // Methoed2
            bool status = false;
            CommCommands comm = CommCommands.VBUS;
            if (vcData == Voltage_Current_Data.VCONN)
                comm = CommCommands.VCONN;

            byte[] refBuffer = CommanCommands(port, comm, ref status, MethodBase.GetCurrentMethod().Name);
            int retValue = -1;

            if (status)
            {
                if (data == VBUS_VCONN_Data.Voltage)
                {
                    uint utemp = (uint)((refBuffer[4] << 8) | refBuffer[3]);
                    retValue = (int)utemp;
                }
                else if (data == VBUS_VCONN_Data.Current)
                {
                    uint utemp = (uint)((refBuffer[6] << 8) | refBuffer[5]);
                    retValue = (int)utemp;
                }
            }
            else
                retValue = -1;


            return retValue;
        }
        public string GetData(PortID port, Voltage_Current_Data vcData = Voltage_Current_Data.VBUS)
        {
            // Method 1
            bool status = false;
            CommCommands comm = CommCommands.VBUS;
            if (vcData == Voltage_Current_Data.VCONN)
                comm = CommCommands.VCONN;

            byte[] refBuffer = CommanCommands(port, comm, ref status, MethodBase.GetCurrentMethod().Name);
            string retValue = "";
            try
            {
                if (status)
                {
                    uint utemp = (uint)((refBuffer[4] << 8) | refBuffer[3]);
                    // Since data will be received in Milli volts 
                    retValue += "Voltage : " + (utemp / 1000.0).ToString() + " V | ";

                    utemp = (uint)((refBuffer[6] << 8) | refBuffer[5]);
                    // Since data will be received in Milli volts 
                    retValue += "Current : " + (utemp / 1000.0).ToString() + " A";

                }
                else
                    retValue = "";
            }
            catch (Exception ex)
            {
                Debug(DebugType.DEBUG, MethodBase.GetCurrentMethod().Name, ex);
            }

            return retValue;
        }
        public bool GetControlCardSerialNumber(out int controlCardSerialNumber, out int controlCardBoradNumber, out int ppsSerialNumber, out int ppsBoardNumber)
        {
            bool status = false;
            controlCardSerialNumber = 0;
            controlCardBoradNumber = 0;
            ppsSerialNumber = 0;
            ppsBoardNumber = 0;
            var dataBuffer = GetSerialNumber(PortID.NONE, SerialNumber.ControlCard, ref status);
            if (status)
            {
                if (dataBuffer.Length > 6)
                {
                    controlCardSerialNumber = (dataBuffer[5] << 8) | dataBuffer[4];
                    controlCardBoradNumber = dataBuffer[6];
                    ppsSerialNumber = (dataBuffer[8] << 8) | dataBuffer[7];
                    ppsBoardNumber = dataBuffer[9];
                }
            }
            else
                return false;

            return false;
        }
        public bool GetTesterCardSerialNumber(PortID portID, out int testerCardSerialNumber, out int testerCardBoradNumber)
        {
            bool status = false;
            testerCardSerialNumber = 0;
            testerCardBoradNumber = 0;
            var dataBuffer = GetSerialNumber(portID, SerialNumber.TesterCard, ref status);
            if (status)
            {
                if (dataBuffer.Length > 6)
                {
                    testerCardSerialNumber = (dataBuffer[5] << 8) | dataBuffer[4];
                    testerCardBoradNumber = dataBuffer[6];
                }
            }
            else
                return false;

            return true;
        }
        public double LinkSpeed(PortID port)
        {
            byte[] refBuffer = new byte[] { 0x00, 0x00, 0x04, 0x01 };

            int iByte0 = (((int)port) << 4) | _byteGet;

            refBuffer[0] = (byte)(iByte0);
            refBuffer[1] = (byte)(refBuffer.Length - 2);
            bool status = comm_Read_Write.Read(ref refBuffer, MethodBase.GetCurrentMethod().Name);
            double retValue;
            if (status)
            {
                uint utemp = (uint)((refBuffer[4] << 8) | refBuffer[5]);
                retValue = (double)utemp;
            }
            else
                retValue = -1;

            return retValue;
        }
        public string GetFirmwareVersion(PortID port = PortID.NONE, FirmwareName firmwareName = FirmwareName.Controller)
        {

            if (firmwareName == FirmwareName.PPS && HelperModule.AppType == ApplicationType.V_TE)
            {
                GetPPSData getPPSData = Get_PPS_Data();
                return getPPSData.FirmwareVersion;
            }

            byte[] dataBuffer = new byte[] { 0x00, 0x00, 0x00, 0x00 };

            // Byte 0 - Default value to set data app commands 
            dataBuffer[0] = (byte)((((int)port) << 4) | _byteGet);

            // Byte 1 - Payload Length 
            dataBuffer[1] = 0x02;

            // Byte 2 - Firmware Version
            dataBuffer[2] = 0x01;

            // Byte 3 - individual Cards 
            dataBuffer[3] = (byte)firmwareName;

            // Need decode more on this
            bool status = comm_Read_Write.Read(ref dataBuffer, MethodBase.GetCurrentMethod().Name);
            string str = "";
            if (status)
            {
                if (firmwareName == FirmwareName.TesterCard || firmwareName == FirmwareName.Controller)
                {
                    if (dataBuffer.Length > 0)
                    {

                        if (HelperModule.AppType == ApplicationType.V_UP)
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                str += AsciiConversion((char)dataBuffer[i + 5]);
                            }
                        }
                        else if (HelperModule.AppType == ApplicationType.V_TE)
                        {
                            if (firmwareName == FirmwareName.Controller)
                            {
                                for (int i = 0; i < 3; i++)
                                {
                                    str += AsciiConversion((char)dataBuffer[i + 3]) + ".";
                                }
                                str = str.Remove(str.Length - 1);
                            }
                            else if (firmwareName == FirmwareName.TesterCard)
                            {
                                for (int i = 0; i < 5; i++)
                                {
                                    str += AsciiConversion((char)dataBuffer[i + 5]);
                                }
                            }
                        }
                    }
                }
                else if (firmwareName == FirmwareName.PD_Controller)
                {
                    for (int i = 1; i < 6; i++)
                        str += Convert.ToChar(dataBuffer[i + 5]).ToString();
                }
                else if (firmwareName == FirmwareName.Eload)
                {
                    for (int i = 0; i < 3; i++)
                        str += Convert.ToChar(dataBuffer[i + 5]).ToString();
                }
                else if (firmwareName == FirmwareName.Connectivity_Manager && HelperModule.AppType == ApplicationType.V_TE)
                {
                    str = $"{dataBuffer[6]}.{dataBuffer[7]}";
                }
            }

            return str;
        }
        public string GetControlCardSerialNumber()
        {
            bool status = true;
            string retValue = "";
            var dataBuffer = GetControlCardSerialNumber(out int controlCardSerialNumber, out int ccBoardNumber, out int ppsSerialNumber, out int ppsBoardNumber);
            if (status)
            {
                retValue += "Control Card SlN. - " + controlCardSerialNumber;
                retValue += "\nControl Card BaseBoard Revision. - " + ccBoardNumber;
                retValue += "\nPPS  SlN. - " + ppsSerialNumber;
                retValue += "\nPPS  BoardNumber. - " + ppsBoardNumber;
            }
            else
                retValue = "0.0";

            return retValue;
        }
        public string GetTesterCardSerialNumber(PortID portID)
        {
            bool status = false;
            string retValue = "";
            var dataBuffer = GetSerialNumber(portID, SerialNumber.TesterCard, ref status);
            if (status)
            {
                if (dataBuffer.Length > 6)
                {
                    int testerCardSerialNumber = (dataBuffer[5] << 8) | dataBuffer[4];
                    int testerCardBoradNumber = dataBuffer[6];
                    retValue = "Tester card Sln. - " + testerCardSerialNumber + "\nTester Card BaseBoard Revision. - " + testerCardBoradNumber;
                }
            }
            else
                retValue = "0.0";

            return retValue;
        }
        public string FWSystemInfo(PortID port)
        {
            string retValue = "unknown";

            byte[] refBuffer = new byte[] { 0x00, 0x00, 0x02 };

            int iByte0 = (((int)port) << 4) | _byteGet;

            refBuffer[0] = (byte)(iByte0);
            refBuffer[1] = (byte)(refBuffer.Length - 2);
            bool status = comm_Read_Write.Read(ref refBuffer, MethodBase.GetCurrentMethod().Name);
            if (status)
            {
                for (int i = 0; i < refBuffer.Length; i++)
                    retValue += Convert.ToChar(refBuffer[i]);
            }
            else
                retValue = "unknown";

            return retValue;
        }
        public string GetDUTSinkCap(PortID port)
        {
            string retValue = "unknown";

            byte[] refBuffer = new byte[] { 0x00, 0x00, 0x05, (byte)DUT_SRC_INFO.SINK_CAPS };

            int iByte0 = (((int)port) << 4) | _byteGet;

            refBuffer[0] = (byte)(iByte0);
            refBuffer[1] = (byte)(refBuffer.Length - 2);
            bool status = comm_Read_Write.Read(ref refBuffer, MethodBase.GetCurrentMethod().Name);
            if (status)
            {
                for (int i = 0; i < refBuffer.Length; i++)
                    retValue += Convert.ToChar(refBuffer[i]);
            }
            else
                retValue = "unknown";

            return retValue;
        }
        public VDM_Information GetVdmInfo(PortID port)
        {
            VDM_Information vDM_Information = new VDM_Information();
            try
            {
                byte[] refBuffer = new byte[] { 0x00, 0x00, 0x05, (int)DUT_SRC_INFO.VDM_UVDM_Data };

                //0x17 0x18 0x05 0xF6 0xFA 0xD6 0x14 0x04 0x41 0x80 0x00 0xFF 0xB4 0x04 0x00 0x00 0x00 0x00 0x00 0x00 0x00
                int iByte0 = (((int)port) << 4) | _byteGet;

                refBuffer[0] = (byte)(iByte0);
                refBuffer[1] = (byte)(refBuffer.Length - 2);
                bool status = comm_Read_Write.Read(ref refBuffer, MethodBase.GetCurrentMethod().Name);

                if (status)
                {
                    vDM_Information.NumberOfDataObjects = refBuffer[7];
                    if (refBuffer[5] == 0xD6 && vDM_Information.NumberOfDataObjects > 1)
                    {

                        byte[] tempBuffer = new byte[4]
                        {
                        refBuffer[8],
                        refBuffer[9],
                        refBuffer[10],
                        refBuffer[11],
                        };

                        vDM_Information.VDM_Data_Object_1 = VDM_Data_Object_1.DecodeByteValue(tempBuffer);

                        tempBuffer = new byte[4]
                        {
                        refBuffer[12],
                        refBuffer[13],
                        refBuffer[14],
                        refBuffer[15],
                        };

                        vDM_Information.VDM_Data_Object_2 = VDM_Data_Object_2.DecodeByteValue(tempBuffer);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug(DebugType.DEBUG, MethodBase.GetCurrentMethod().Name, ex);
            }
            return vDM_Information;
        }
        public Unstructured_VDM_Information GetUVdmInfo(PortID port)
        {
            Unstructured_VDM_Information vDM_Information = new Unstructured_VDM_Information();
            try
            {
                byte[] refBuffer = new byte[] { 0x00, 0x00, 0x05, (int)DUT_SRC_INFO.VDM_UVDM_Data };

                int iByte0 = (((int)port) << 4) | _byteGet;
                refBuffer[0] = (byte)(iByte0);
                refBuffer[1] = (byte)(refBuffer.Length - 2);
                bool status = comm_Read_Write.Read(ref refBuffer, MethodBase.GetCurrentMethod().Name);

                List<byte> value = new List<byte>();
                value.AddRange(refBuffer);
                if (status)
                {
                    vDM_Information.NumberOfDataObjects = refBuffer[7];
                    if (refBuffer[5] == 0xD6 && vDM_Information.NumberOfDataObjects > 1)
                    {
                        vDM_Information.Unstrt_VDM_DataObject_1 = Unstructure_VDM_Data_Object_1.DecodeByteValue(value.GetRange(8, 4).ToArray());
                        vDM_Information.Unstrt_VDM_DataObject_2 = Unstructure_VDM_Data_Object_2.DecodeByteValue(value.GetRange(12, 4).ToArray());
                        List<uint> tempValue = new List<uint>();
                        int payloadIndex = 8;
                        int payloadSize = 4;
                        for (int i = 0; i < 7; i++)
                            tempValue.Add((uint)HelperModule.GetIntFromByteArray(value.GetRange(payloadIndex + (payloadSize * i), 4).ToArray()));
                        vDM_Information.AddDataObject(tempValue);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug(DebugType.DEBUG, MethodBase.GetCurrentMethod().Name, ex);
            }
            return vDM_Information;
        }
        public PDC_Status DecoderPdcStatus(PortID portID)
        {
            PDC_Status objPDC = new PDC_Status();
            byte[] refBuffer = new byte[] { 0x17, 0x00, 0x05, (int)DUT_SRC_INFO.PDC_INFO };
            refBuffer[0] = (byte)((((int)portID) << 4) | _byteGet);
            refBuffer[1] = (byte)(refBuffer.Length - 2);

            bool status = comm_Read_Write.Read(ref refBuffer, MethodBase.GetCurrentMethod().Name);
            if (status)
            {
                List<byte> PdcStatusbytes = refBuffer.ToList().GetRange(4, refBuffer.Length - 4);
                if (PdcStatusbytes[0] == CONFM_CCG3P_KEYWORD && PdcStatusbytes[1] == 0xD4)
                {
                    objPDC.Pdcstatus = PDContractNegotation.Failed;
                    if (PdcStatusbytes[3] > 0)
                        objPDC.Pdcstatus = PDContractNegotation.Successful;
                    objPDC.PdoIndex = (PDOIndex)PdcStatusbytes[4];

                    uint PowerCurrent = ((uint)PdcStatusbytes[6] << 8) | PdcStatusbytes[5];
                    uint MaxVoltage = ((uint)PdcStatusbytes[8] << 8) | PdcStatusbytes[7];
                    uint MinVoltage = ((uint)PdcStatusbytes[10] << 8) | PdcStatusbytes[9];
                    uint VbusVoltage = ((uint)PdcStatusbytes[12] << 8) | PdcStatusbytes[11];
                    if (objPDC.Pdcstatus != 0)
                    {
                        objPDC.RequestedCurrentOrPower = PowerCurrent * 10 / 1000.0;
                        objPDC.RequestedMaxVoltage = MaxVoltage / 1000.0;
                        objPDC.RequestedMinVoltage = MinVoltage / 1000.0;
                        objPDC.VbusVoltage = VbusVoltage / 1000.0;
                        objPDC.ActiveCcLine = (CCline)PdcStatusbytes[13];
                    }
                }
            }

            return objPDC;
        }
        public LoopBackInfo GetLoopBackInfo(PortID port)
        {
            LoopBackInfo retVal = new LoopBackInfo();
            try
            {
                byte[] refBuffer = new byte[] { 0x00, 0x00, 0x04, 0x01 };

                int iByte0 = (((int)port) << 4) | _byteGet;

                refBuffer[0] = (byte)(iByte0);
                refBuffer[1] = (byte)(refBuffer.Length - 2);
                bool status = comm_Read_Write.Read(ref refBuffer, MethodBase.GetCurrentMethod().Name);
                if (status)
                {
                    uint linkspeed = (uint)(refBuffer[5]);
                    uint loopbackstatus = (uint)(refBuffer[6]);

                    if (refBuffer[4] == 0xD5)
                    {
                        retVal = new LoopBackInfo((Loopbackstatus)loopbackstatus, (LinkSpeed)linkspeed);
                        return retVal;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug(DebugType.DEBUG, MethodBase.GetCurrentMethod().Name, ex);

            }
            return retVal;
        }
        public PhysicalLinkError GetPhysicalLinkErrorCount(PortID port)
        {
            PhysicalLinkError error = new PhysicalLinkError();
            //Decoding Physical & Link error response
            byte[] dataBuffer = new byte[] { 0x17, 0x01, 0x40 };
            int iByte0 = (((int)port) << 4) | _byteGet;
            dataBuffer[0] = (byte)(iByte0);
            bool status = comm_Read_Write.Read(ref dataBuffer, MethodBase.GetCurrentMethod().Name);
            if (status)
            {
                error.PhysicalError = ((dataBuffer[4] << 8) | dataBuffer[3]);
                error.LinkError = ((dataBuffer[6] << 8) | dataBuffer[5]);
                error.IterationCount = ((dataBuffer[8] << 8) | dataBuffer[7]);
                error.Total_PhysicalError = ((dataBuffer[10] << 8) | dataBuffer[9]);
                error.Total_LinkError = ((dataBuffer[12] << 8) | dataBuffer[11]);
                error.Present_USB_2p0 = dataBuffer[13];
                error.Total_USB_2p0 = ((dataBuffer[15] << 8) | dataBuffer[14]);
            }
            return error;
        }
        public SourceCapabilities DecoderSourceCaps(PortID port)
        {
            //Getting DUT source caps before setting the PD configuration 
            SourceCapabilities srcCaps = new SourceCapabilities();
            try
            {
                byte[] refBuffer = new byte[] { 0x00, 0x02, 0x05, (byte)DUT_SRC_INFO.SRC_CAPS };
                refBuffer[0] = (byte)((((int)port) << 4) | _byteGet);
                bool status = comm_Read_Write.Read(ref refBuffer, MethodBase.GetCurrentMethod().Name);
                if (status)
                {
                    List<byte> SrcCapbytes = refBuffer.ToList().GetRange(4, (refBuffer.Length - 4));
                    if (SrcCapbytes[0] == CONFM_CCG3P_KEYWORD && SrcCapbytes[1] == 0xD1)
                    {
                        int ilength = SrcCapbytes[2];
                        int ipdoCount = SrcCapbytes[3];
                        if (ilength >= (((ipdoCount * 7) + 1)))
                        {
                            srcCaps = DecodePDOs(ipdoCount, SrcCapbytes);
                            srcCaps.Port = port;
                            return srcCaps;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug(DebugType.DEBUG, MethodBase.GetCurrentMethod().Name, ex);
            }
            return srcCaps;
        }
        public LEDSystemStatus TesterCardSystemStatus()
        {
            try
            {
                if (!isPollingRunning)
                {
                    isPollingRunning = true;
                    GetTesterCardSystemStatus();
                    isPollingRunning = false;
                }
            }
            catch (Exception ex)
            {
                isPollingRunning = false;
                LEDSystemStatus lEDSystemStatus = new LEDSystemStatus
                {
                    Error = "WARNING: Unknown error Stop polling"
                };
                _ledSystemStatus = lEDSystemStatus;

                Debug(DebugType.DEBUG, MethodBase.GetCurrentMethod().Name, ex);
            }
            return _ledSystemStatus;
        }
        public SwingDeEmphasis USBSwingDeEmphasisRegisterRead(PortID port, TxConfigType txConfigType)
        {
            SwingDeEmphasis swingDeEmphsis = new SwingDeEmphasis();
            try
            {
                byte[] refBuffer = new byte[] { 0x17, 0x02, 0x0F, 0x00 };
                int iByte0 = (((int)port) << 4) | _byteGet;
                refBuffer[0] = (byte)(iByte0);
                refBuffer[1] = (byte)(refBuffer.Length - 2);

                //USB 3.0 PHY Transmitter Config Register
                refBuffer[3] = (byte)txConfigType;

                bool retValue = comm_Read_Write.Read(ref refBuffer, MethodBase.GetCurrentMethod().Name);
                if (retValue)
                {
                    byte[] tempBuffer = new byte[4];
                    for (int i = 0; i < tempBuffer.Length; i++)
                        tempBuffer[i] = refBuffer[i + 5];

                    int intValue = HelperModule.GetIntFromByteArray(tempBuffer);
                    if (txConfigType == TxConfigType.USB_3_0_PHY)
                    {
                        swingDeEmphsis.Tx_De_Emphasis_3p5dB_3p0 = intValue & 0x1F;
                        swingDeEmphsis.TX_De_Emphasis_6dB_3p0 = (intValue >> 7) & 0x3F;
                        swingDeEmphsis.TX_De_Emphasis_6dB_3p0 = (intValue >> 14) & 0x7F;
                        swingDeEmphsis.TX_De_Emphasis_6dB_3p0 = (intValue >> 21) & 0x7F;
                    }
                    else if (txConfigType == TxConfigType.USB_3_0_PHY)
                    {
                        swingDeEmphsis.Pre_Emphasis_2p0 = (intValue >> 22) & 0x1;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug(DebugType.DEBUG, MethodBase.GetCurrentMethod().Name, ex);

            }
            return swingDeEmphsis;
        }
        public DataPowerRole Get_Data_Power_Role(PortID port)
        {
            DataPowerRole dataPowerRole = new DataPowerRole();
            try
            {
                byte[] refBuffer = new byte[] { 0x17, 0x02, 0x05, (byte)DUT_SRC_INFO.Current_Port_Role };
                refBuffer[0] = (byte)((((int)port) << 4) | _byteGet);
                refBuffer[1] = (byte)(refBuffer.Length - 2);
                bool retValue = comm_Read_Write.Read(ref refBuffer, MethodBase.GetCurrentMethod().Name);
                if (retValue)
                {
                    // 0xFA - Keyword saying Data is from ccg3pa
                    // 0xD7 - Keyword for GetCurrent Port role
                    if (refBuffer[4] == CONFM_CCG3P_KEYWORD && refBuffer[5] == 0xD7)
                    {
                        dataPowerRole.PDC_Negotation = (PDContractNegotation)refBuffer[7];
                        dataPowerRole.DataRole = (DataRoleType)refBuffer[8];
                        dataPowerRole.PowerRole = (PowerRoleType)refBuffer[9];
                        dataPowerRole.IntenalPowerRole = (IntenalPowerRoleType)refBuffer[10];
                    }
                    dataPowerRole.ReturnValue = retValue;
                }
            }
            catch (Exception ex)
            {
                Debug(DebugType.DEBUG, MethodBase.GetCurrentMethod().Name, ex);
            }
            return dataPowerRole;
        }
        public int Get_SRC_CAPS_Extended(PortID portID)
        {

            int retVal = 0;
            byte PDP_VALUE = 7;

            PDCommandInit(portID, MsgCategory.ExtendedMsg, SOPType.SOP, MsgType.Get_Source_Cap_Extended_Message);
            Sleep(10);
            byte[] refBuffer = new byte[] { 0x00, 0x00, 0x05, (int)DUT_SRC_INFO.SRC_CAPS_EXT };

            int iByte0 = (((int)portID) << 4) | _byteGet;
            refBuffer[0] = (byte)(iByte0);
            refBuffer[1] = (byte)(refBuffer.Length - 2);
            bool status = comm_Read_Write.Read(ref refBuffer, MethodBase.GetCurrentMethod().Name);

            if (refBuffer[4] == CONFM_CCG3P_KEYWORD && status)
                retVal = refBuffer[PDP_VALUE];

            return retVal;
        }
        public bool Get_PDControllerLogData(PortID portID)
        {
            //X1 04 02 03 F2 02
            Sleep(10);
            byte[] refBuffer = new byte[] { 0x00, 0x00, 0x05, (byte)DUT_SRC_INFO.Event_Logging_Information };

            refBuffer[0] = (byte)((((int)portID) << 4) | _byteGet);
            refBuffer[1] = (byte)(refBuffer.Length - 2);

            bool retVal = comm_Read_Write.Read(ref refBuffer, MethodBase.GetCurrentMethod().Name);
            if (retVal)
                HelperModule.Debug(refBuffer, $"{MethodBase.GetCurrentMethod().Name} -  {DateTime.Now}");
            return retVal;
        }
        public string Get_System_Error_Status()
        {
            bool status = false;
            string error = "No Error";
            byte[] refBuffer = CommanCommands(PortID.NONE, CommCommands.VUPState, ref status, MethodBase.GetCurrentMethod().Name);
            if (status)
            {
                byte byteVal = refBuffer[3];
                if (byteVal == 0xF0)
                    error = $"Error Code {byteVal:X} : Temperature exceeded the limit set by user";
                else if (refBuffer[3] == 0x00)
                    error = $"Error Code {byteVal:X} : Temperature is with in range";

                byteVal = refBuffer[4];
                if (byteVal == 0xF1)
                    error += $"\nError Code {byteVal:X}: Temperature sensor not connected/open or sensor is spoiled";
                else if (byteVal == 0x00)
                    error += $"\nError Code {byteVal:X}: Sensor are connected";
            }
            return error;
        }
        public string Get_DUT_Firmware_Version(PortID portID)
        {
            try
            {
                byte[] dataBuffer = new byte[] { 0x00, 0x00, (byte)APIByte2.TestFunctionCardCommands, (byte)APIByte3.PDSystemSettings, (byte)APIByte4.MiscellaneousOperations, 0x00 };
                int iByte0 = (((int)portID) << 4) | _byteSet;

                dataBuffer[0] = (byte)iByte0;
                dataBuffer[1] = (byte)(dataBuffer.Length - 2);
                dataBuffer[5] = (byte)MiscellaneousOperations.DEVICE_VERSION;
                bool retValue = comm_Read_Write.Write(dataBuffer, MethodBase.GetCurrentMethod().Name);
                if (retValue)
                {
                    Unstructured_VDM_Information info = GetUVdmInfo(portID);
                    // ex firmware version format 0x32000650 - 0x3.2.0.00650
                    string value = info.DataObject_RawValue[3].ToString("X");
                    string[] firmware = value.Insert(1, ".").Insert(3, ".").Insert(5, ".").Split('.');

                    string valueDec = "";
                    string valueHex = "";
                    foreach (var charVal in firmware)
                    {
                        valueHex += charVal + ".";
                        valueDec += Convert.ToInt16(charVal, 16).ToString() + ".";
                    }

                    string version = $"\nFirmware Version - \n0x{value}h\nv {valueDec.Remove(valueDec.Length - 1, 1)}\nv {valueHex.Remove(valueHex.Length - 1, 1)}h";
                    return version;
                }
            }
            catch (Exception ex)
            {
                Debug(DebugType.DEBUG, MethodBase.GetCurrentMethod().Name, ex);

            }
            return "";
        }
        public CableData GetCableCapabilities(PortID port)
        {
            CableData cableData = new CableData();
            byte[] refBuffer = new byte[] { 0x00, 0x00, 0x05, (byte)DUT_SRC_INFO.Get_SOP1_Response };
            int iByte0 = (((int)port) << 4) | _byteGet;
            refBuffer[0] = (byte)(iByte0);
            refBuffer[1] = (byte)(refBuffer.Length - 2);
            bool status = comm_Read_Write.Read(ref refBuffer, MethodBase.GetCurrentMethod().Name);
            if (status)
            {
                if (CONFM_CCG3P_KEYWORD == refBuffer[4] && SOP_1_Rx == refBuffer[5])
                {
                    int startIndex = 7;
                    cableData.SOP_Type = (SOPType)((refBuffer[startIndex] & 0xF0) >> 4);
                    cableData.ResposeType = (RESPONSE_TYPE)(refBuffer[startIndex] & 0x03);
                    if ((refBuffer[startIndex] & 0x4) == 0x4)
                    {
                        cableData.IsEnabled = true;
                    }
                    else
                    {
                        HelperModule.Debug("Cable tester is not configured");
                    }

                    if (cableData.IsEnabled)
                    {
                        cableData.NumberDataObject = refBuffer[startIndex + 1];
                        cableData.MessageHeader = (uint)((refBuffer[startIndex + 3] << 8) | refBuffer[startIndex + 2]);
                        if (cableData.NumberDataObject > 1)
                        {
                            for (int i = 0; i < (cableData.NumberDataObject * 4); i += 4)
                            {
                                uint value = (uint)((refBuffer[startIndex + 7 + i] << 24) | (refBuffer[startIndex + 6 + i] << 16)
                                    | (refBuffer[startIndex + 5 + i] << 8) | refBuffer[startIndex + 4 + i]);
                                cableData.DataObject.Add(value);
                            }
                        }
                    }
                }
            }
            return cableData;
        }
        public PDC_Status CableFlip(PortID port)
        {
            Detach(port);
            PDC_Status pDC_Status = new PDC_Status();
            if (CableSelection(port, CableType.Special_Cable) == 0)
            {
                if (cableFlip)
                {
                    if (RaSelection(port, Ra_Selection.RaAssert_CC1) == 0)
                    {
                        Sleep(1000);
                        if (Attach(port) == 0)
                        {
                            Sleep(1000);
                            pDC_Status = DecoderPdcStatus(port);
                        }
                        else
                        {
                            HelperModule.Debug("Attach failed");
                        }

                    }
                    else
                    {
                        HelperModule.Debug("Ra selection failed");
                    }
                }
                else
                {
                    if (RaSelection(port, Ra_Selection.RaAssert_CC2) == 0)
                    {
                        Sleep(1000);

                        if (Attach(port) == 0)
                        {
                            Sleep(1000);
                            pDC_Status = DecoderPdcStatus(port);
                        }
                        else
                        {
                            HelperModule.Debug("Attach failed");
                        }
                    }
                    else
                    {
                        HelperModule.Debug("Ra selection failed");
                    }

                }

                cableFlip = !cableFlip;
            }
            CableSelection(port, CableType.TypeC_Cable);
            return pDC_Status;
        }
        public CalibrationExpiryDetailsList Calibration_Expire_Details()
        {
            CalibrationExpiryDetailsList calibrationExpiryDetailsList = new CalibrationExpiryDetailsList();
            try
            {
                byte[] refBuffer = new byte[] { 0x07, 0x01, 0x82 };
                bool status = comm_Read_Write.Read(ref refBuffer, MethodBase.GetCurrentMethod().Name);

                if (!status)
                {
                    return calibrationExpiryDetailsList;
                }

                int totalByts = refBuffer[1];
                int index = 3;
                PortID portID = PortID.NONE;
                int day = 0;
                int month = 0;
                int year = 0;

                if (!(totalByts > 1))
                {
                    return calibrationExpiryDetailsList;
                }

                CalibrationExpiryDetails keyValuePairs;
                for (int i = 0; i < 55; i += 5)
                {
                    keyValuePairs = new CalibrationExpiryDetails();
                    int portValue = refBuffer[i + index];
                    day = refBuffer[i + index + 1];
                    month = refBuffer[i + index + 2];
                    year = (refBuffer[i + index + 4] << 8) | refBuffer[i + index + 3];
                    if (portValue == 0xCC)
                    {
                        portID = PortID.NONE;
                    }
                    else if (portValue == 0)
                    {
                        continue;
                    }
                    else
                    {
                        portID = (PortID)portValue;
                    }
                    DateTime dateTime = new DateTime(year, month, day);


                    keyValuePairs.ExpiryDate = dateTime;
                    keyValuePairs.Port = portID;

                    TimeSpan isExpried = dateTime.Subtract(keyValuePairs.PresentDate);

                    if (isExpried.TotalDays <= 0)
                    {
                        keyValuePairs.IsExpired = true;
                        calibrationExpiryDetailsList.IsExpiredCardPresent = true;
                    }
                    else
                    {
                        keyValuePairs.IsExpired = false;
                    }

                    if (!calibrationExpiryDetailsList.List.ContainsKey(portID))
                    {
                        calibrationExpiryDetailsList.List.Add(portID, keyValuePairs);
                    }
                }
            }
            catch (Exception ex)
            {
                HelperModule.Debug(MethodBase.GetCurrentMethod().Name, ex);
            }

            return calibrationExpiryDetailsList;
        }

        #region PPS Source API
        public PPS_ADC_Channel_Read Get_PPS_ADC_Data(int ittiteration = 50, int delay = 1)
        {
            PPS_ADC_Channel_Read pPS_ADC_Channel_Read = new PPS_ADC_Channel_Read();
            for (int i = 0; i < ittiteration; i++)
            {
                GetPPSADCData getPPSDataResult = Get_PPS_ADC_Data_Command();
                if (getPPSDataResult.ReturnValue)
                {
                    pPS_ADC_Channel_Read.ADC.AddCount(getPPSDataResult.ADC_Voltage_Data);
                    pPS_ADC_Channel_Read.Current.AddCount(getPPSDataResult.ADC_Current_Data);
                    pPS_ADC_Channel_Read.DAC.AddCount(getPPSDataResult.DAC_Voltage_Data);
                    Sleep(delay);
                }
                else
                {
                    HelperModule.Debug("Channel read incorrect");
                    break;
                }
            }
            return pPS_ADC_Channel_Read;
        }
        public GetPPSData Get_PPS_Data()
        {
            GetPPSData getPPSData = new GetPPSData();
            GetPPSADCData getPPSADCData = Get_PPS_ADC_Data_Command();
            getPPSData.Voltage = getPPSADCData.ADC_Voltage_Data;
            getPPSData.Current = getPPSADCData.ADC_Current_Data;
            getPPSData.FirmwareVersion = getPPSADCData.FirmwareVersion;
            getPPSData.ReturnValue = true;
            return getPPSData;
        }

        #endregion
        #endregion

        #region Programming App commands 

        #region V-UP
        public int PD_Controller_BootProgramModeSelection(PortID portID)
        {
            byte[] dataBuffer = new byte[]
            {
                 // Byte 0 - Default value to set data app commands 
                (byte)((((int)portID) << 4) | _byteProgramming),

                // Byte 1 - PayLoad length
                 0x02,

                 // Byte 2 - PD_Controller boot programming MOde
                 0x03,

                 // Byte 3 - Programming mode selection
                 0x0F,

             };

            bool status = comm_Read_Write.Write(dataBuffer, MethodBase.GetCurrentMethod().Name);
            int retValue;
            if (status)
                retValue = (int)APIErrorEnum.NoError;
            else
                retValue = (int)APIErrorEnum.UnknownError;

            return retValue;
        }
        public int ControlCard_SecondStage_Bootloader_FirmwareUpdate(string fileName, FirmwareName firmwareName = FirmwareName.Controller)
        {
            return -1;
            //try
            //{
            //    DataSentBytes = 0;

            //        var devices = GetUSBDevices();
            //        foreach (var device in devices)
            //        {
            //            var deviceInfo = device as CyFX3Device;
            //            InitilizeController(deviceInfo);
            //            if (deviceInfo.FriendlyName.Contains("BootLoader"))
            //            {

            //                // check for boot-loader first, if it is not running then prompt message to user.
            //                if (!comm_Read_Write.USBLinkComm.USBCommunication.CyDevice.IsBootLoaderRunning())
            //                {
            //                    MessageBox.Show("Please reset your device to upload firmware", "Boot-loader is not running");
            //                    return -1;
            //                }

            //                break;
            //            }
            //        }
            //        string fPath_fromShortcut = Path.Combine(Directory.GetParent(Application.ExecutablePath).FullName, "CyBootProgrammer.img");
            //        string fPath_buildDir = Path.Combine(Directory.GetParent(Application.ExecutablePath).FullName, "..\\..\\CyBootProgrammer.img");
            //        string fPath = null;

            //        if (File.Exists(fPath_fromShortcut))
            //            fPath = fPath_fromShortcut;
            //        else if (File.Exists(fPath_buildDir))
            //            fPath = fPath_buildDir;
            //        else
            //        {
            //            MessageBox.Show("Can't find the file", "CyBootProgrammer.img");
            //            return -1;
            //        }

            //        // Download CyBootProgrammer 
            //        FX3_FWDWNLOAD_ERROR_CODE errorCode = comm_Read_Write.USBLinkComm.USBCommunication.CyDevice.DownloadFw(fPath, FX3_FWDWNLOAD_MEDIA_TYPE.RAM);

            //        DataSentBytes = 50;
            //        Sleep(1000);

            //        // Reconnect to Fx3 controller 
            //        var initialDevices = GetUSBDevices();
            //        foreach (var device in initialDevices)
            //        {
            //            var deviceInfo = device as CyFX3Device;
            //            if (deviceInfo.FriendlyName.Contains("BootProgrammer"))
            //            {
            //                InitilizeController(deviceInfo);
            //                if (firmwareName == FirmwareName.SSBL)
            //                {

            //                    var temp = comm_Read_Write.USBLinkComm.USBCommunication.ControllerType;
            //                    comm_Read_Write.USBLinkComm.USBCommunication.ControllerType = ControllerType.ControlEndPoint;
            //                    var buffer = new byte[] { 0xAF, 0x55, 0xAA, 0xFA };
            //                    comm_Read_Write.USBLinkComm.USBCommunication.CyControlEndPoint.Value = 0x0003;
            //                    comm_Read_Write.USBLinkComm.USBCommunication.CyControlEndPoint.Index = 0xFFFC;
            //                    comm_Read_Write.Write(buffer, "", (byte)ReqCode.FirmwareUpdate);
            //                    comm_Read_Write.USBLinkComm.USBCommunication.ControllerType = temp;
            //                }
            //                break;
            //            }
            //        }

            //        DataSentBytes = 80;

            //        if (comm_Read_Write.USBLinkComm.USBCommunication.CyDevice == null)
            //            Debug(DebugType.DEBUG, "ControlCardFWUpdate : device not connected;");

            //        // upload the Firmware
            //        errorCode = comm_Read_Write.USBLinkComm.USBCommunication.CyDevice.DownloadFw(fileName, FX3_FWDWNLOAD_MEDIA_TYPE.I2CE2PROM);

            //        if (errorCode == FX3_FWDWNLOAD_ERROR_CODE.SUCCESS)
            //            return (int)APIErrorEnum.NoError;
            //        else if (errorCode == FX3_FWDWNLOAD_ERROR_CODE.FAILED || errorCode == FX3_FWDWNLOAD_ERROR_CODE.INVALID_FILE)
            //            return (int)APIErrorEnum.UnknownError;
            //    }
            //    catch (Exception ex)
            //    {
            //        Debug(DebugType.DEBUG, MethodBase.GetCurrentMethod().Name, ex);
            //    }
            //    DataSentBytes = 100;
            //    return (int)APIErrorEnum.UnknownError;
            //}
            //public int FunctionalProgrammingMode(PortID port, ProgrammingMode mode)
            //{
            //    List<byte> dataBuffer = new List<byte>
            //    {
            //         // Byte 0 - Default value to set data app commands 
            //        (byte)((((int)port) << 4) | _byteProgramming),

            //        // Byte 1 - PayLoad length
            //         0x02,

            //         // Byte 2 - Functional Programming
            //         0x01,

            //         // Byte 3 - Programming mode selection
            //         (byte)mode,

            //     };

            //    bool status = comm_Read_Write.Write(dataBuffer.ToArray(), MethodBase.GetCurrentMethod().Name);
            //    int retValue;
            //    if (status)
            //        retValue = (int)APIErrorEnum.NoError;
            //    else
            //        retValue = (int)APIErrorEnum.UnknownError;

            //return retValue;
        }
        public int PD_ControllerProgrammingModeSelection(PortID port, PD_ControllerProgrammingModeSelection CCG4)
        {
            byte[] dataBuffer = new byte[] { 0x00, 0x00, 0x03, 0x00 };

            int iByte0 = (((int)port) << 4) | _byteProgramming;

            dataBuffer[0] = (byte)(iByte0);
            dataBuffer[1] = (byte)(dataBuffer.Length - 2);
            dataBuffer[3] = (byte)CCG4;

            bool status = comm_Read_Write.Write(dataBuffer, MethodBase.GetCurrentMethod().Name);
            int retValue;
            if (status)
                retValue = (int)APIErrorEnum.NoError;
            else
                retValue = (int)APIErrorEnum.UnknownError;

            return retValue;
        }
        public int RackReferenceLEDControl(PortID port, LEDControl switchControl)
        {
            byte[] dataBuffer = new byte[]
            {
                // byte 0 - default byte for set command
                (byte)((((int)port) << 4) | _byteSet),

                // byte 1 - payload length
                0x03,

                // byte 2 - function card command
                0x02,

                // byte 3 - RACK reference LED control
                0x0B,

                (byte)switchControl,

            };

            bool status = comm_Read_Write.Write(dataBuffer, MethodBase.GetCurrentMethod().Name);
            int retValue;
            if (status)
                retValue = (int)APIErrorEnum.NoError;
            else
                retValue = (int)APIErrorEnum.UnknownError;

            return retValue;
        }
        public int SinkConfigure(PortID portID)
        {
            byte[] dataBuffer = new byte[]
        {
                // byte 0 - default byte for set command
                (byte)((((int)portID) << 4) | _byteSet),

                // byte 1 - payload length
                0x03,

                // byte 2 - function card command
                0x02,

                // byte 3 - PD System Settings
                0x03,

                // byte 4 - PD Tester Mode Sink Setting
                0x03,

        };

            bool status = comm_Read_Write.Write(dataBuffer, MethodBase.GetCurrentMethod().Name);
            int retValue;
            if (status)
                retValue = (int)APIErrorEnum.NoError;
            else
                retValue = (int)APIErrorEnum.UnknownError;

            return retValue;
        }
        public bool ControlCard_Firmware_Update(List<byte> data)
        {
            DataSentBytes = 0;
            bool retVal = false;
            //try
            //{
            //    comm_Read_Write.USBLinkComm.USBCommunication.ControllerType = ControllerType.ControlEndPoint;
            //    comm_Read_Write.USBLinkComm.USBCommunication.CyControlEndPoint.Value = (PRIMARY_FW_START >> 16);
            //    comm_Read_Write.USBLinkComm.USBCommunication.CyControlEndPoint.Index = (PRIMARY_FW_START & 0xFFFF);

            //    uint maxLength = 128;
            //    uint offSetAddress = 0;
            //    StopFWUpdate = false;
            //    if (data.Count > maxLength)
            //    {
            //        do
            //        {
            //            if (StopFWUpdate)
            //            {
            //                StopFWUpdate = false;
            //                return false;
            //            }
            //            List<byte> tempByte = new List<byte>();
            //            for (int i = 0; i < maxLength; i++)
            //                tempByte.Add(data[i]);

            //            retVal = comm_Read_Write.Write(tempByte.ToArray(), "Control Card FW Update", 0xFF, true);

            //            ushort diff = (ushort)(0x10000 - comm_Read_Write.USBLinkComm.USBCommunication.CyControlEndPoint.Index);
            //            if (diff == 0x80)
            //            {
            //                comm_Read_Write.USBLinkComm.USBCommunication.CyControlEndPoint.Value += 0x0001;
            //                comm_Read_Write.USBLinkComm.USBCommunication.CyControlEndPoint.Index = 0x0000;
            //            }
            //            else
            //                comm_Read_Write.USBLinkComm.USBCommunication.CyControlEndPoint.Index += 0x080;

            //            offSetAddress += (uint)maxLength;
            //            data.RemoveRange(0, (int)maxLength);
            //            if (retVal)
            //                DataSentBytes += maxLength;
            //            else
            //                break;
            //        } while (data.Count() > maxLength);
            //    }
            //    if (data.Count > 0)
            //        retVal = comm_Read_Write.Write(data.ToArray(), "Control Card FW Update", 0xFF, true);
            //    Sleep(500);
            //    if (retVal)
            //        DataSentBytes += (uint)data.Count();

            //}
            //catch (Exception ex)
            //{

            //    Debug(DebugType.DEBUG, MethodBase.GetCurrentMethod().Name, ex);
            //}
            return retVal;
        }
        public bool Config_ControllCard_FirmwareUpdate(ControlCardFirmwareUpdate controlCardFirmwareUpdate)
        {
            //try
            //{
            //    // Set the controller to second stage boot loader
            //    ControllerProgramingMode(controlCardFirmwareUpdate);

            //    // Restart the controller 
            //    ControllerReset();

            //    //Wait till the Device enumerate
            //    Sleep(10000);

            //    if (controlCardFirmwareUpdate == ControlCardFirmwareUpdate.SECOND_STAGE_BOOT_MODE)
            //        return true;

            //    // Reconnect to Fx3 controller 
            //    var devices = GetUSBDevices();
            //    foreach (var device in devices)
            //    {
            //        var deviceInfo = device as CyFX3Device;
            //        if (deviceInfo.FriendlyName.Contains("BootLoader"))
            //        {
            //            InitilizeController(deviceInfo);

            //            // Check Vendor ID and ProductID
            //            int verdorID = comm_Read_Write.USBLinkComm.USBCommunication.CyDevice.VendorID;
            //            int ProductID = comm_Read_Write.USBLinkComm.USBCommunication.CyDevice.ProductID;

            //            // After restart if the Vendor ID and Product ID are as expected then below configuration are done at the control END-Point
            //            if ((verdorID == 0x04B4 && ProductID == 0x00F3))
            //            {
            //                byte[] tempBuffer = new byte[5];
            //                int bufferLength = tempBuffer.Length;
            //                comm_Read_Write.USBLinkComm.USBCommunication.CyControlEndPoint.Target = CyConst.TGT_DEVICE;
            //                comm_Read_Write.USBLinkComm.USBCommunication.CyControlEndPoint.ReqType = CyConst.REQ_VENDOR;
            //                comm_Read_Write.USBLinkComm.USBCommunication.CyControlEndPoint.Direction = CyConst.DIR_FROM_DEVICE;
            //                comm_Read_Write.USBLinkComm.USBCommunication.CyControlEndPoint.ReqCode = 0xFB;
            //                comm_Read_Write.USBLinkComm.USBCommunication.CyControlEndPoint.Value = 0x0000;
            //                comm_Read_Write.USBLinkComm.USBCommunication.CyControlEndPoint.Index = 0x0000;
            //                bool ret = comm_Read_Write.USBLinkComm.USBCommunication.CyControlEndPoint.XferData(ref tempBuffer, ref bufferLength);
            //                if (tempBuffer[0] == 0x00)
            //                    return true;
            //                else if (tempBuffer[0] == 0xFF)
            //                    return false;
            //                else
            //                {
            //                    /* EEPROM address 0x3FFFF is expected to hold the firmware update byte*/
            //                    /* This byte is expected to be either 0x00(Primary FW) or 0xFF(Secondary FW)*/
            //                    comm_Read_Write.USBLinkComm.USBCommunication.CyControlEndPoint.Abort();
            //                    comm_Read_Write.USBLinkComm.USBCommunication.CyControlEndPoint.Reset();
            //                    return false;
            //                }
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Debug(DebugType.DEBUG, MethodBase.GetCurrentMethod().Name, ex);
            //    return false;
            //}
            return false;
        }
        public bool PD_Controller_Programming(PortID portID, ushort rowNumb, byte arrayID, byte[] rowData)
        {
            uint maxLength = 25;
            bool retVal = false;
            List<byte> data = new List<byte>();
            for (int i = 0; i < rowData.Length; i++)
                data.Add(rowData[i]);
            if (data.Count > maxLength)
            {
                do
                {
                    List<byte> tempByte = new List<byte>();
                    for (int i = 0; i < maxLength; i++)
                        tempByte.Add(data[i]);
                    retVal = PD_Controller_CreateRowBuffer(portID, tempByte.ToArray(), PD_Controller_Fimware.CMD_SEND_DATA);
                    //if (!retVal)
                    //    break;

                    data.RemoveRange(0, (int)maxLength);
                } while (data.Count() > maxLength);
            }
            if (data.Count > 0)
            {
                byte remaningBytes = (byte)data.Count();
                data.Insert(0, (byte)(rowNumb >> 8));
                data.Insert(0, (byte)rowNumb);
                data.Insert(0, (byte)arrayID);
                retVal = PD_Controller_CreateRowBuffer(portID, data.ToArray(), PD_Controller_Fimware.CMD_PROGRAM_ROW, remaningBytes);

            }
            return retVal;
        }
        public bool PD_Controller_FirmwareUpdate(PortID portID, string[] byteFile)
        {
            DataSentBytes = 0;
            StopFWUpdate = false;
            bool retVal = PD_Controller_CreateRowBuffer(portID, null, PD_Controller_Fimware.CMD_ENTER_BOOTLOADER);
            int ingoreFirstLine = 0;
            int count = 0;
            int TotalBytes = byteFile.Count();
            foreach (string line in byteFile)
            {
                if (StopFWUpdate)
                {
                    StopFWUpdate = false;
                    return false;
                }

                if (ingoreFirstLine == 0)
                {
                    ingoreFirstLine++;
                    DataSentBytes++;
                    continue;
                }
                count++;
                var err = CyBtldr_ParseRowData((uint)(line.Length), line.ToArray(), ref arrayID, ref rowNumber, ref size, ref checkSum);
                if (err == 0)
                {
                    retVal = PD_Controller_Programming(portID, rowNumber, arrayID, rowData);
                    var checkSum2 = checkSum + arrayID + rowNumber + (rowNumber >> 8) + size + (size >> 8);
                }

                if (retVal)
                {
                    //HelperModule.AddStatusUpdate($"data  {(int)(DataSentBytes / TotalBytes * 100)} % ");
                    DataSentBytes++;
                }

                if (!retVal)
                    break;
            }

            // Exit boot loader
            PD_Controller_CreateRowBuffer(portID, null, PD_Controller_Fimware.CMD_EXIT_BOOTLOADER);
            return retVal;
        }
        public bool PD_Controller_CreateRowBuffer(PortID portID, byte[] buffer, PD_Controller_Fimware pd_Controller_Fimware, byte leftOverBytes = 0)
        {
            ulong size = (int)PD_Controller_Fimware.BASE_CMD_SIZE;
            byte lsb = 0;
            byte msb = 0;
            byte RESET = 0x00;
            if (buffer != null)
            {
                lsb = (byte)buffer.Length;
                msb = (byte)(buffer.Length >> 8);
            }
            if (pd_Controller_Fimware == PD_Controller_Fimware.CMD_SEND_DATA)
            {
                size += (ulong)buffer.Length;
            }
            else if (pd_Controller_Fimware == PD_Controller_Fimware.CMD_PROGRAM_ROW)
            {
                size += (ulong)(leftOverBytes + 3);
            }
            else if (pd_Controller_Fimware == PD_Controller_Fimware.CMD_EXIT_BOOTLOADER)
            {
                ulong COMMAND_DATA_SIZE = 1;
                size += COMMAND_DATA_SIZE;
                lsb = (byte)COMMAND_DATA_SIZE;
                msb = (byte)(COMMAND_DATA_SIZE >> 8);
            }

            List<byte> dataBuffer = new List<byte>()
            {
            (byte)PD_Controller_Fimware.CMD_START,
            (byte)pd_Controller_Fimware,
            lsb,
            msb,
        };

            // Extra byte reset will be added for exit boot loader
            if (pd_Controller_Fimware == PD_Controller_Fimware.CMD_EXIT_BOOTLOADER)
                dataBuffer.Add(RESET);

            if (buffer != null)
                foreach (var bytes in buffer)
                    dataBuffer.Add(bytes);

            uint value = (uint)CyBtldr_ComputeChecksum(dataBuffer.ToArray(), (uint)size - 3);

            dataBuffer.Add((byte)value);
            dataBuffer.Add((byte)(value >> 8));
            dataBuffer.Add((byte)PD_Controller_Fimware.CMD_STOP);

            #region Internal API Header Append
            //Test Function Card Commands
            dataBuffer.Insert(0, 0x04);

            //Payload Length
            dataBuffer.Insert(0, (byte)dataBuffer.Count);

            // Set command for each port
            dataBuffer.Insert(0, (byte)((((int)portID) << 4) | _byteProgramming));

            #endregion

            bool retValue = comm_Read_Write.Write(dataBuffer.ToArray(), MethodBase.GetCurrentMethod().Name, 0x01, true);
            Sleep(100);
            if (retValue)
                return true;
            else
                return false;
        }
        public bool PD_Controller_EraseRowBuffer(PortID portID, byte arrayID, byte rowNumber, ref byte cmdSize, ref byte resSize)
        {
            resSize = (byte)PD_Controller_Fimware.BASE_CMD_SIZE;
            cmdSize = (byte)PD_Controller_Fimware.BASE_CMD_SIZE + 3;
            ulong size = (int)PD_Controller_Fimware.BASE_CMD_SIZE;
            int HEADER_COUNT = 4;
            int FOOTER_COUNT = 3;
            byte totalength = (byte)(1 + HEADER_COUNT + FOOTER_COUNT);
            var dataBuffer = new List<byte>()
            {
                (byte)((((int)portID) << 4) | _byteProgramming),
                 totalength,
                 0x04,
                (byte)PD_Controller_Fimware.CMD_START,
                (byte)PD_Controller_Fimware.CMD_ERASE_ROW,
                3,
                3 >> 8,
                arrayID,
                rowNumber,
                (byte)(rowNumber >>8),

            };

            var value = CyBtldr_ComputeChecksum(dataBuffer.ToArray(), (uint)(size + 3) - 3);

            dataBuffer.Add((byte)value);
            dataBuffer.Add((byte)(value >> 8));
            dataBuffer.Add((byte)PD_Controller_Fimware.CMD_STOP);

            bool retValue = comm_Read_Write.Write(dataBuffer.ToArray(), MethodBase.GetCurrentMethod().Name);
            Sleep(200);
            if (retValue)
                return true;
            else
                return false;
        }
        public bool CCline_FirmwareUpdate(PortID portID, string[] byteFile)
        {

            HelperModule.AddStatusUpdate("Firmware update in progress...");
            // Set DR-swap 
            PDCommandInit(portID, MsgCategory.CtrlMsg, SOPType.SOP, MsgType.DR_Swap_Message);

            // Validate PDC and Validate port Data role, If DFP initiate DR_Swap else proceed to next step,
            DataPowerRole dataPowerRole = Get_Data_Power_Role(portID);
            Sleep(100);
            if (dataPowerRole.PDC_Negotation == PDContractNegotation.Failed && dataPowerRole.DataRole == DataRoleType.UFP)
            {
                if (Initiate_CCLine_DR_Swap_Once > 5)
                {
                    if (dataPowerRole.PDC_Negotation == PDContractNegotation.Failed)
                        Debug(DebugType.DEBUG, "PD Contract failed");
                    if (dataPowerRole.DataRole == DataRoleType.DFP)
                        Debug(DebugType.DEBUG, "DataRole " + (byte)DataRoleType.DFP);

                    return false;
                }
                Initiate_CCLine_DR_Swap_Once += 1;
                return CCline_FirmwareUpdate(portID, byteFile);
            }

            // Initiate structured VDM :
            CCLine_Create_API_RowBuffer(portID, VDM_Data_Object_1.GetByteValue(0xFF00, 1, 0, 0, 0, 0, 0, 0x01));

            // Read the vendor ID from DUTs response for further use
            VDM_Information vDM_Information = GetVdmInfo(portID);
            int intVendorID = vDM_Information.VDM_Data_Object_2.USBVendorID;

            // For debug reference can be removed before the release 
            Debug(DebugType.DEBUG, "VendorID - " + intVendorID);

            // Verify the device mode - if boot-mode or flash mode            
            CCLine_Create_API_RowBuffer(portID, Unstructure_VDM_Data_Object_1.GetByteValues(0x0001, intVendorID, 0));

            // Get the response from the DUT to verify which mode it is in, boot-mode or Flash-mode 
            Unstructured_VDM_Information uVDM_Information = GetUVdmInfo(portID);

            List<byte> tempByteDataObject = new List<byte>();
            if (!(uVDM_Information.NumberOfDataObjects > 2))
            {
                if (Initiate_CCline_Boot_Once > 5)
                {
                    Debug(DebugType.DEBUG, "DUT Remained in Flash Mode, Exiting the sequence");
                    return false;
                }

                // Verify the device mode - if boot-mode or flash mode
                foreach (byte value in Unstructure_VDM_Data_Object_1.GetByteValues(0x0005, intVendorID, 0))
                    tempByteDataObject.Add(value);

                foreach (byte value in Unstructure_VDM_Data_Object_2.GetByteValues(0x0000004A))
                    tempByteDataObject.Add(value);
                Initiate_CCline_Boot_Once += 1;
                CCLine_Create_API_RowBuffer(portID, tempByteDataObject.ToArray());

                Sleep(1000);

                return CCline_FirmwareUpdate(portID, byteFile);
            }

            tempByteDataObject.Clear();
            foreach (byte value in Unstructure_VDM_Data_Object_1.GetByteValues(0x0006, intVendorID, 0))
                tempByteDataObject.Add(value);

            foreach (byte value in Unstructure_VDM_Data_Object_2.GetByteValues(0x00000050))
                tempByteDataObject.Add(value);

            CCLine_Create_API_RowBuffer(portID, tempByteDataObject.ToArray());
            GetVdmInfo(portID);

            DataSentBytes = 0;
            StopFWUpdate = false;
            bool retVal = false;
            int ingoreFirstLine = 0;
            int count = 0;
            foreach (var line in byteFile)
            {
                if (StopFWUpdate)
                {
                    StopFWUpdate = false;
                    return false;
                }

                if (ingoreFirstLine == 0)
                {
                    ingoreFirstLine++;
                    DataSentBytes++;
                    continue;
                }

                count++;
                var err = CyBtldr_ParseRowData((uint)(line.Length), line.ToArray(), ref arrayID, ref rowNumber, ref size, ref checkSum);
                if (err == 0)
                {
                    retVal = CCLine_Programming(portID, rowData, intVendorID);
                    var checkSum2 = (checkSum + arrayID + rowNumber + (rowNumber >> 8) + size + (size >> 8));
                }

                tempByteDataObject.Clear();
                foreach (var value in Unstructure_VDM_Data_Object_1.GetByteValues(0x0008, intVendorID, 0))
                    tempByteDataObject.Add(value);

                foreach (var value in Unstructure_VDM_Data_Object_2.GetByteValues((rowNumber << 8) | 0x46))
                    tempByteDataObject.Add(value);

                CCLine_Create_API_RowBuffer(portID, tempByteDataObject.ToArray());

                if (retVal)
                    DataSentBytes++;

                if (!retVal)
                {

                    return false;
                }
            }

            tempByteDataObject.Clear();
            foreach (byte value in Unstructure_VDM_Data_Object_1.GetByteValues(0x0004, intVendorID, 0))
                tempByteDataObject.Add(value);

            foreach (byte value in Unstructure_VDM_Data_Object_2.GetByteValues(0x00000052))
                tempByteDataObject.Add(value);

            retVal = CCLine_Create_API_RowBuffer(portID, tempByteDataObject.ToArray());

            return retVal;
        }
        public bool TesterCardProgramming(PortID portID, List<byte> data, uint offset)
        {
            DataSentBytes = 0;
            uint maxLength = 128;
            uint offSetAddress = offset;
            bool retVal = false;
            StopFWUpdate = false;
            int TotalBytes = data.Count();
            HelperModule.AddStatusUpdate("Firmware update in progress...");
            if (data.Count > maxLength)
            {
                do
                {
                    if (StopFWUpdate)
                    {
                        StopFWUpdate = false;
                        return false;
                    }
                    List<byte> tempByte = new List<byte>();
                    for (int i = 0; i < maxLength; i++)
                        tempByte.Add(data[i]);

                    retVal = WriteFunctionalProgramming(portID, tempByte, offSetAddress);
                    offSetAddress += (uint)maxLength;

                    data.RemoveRange(0, (int)maxLength);


                    if (retVal)
                    {
                        DataSentBytes += maxLength;
                    }
                    else
                    {
                        return false;
                    }
                } while (data.Count() > maxLength);
            }
            if (data.Count > 0)
                retVal = WriteFunctionalProgramming(portID, data, offSetAddress);

            if (retVal)
                DataSentBytes += (uint)data.Count();


            return retVal;
        }
        public bool ChangeCommunication(ApplicationType communication = ApplicationType.V_UP)
        {
            try
            {
                //Config USB/ UART FRAM Data
                byte[] dataBuffer = new byte[] { 0x01, 0x00, 0x01, 0x06, (byte)communication };
                dataBuffer[1] = (byte)(dataBuffer.Length - 2);
                return comm_Read_Write.Write(dataBuffer, MethodBase.GetCurrentMethod().Name);

            }
            catch (Exception ex)
            {
                Debug(DebugType.DEBUG, MethodBase.GetCurrentMethod().Name, ex);
            }
            return false;
        }

        #endregion

        #region V-DPWR 

        #region TI Firmware Update 
        public bool TI_Firmware_Mode_Selection(TI_Firmware_Config tI_Firmware_Config, TI_Program_Config tI_Program_Config, Communication_Phy communication_Phy = Communication_Phy.Eth)
        {
            return TI_Firmware_Programming_Command(tI_Firmware_Config, tI_Program_Config, MethodBase.GetCurrentMethod().Name, null, PortID.NONE, communication_Phy);
        }
        public bool TI_Firmware_Erase(TI_Firmware_Config tI_Firmware_Config, Communication_Phy communication_Phy = Communication_Phy.Eth)
        {
            return TI_Firmware_Programming_Command(tI_Firmware_Config, TI_Program_Config.Erase, MethodBase.GetCurrentMethod().Name, null, PortID.NONE, communication_Phy);
        }
        public bool TI_Firmware_Write(TI_Firmware_Config tI_Firmware_Config, List<byte> data, int delay)
        {
            DataSentBytes = 0;
            uint maxLength = 256;
            bool retVal = false;
            StopFWUpdate = false;

            try
            {
                HelperModule.AddStatusUpdate("Firmware update in progress...");
                int TotalBytes = data.Count();
                if (data.Count > maxLength)
                {
                    do
                    {
                        if (StopFWUpdate)
                        {
                            StopFWUpdate = false;
                            retVal = false;
                            break;
                        }

                        List<byte> tempByte = new List<byte>();
                        for (int i = 0; i < maxLength; i++)
                            tempByte.Add(data[i]);

                        retVal = TI_Firmware_Programming_Command(tI_Firmware_Config, TI_Program_Config.Write, MethodBase.GetCurrentMethod().Name, tempByte);




                        Sleep(delay);
                        data.RemoveRange(0, (int)maxLength);
                        if (retVal)
                        {
                            DataSentBytes += maxLength;
                            //HelperModule.AddStatusUpdate($"data  {(int)(DataSentBytes / TotalBytes * 100)} % ");
                        }
                        else
                        {
                            break;
                        }

                    } while (data.Count() > maxLength);
                }

                if (data.Count > 0 && retVal)
                {
                    retVal = TI_Firmware_Programming_Command(tI_Firmware_Config, TI_Program_Config.Write, MethodBase.GetCurrentMethod().Name, data);
                    if (retVal)
                        DataSentBytes += (uint)data.Count();
                    //HelperModule.AddStatusUpdate($"data  {(int)(DataSentBytes / TotalBytes * 100)} % ");
                }

            }
            catch (Exception ex)
            {
                HelperModule.Debug(MethodBase.GetCurrentMethod().Name, ex);
            }


            return retVal;

        }
        public bool TI_Firmware_Write_CM_Core_Reset()
        {
            return TI_Firmware_Programming_Command(TI_Firmware_Config.ConnectivityManager_CMCore, TI_Program_Config.CM_Reset, "CM Core Resert", null, PortID.NONE, Communication_Phy.UART);
        }
        public bool TI_Firmware_Write_CM_Core(List<byte> data, TI_Firmware_Config tI_Firmware_Config, int delay)
        {
            DataSentBytes = 0;
            uint maxLength = 1024;
            bool retVal = false;
            StopFWUpdate = false;

            try
            {
                HelperModule.AddStatusUpdate("Firmware update in progress...");
                int TotalBytes = data.Count();
                if (data.Count > maxLength)
                {

                    do
                    {
                        if (StopFWUpdate)
                        {
                            StopFWUpdate = false;
                            retVal = false;
                            break;
                        }

                        List<byte> tempByte = new List<byte>();
                        for (int i = 0; i < maxLength; i++)
                            tempByte.Add(data[i]);
                        TI_Firmware_Write_CM_Core(tempByte);
                        Sleep(delay);

                        retVal = TI_Firmware_Programming_Command(tI_Firmware_Config, TI_Program_Config.Write, "CM Core Write flash", null, PortID.NONE, Communication_Phy.UART);


                        data.RemoveRange(0, (int)maxLength);
                        if (retVal)
                        {
                            DataSentBytes += maxLength;
                            //HelperModule.AddStatusUpdate($"data  {(int)(DataSentBytes / TotalBytes * 100)} % ");
                        }
                        else
                        {
                            break;
                        }
                    } while (data.Count() > maxLength);
                }

                if (data.Count > 0 && retVal)
                {

                    TI_Firmware_Write_CM_Core(data);
                    retVal = TI_Firmware_Programming_Command(tI_Firmware_Config, TI_Program_Config.Write, "CM Core Last chunkWrite flash", null, PortID.NONE, Communication_Phy.UART);


                    Sleep(delay);
                    if (retVal)
                        DataSentBytes += (uint)data.Count();

                    //HelperModule.AddStatusUpdate($"data  {(int)(DataSentBytes / TotalBytes * 100)} % ");
                }

            }
            catch (Exception ex)
            {
                HelperModule.Debug(MethodBase.GetCurrentMethod().Name, ex);
            }


            return retVal;

        }



        #endregion

        #endregion

        #endregion

        #endregion

        #region Eload App Commands 


        public int SetCurrent(double vbusCurrent, double vconnCurrent, PortID port, VbusEload vbusEload, TypeCVbusEload typeCVbusEload,
            VconnEload vconnEload, CCline cCline, VBUSModeConfig vBUSModeConfig, VCONNModeConfig vCONNModeConfig,
            EloadChannels eloadChannels = EloadChannels.None)
        {
            //ToDO: Add limits to CC mode and CR mode 
            vbusCurrent = Math.Abs(vbusCurrent);
            vconnCurrent = Math.Abs(vconnCurrent);

            vbusCurrent *= 1000;
            vconnCurrent *= 1000;

            if (vbusEload == VbusEload.On && !VBUS_LimitCheck(vbusCurrent, vBUSModeConfig))
                return -1;

            if (vconnEload == VconnEload.On && !VCONN_LimitCheck(vconnCurrent, vCONNModeConfig))
                return -1;

            if (cCline == CCline.CC1 && vconnEload == VconnEload.On)
            {
                VCONNLoadSwitch(port, VCONN_Load_Switch.VCONN_Load_CC1);
            }
            else if (cCline == CCline.CC2 && vconnEload == VconnEload.On)
            {
                VCONNLoadSwitch(port, VCONN_Load_Switch.VCONN_Load_CC2);
            }


            //else if(cCline == CCline.ActiveCCline && vconnEload == VconnEload.On)
            //{
            //    VCONN_Load_Switch(port, VseriesControllerLibrary_V1.VCONN_Load_Switch.VCONN_Load_CC2);
            //}

            List<byte> dataBuffer = new List<byte>
            {
                 //Byte 0 - Default value to set data app commands 
                (byte)((((int)port) << 4) | _byteSet),

                //Byte 1 - Length of the app command 
                10,

                //Byte 2 - Test Function Card Commands
                0x02,

                //Byte 3 - Load settings
                0x02,

                //Byte 4 : 5 - VBUS load value LSB and then MSB
                (byte)((int)vbusCurrent & 0xFF),
                (byte)(((int)vbusCurrent >> 8) & 0xFF),

                //Byte 6 : 7 VCONN load value LSB and then MSB
                (byte)((int)vconnCurrent & 0xFF),
                (byte)(((int)vconnCurrent >> 8)& 0xFF),

                //Byte 8 - Eload switch for VBUS , VCONN, CCline
                (byte)(((int)cCline << 5) | ((int)vconnEload << 4) | ((int)typeCVbusEload << 1) | (int)vbusEload ),

                //Byte 9 - VBUS , VCONN mode configuration
                (byte)(((int)vCONNModeConfig << 4) | (int)vBUSModeConfig ),
                
                //Byte 10 - Eload channels used only for calibration
                GetChannelByteValue(eloadChannels),

                // Byte 11 - Reserved 
                0
            };



            // Writes to 
            bool status = comm_Read_Write.Write(dataBuffer.ToArray(), MethodBase.GetCurrentMethod().Name);
            int retValue;
            if (status)
                retValue = (int)APIErrorEnum.NoError;
            else
                retValue = (int)APIErrorEnum.UnknownError;
            return retValue;
        }



        public int SetCurrent(double vbusCurrent, double vconnCurrent, PortID port, VbusEload vbusEload, TypeCVbusEload typeCVbusEload,
            VconnEload vconnEload, CCline cCline, VBUSModeConfig vBUSModeConfig, VCONNModeConfig vCONNModeConfig, List<EloadChannels> eloadChannels = null)
        {
            vbusCurrent = Math.Abs(vbusCurrent);
            vconnCurrent = Math.Abs(vconnCurrent);

            vbusCurrent *= 1000;
            vconnCurrent *= 1000;

            if (vbusEload == VbusEload.On && !VBUS_LimitCheck(vbusCurrent, vBUSModeConfig))
                return -1;

            if (vconnEload == VconnEload.On && !VCONN_LimitCheck(vconnCurrent, vCONNModeConfig))
                return -1;

            List<byte> dataBuffer = new List<byte>
            {
                 //Byte 0 - Default value to set data app commands 
                (byte)((((int)port) << 4) | _byteSet),

                //Byte 1 - Length of the app command 
                10,

                //Byte 2 - Test Function Card Commands
                0x02,

                //Byte 3 - Load settings
                0x02,

                //Byte 4 : 5 - VBUS load value LSB and then MSB
                (byte)((int)vbusCurrent & 0xFF),
                (byte)(((int)vbusCurrent >> 8) & 0xFF),

                //Byte 6 : 7 VCONN load value LSB and then MSB
                (byte)((int)vconnCurrent & 0xFF),
                (byte)(((int)vconnCurrent >> 8)& 0xFF),

                //Byte 8 - Eload switch for VBUS , VCONN, CCline
                (byte)(((int)cCline << 5) | ((int)vconnEload << 4) | ((int)typeCVbusEload << 1) | (int)vbusEload ),

                //Byte 9 - VBUS , VCONN mode configuration
                (byte)(((int)vCONNModeConfig << 4) | (int)vBUSModeConfig ),

                //Byte 10 - Eload channels used only for calibration
                AddChannels(eloadChannels),

                // Byte 11 - Reserved 
                0
            };

            // Writes to 
            bool status = comm_Read_Write.Write(dataBuffer.ToArray(), MethodBase.GetCurrentMethod().Name);
            int retValue;
            if (status)
                retValue = (int)APIErrorEnum.NoError;
            else
                retValue = (int)APIErrorEnum.UnknownError;
            return retValue;
        }


        public int SetCurrent(StepSelection vbusVconSteps, StepIncDec stepIncDec, PortID port, VbusEload vbusEload,
            TypeCVbusEload typeCVbusEload, VconnEload vconnEload, CCline cCline, VBUSModeConfig vBUSModeConfig,
            VCONNModeConfig vCONNModeConfig, EloadChannels eloadChannels = EloadChannels.None,
            VbusVconnSelection vbusVconnSelection = VbusVconnSelection.None)
        {

            return CommonCurrentCommand(port, (byte)vbusVconSteps, (byte)stepIncDec, (int)cCline, (int)vbusEload, (int)typeCVbusEload, (int)vconnEload,
                (int)vCONNModeConfig, (int)vBUSModeConfig, GetChannelByteValue(eloadChannels), (byte)vbusVconnSelection);
        }


        public int SetCurrent(StepSelection vbusVconSteps, StepIncDec stepIncDec, PortID port, VbusEload vbusEload,
            TypeCVbusEload typeCVbusEload, VconnEload vconnEload, CCline cCline, VBUSModeConfig vBUSModeConfig,
            VCONNModeConfig vCONNModeConfig, List<EloadChannels> eloadChannels = null,
            VbusVconnSelection vbusVconnSelection = VbusVconnSelection.None)
        {
            return CommonCurrentCommand(port, (byte)vbusVconSteps, (byte)stepIncDec, (int)cCline, (int)vbusEload, (int)typeCVbusEload, (int)vconnEload,
                 (int)vCONNModeConfig, (int)vBUSModeConfig, AddChannels(eloadChannels), (byte)vbusVconnSelection);
        }

        public int EloadProgrammingModeSelection(PortID port, EloadProgramingPort eloadPort, EloadProgramingMode eloadMode)
        {

            byte[] dataBuffer = new byte[]
            {
                // byte 0 - default byte for set command
                (byte)((((int)port) << 4) | _byteProgramming),

                // byte 1 - payload length
                0x03,

                // byte 2 - Eload Programming Mode Selection
                0x05,

                // byte 3 - eload port selection USB or UART 
                (byte)eloadPort,

                // byte 4 - Programing mode selection 
                (byte)eloadMode,

            };

            bool status = comm_Read_Write.Write(dataBuffer, MethodBase.GetCurrentMethod().Name);
            int retValue;
            if (status)
                retValue = (int)APIErrorEnum.NoError;
            else
                retValue = (int)APIErrorEnum.UnknownError;

            return retValue;
        }

        public string FWELoad(PortID port)
        {
            string retValue = "Unknown";

            byte[] refBuffer = new byte[] { 0x00, 0x00, 0x01, 0x05 };

            int iByte0 = (((int)port) << 4) | _byteGet;

            refBuffer[0] = (byte)(iByte0);
            refBuffer[1] = (byte)(refBuffer.Length - 2);
            bool status = comm_Read_Write.Read(ref refBuffer, MethodBase.GetCurrentMethod().Name);
            if (status)
            {
                for (int i = 0; i < refBuffer.Length; i++)
                    retValue += Convert.ToChar(refBuffer[i]);
            }
            else
                retValue = "Unknown";

            return retValue;
        }

        #endregion

        #region Voltage App commands 

        /// <summary>
        /// NOTE : THIS API IS ONLY FOR CALIBRATION DO NOT EXPOSE IT EXTERNALLY
        /// This app command will configure for voltage based on the channel inputs
        /// </summary>
        /// <param name="port">Port 1 - 10</param>
        /// <param name="eloadChannels">ELoad channels 
        /// None = 0,
        /// VBUS_Voltage = 0x01,
        /// VBUS_EXT_Voltage = 0x02,
        /// VBUS_Current = 0x04,
        /// VCONN_Current = 0x08,
        /// VCONN_CC1_Voltage = 0x10,
        /// VCONN_CC2_Voltage = 0x20,
        /// </param>
        /// <param name="vbusEload">VBUS Eload on off</param>
        /// <param name="typeCVbusEload">Type C connector VBUS On Off</param>
        /// <param name="vconnEload">VCONN Eload On Off</param>
        /// <param name="cCline">CC line selection CC1 or CC2</param>
        /// <param name="vBUSModeConfig">VBUS eLoad mode configuration
        /// CCMode = 0,
        /// VbusCRMode = 1,
        /// VbusCPMode = 2,
        /// VbusCVMode = 3,
        /// </param>
        /// <param name="vCONNModeConfig">VCONN eLoad mode configuration
        /// CCMode = 0,
        /// VbusCRMode = 1,
        /// VbusCPMode = 2,
        /// VbusCVMode = 3,
        /// </param>        
        /// <returns></returns>
        public int ConfigADCVoltage(PortID port, EloadChannels eloadChannels = EloadChannels.None, VbusEload vbusEload = VbusEload.Off,
            TypeCVbusEload typeCVbusEload = TypeCVbusEload.Off, VconnEload vconnEload = VconnEload.Off, CCline cCline = CCline.CC1,
            VBUSModeConfig vBUSModeConfig = VBUSModeConfig.CCMode, VCONNModeConfig vCONNModeConfig = VCONNModeConfig.CCMode)
        {
            List<byte> dataBuffer = new List<byte>
            {

                // NOTE: Config voltage app command is same as Set current app command, Difference is some bits are ignore in this. 

                 //Byte 0 - Default value to set data app commands 
                (byte)((((int)port) << 4) | _byteSet),

                //Byte 1 - Length of the app command 
                10,

                //Byte 2 - Test Function Card Commands
                0x02,

                //Byte 3 - Load settings
                0x02,

                //Byte 4 - This bit is ignored for voltage
                0x00,

                //Byte 5 - This bit is ignored for voltage
                0x00,

                //Byte 6 - This bit is ignored for voltage
                0x00,

                //Byte 7 - This bit is ignored for voltage
                0x00,

                //Byte 8 - Eload switch for VBUS , VCONN, CCline
                (byte)(((int)cCline << 5) | ((int)vconnEload << 4) | ((int)typeCVbusEload << 1) | (int)vbusEload ),

                //Byte 9 - VBUS , VCONN mode configuration
                (byte)(((int)vCONNModeConfig << 4) | (int)vBUSModeConfig ),

                //Byte 10 - Eload channels used only for calibration
                GetChannelByteValue(eloadChannels),

                // Byte 11 - This bit is ignored for voltage 
                0x00,
            };

            bool status = comm_Read_Write.Write(dataBuffer.ToArray(), MethodBase.GetCurrentMethod().Name);
            int retValue;
            if (status)
                retValue = (int)APIErrorEnum.NoError;
            else
                retValue = (int)APIErrorEnum.UnknownError;
            return retValue;
        }

        /// <summary>
        /// NOTE : THIS API IS ONLY FOR CALIBRATION DO NOT EXPOSE IT EXTERNALLY
        /// This app command will configure for voltage based on the channel inputs
        /// </summary>
        /// <param name="port">Port 1 - 10</param>
        /// <param name="eloadChannels">List of ELoad channels 
        /// None = 0,
        /// VBUS_Voltage = 0x01,
        /// VBUS_EXT_Voltage = 0x02,
        /// VBUS_Current = 0x04,
        /// VCONN_Current = 0x08,
        /// VCONN_CC1_Voltage = 0x10,
        /// VCONN_CC2_Voltage = 0x20,
        /// </param>
        /// <param name="vbusEload">VBUS Eload on off</param>
        /// <param name="typeCVbusEload">Type C connector VBUS On Off</param>
        /// <param name="vconnEload">VCONN Eload On Off</param>
        /// <param name="cCline">CC line selection CC1 or CC2</param>
        /// <param name="vBUSModeConfig">VBUS eLoad mode configuration
        /// CCMode = 0,
        /// VbusCRMode = 1,
        /// VbusCPMode = 2,
        /// VbusCVMode = 3,
        /// </param>
        /// <param name="vCONNModeConfig">VCONN eLoad mode configuration
        /// CCMode = 0,
        /// VbusCRMode = 1,
        /// VbusCPMode = 2,
        /// VbusCVMode = 3,
        /// </param>        
        /// <returns></returns>
        public int ConfigADCVoltage(PortID port, List<EloadChannels> eloadChannels = null, VbusEload vbusEload = VbusEload.Off,
         TypeCVbusEload typeCVbusEload = TypeCVbusEload.Off, VconnEload vconnEload = VconnEload.Off, CCline cCline = CCline.CC1,
         VBUSModeConfig vBUSModeConfig = VBUSModeConfig.CCMode, VCONNModeConfig vCONNModeConfig = VCONNModeConfig.CCMode)
        {
            List<byte> dataBuffer = new List<byte>
            {

                // NOTE: Config voltage app command is same as Set current app command, Difference is some bits are ignore in this. 

                 //Byte 0 - Default value to set data app commands 
                (byte)((((int)port) << 4) | _byteSet),

                //Byte 1 - Length of the app command 
                10,

                //Byte 2 - Test Function Card Commands
                0x02,

                //Byte 3 - Load settings
                0x02,

                //Byte 4 - This bit is ignored for voltage
                0x00,

                //Byte 5 - This bit is ignored for voltage
                0x00,

                //Byte 6 - This bit is ignored for voltage
                0x00,

                //Byte 7 - This bit is ignored for voltage
                0x00,

                //Byte 8 - Eload switch for VBUS , VCONN, CCline
                (byte)(((int)cCline << 5) | ((int)vconnEload << 4) | ((int)typeCVbusEload << 1) | (int)vbusEload ),

                //Byte 9 - VBUS , VCONN mode configuration
                (byte)(((int)vCONNModeConfig << 4) | (int)vBUSModeConfig ),

                //Byte 10 - Eload channels used only for calibration
                AddChannels(eloadChannels),

                // Byte 11 - This bit is ignored for voltage 
                0x00,
            };

            bool status = comm_Read_Write.Write(dataBuffer.ToArray(), MethodBase.GetCurrentMethod().Name);
            int retValue;
            if (status)
                retValue = (int)APIErrorEnum.NoError;
            else
                retValue = (int)APIErrorEnum.UnknownError;
            return retValue;
        }

        #endregion

        #region Calibration App Commands

        public bool WriteCalibrationControlCard(List<byte> data, uint offset, Card card = Card.Control, PortID portID = PortID.NONE)
        {
            uint maxLength = 247;
            uint offSetAddress = offset;
            bool retVal = false;
            if (data.Count > maxLength)
            {
                int remainingBytes = data.Count;
                do
                {
                    List<byte> tempByte = new List<byte>();
                    for (int i = 0; i < maxLength; i++)
                    {
                        tempByte.Add(data[i]);
                        remainingBytes--;
                    }
                    retVal = WriteCalibdata(tempByte, offSetAddress, card, portID);
                    offSetAddress += maxLength;
                    data.RemoveRange(0, (int)maxLength);
                } while (remainingBytes > maxLength);

            }
            if (data.Count > 0)
                retVal = WriteCalibdata(data, offSetAddress, card, portID);
            Sleep(500);
            return retVal;
        }
        public bool ReadCalibrationControlCard(ref byte[] dataBuffer, uint adress, uint length, Card card = Card.Control, PortID portID = PortID.NONE)
        {
            dataBuffer = new byte[]
                {

            // Byte 0 - Default value to set data app commands 
             (byte)((((int)portID) << 4) | _byteGet),

            //Byte 1 - Length of the app command
            4,

            //Byte 2 - Default value for FRAM read command
            (byte)(0x80 | ((int)card - 1)),

            //Byte 3 - Total length to read from FRAM 
            (byte)length,

            //Byte 5 : 6 - Start offset LSB and MSB respectively
            (byte)(adress & 0xFF),
            (byte)((adress >> 8) & 0xFF),
        };
            bool retVal = comm_Read_Write.Read(ref dataBuffer, MethodBase.GetCurrentMethod().Name);

            Sleep(100);

            return retVal;
        }


        #endregion

        #region Loop Back Commands 

        public bool Read(ref byte[] dataBuffer, ref int byteCount, uint timeOut)
        {
            return comm_Read_Write.Read(ref dataBuffer, ref byteCount, timeOut);
        }
        public bool Write(ref byte[] buffer, ref int byteCount, uint timeOut)
        {
            return comm_Read_Write.Write(ref buffer, ref byteCount, timeOut);
        }

        #endregion


        #endregion

        #region Private Modules

        #region Calibration FRAM Related API Commands
        private bool WriteCalibdata(List<byte> data, uint offset, Card card = Card.Control, PortID portID = PortID.NONE)
        {
            List<byte> refBuffer = new List<byte>
            {
                //Byte 0 -  Default value to set data app commands 
                (byte)((((int)portID) << 4) | _byteSet),

                //Byte 1 - Length of the app command
                (byte)((7 + data.Count) - 2),

                //Byte 2 - Default Control card command
                (byte)card,

                //Byte 3 - Default value for FRAM write data command
                0x0F,

                //Byte 4 - Total number of fram payload bytes 
                (byte)data.Count,

                //Byte 5 : 6 - Start offset LSB and MSB respectively
                (byte)(offset & 0xFF),
                (byte)((offset >> 8) & 0xFF)
            };

            for (int i = 0; i < data.Count; i++)
                refBuffer.Add(data[i]);

            bool retVal = comm_Read_Write.Write(refBuffer.ToArray(), "Calibration Data");
            Sleep(100);
            return retVal;


        }
        #endregion

        #region Supporting Modules         

        private byte AddChannels(List<EloadChannels> eloadChannels)
        {
            byte channel = 0;
            if (eloadChannels.Contains(EloadChannels.None))
                channel = 0;
            if (eloadChannels.Contains(EloadChannels.VBUS_Voltage))
                channel |= 0x01;
            if (eloadChannels.Contains(EloadChannels.VBUS_EXT_Voltage))
                channel |= 0x02;
            if (eloadChannels.Contains(EloadChannels.VBUS_Current))
                channel |= 0x04;
            if (eloadChannels.Contains(EloadChannels.VCONN_Current))
                channel |= 0x08;
            if (eloadChannels.Contains(EloadChannels.VCONN_CC1_Voltage))
                channel |= 0x10;
            if (eloadChannels.Contains(EloadChannels.VCONN_CC2_Voltage))
                channel |= 0x20;

            return channel;
        }
        private byte GetChannelByteValue(EloadChannels eloadChannel)
        {
            if (eloadChannel == EloadChannels.VBUS_Voltage)
                return 0x01;
            else if (eloadChannel == EloadChannels.VBUS_EXT_Voltage)
                return 0x02;
            else if (eloadChannel == EloadChannels.VBUS_Current)
                return 0x04;
            else if (eloadChannel == EloadChannels.VCONN_Current)
                return 0x08;
            else if (eloadChannel == EloadChannels.VCONN_CC1_Voltage)
                return 0x10;
            else if (eloadChannel == EloadChannels.VCONN_CC2_Voltage)
                return 0x20;
            else
                return
                    0x00;
        }

        #endregion

        #region FirmWare Update
        private bool WriteFunctionalProgramming(PortID portID, List<byte> data, uint offset)
        {
            uint index = (offset & 0xFFFF);
            uint value = ((offset >> 16) & 0xFFFF);

            List<byte> refBuffer = new List<byte>
            {
                //Byte 0 -  Default value to set data app commands 
                (byte)((((int)portID) << 4) | _byteProgramming),

                //Byte 1 - Length of the app command
                (byte)(data.Count + 6),

                //Byte 2 -  Functional Card Programming command
                0x02,

                //Byte 3 : 4 - Start offset LSB and MSB respectively
                (byte)(value),
                (byte)(value >> 8),

                //Byte 5 : 6- Start offset LSB and MSB respectively
                (byte)(index),
                (byte)(index >> 8),

                //Byte 7: Total length
                (byte)data.Count,
            };

            for (int i = 0; i < data.Count; i++)
                refBuffer.Add(data[i]);

            bool retVal = comm_Read_Write.Write(refBuffer.ToArray(), "Calibration Data", 0x01, true);
            Sleep(200);
            return retVal;
        }
        private int CyBtldr_ComputeChecksum(byte[] buf, uint size)
        {
            ushort sum = 0;
            ushort i = 0;
            while (size-- > 0)
            {
                sum += buf[i];
                i++;
            }
            return (1 + (~sum));
        }
        private bool CCLine_Programming(PortID portID, byte[] rowData, int intVendorID)
        {


            uint maxLength = 24;
            bool retVal = false;
            int lineNumber = 0x2;
            List<byte> data = new List<byte>();
            for (int i = 0; i < rowData.Length; i++)
                data.Add(rowData[i]);
            int valueByte;
            if (data.Count > maxLength)
            {
                do
                {
                    List<byte> tempByte = new List<byte>();
                    valueByte = (0x00 << 8) | (lineNumber << 4 | 0x7);
                    byte[] temValue = Unstructure_VDM_Data_Object_1.GetByteValues(valueByte, intVendorID, 0);
                    foreach (var value in temValue)
                        tempByte.Add(value);

                    for (int i = 0; i < maxLength; i++)
                        tempByte.Add(data[i]);
                    retVal = CCLine_Create_API_RowBuffer(portID, tempByte.ToArray());

                    data.RemoveRange(0, (int)maxLength);
                    lineNumber += 2;
                } while (data.Count() > maxLength);
            }
            if (data.Count > 0)
            {
                valueByte = (0x00 << 8) | (lineNumber << 4 | 0x7);
                byte[] temValue = Unstructure_VDM_Data_Object_1.GetByteValues(valueByte, intVendorID, 0);

                for (int i = 1; i <= temValue.Length; i++)
                    data.Insert(0, temValue[temValue.Length - i]);
                retVal = CCLine_Create_API_RowBuffer(portID, data.ToArray());

            }
            return retVal;
        }
        private bool CCLine_Create_API_RowBuffer(PortID portID, byte[] buffer)
        {

            List<byte> dataBuffer = new List<byte>();

            if (buffer != null)
                foreach (var bytes in buffer)
                    dataBuffer.Add(bytes);

            //No of Data Objects Can be max up to 7 
            // Since buffer length always be 25 bytes, No of Data Objects will be less than 7

            dataBuffer.Insert(0, (byte)Math.Ceiling(buffer.Length / 4.0));

            //VDM type and SOP assign
            dataBuffer.Insert(0, (((int)VDM_Type.UNSTRUCTURED) << 4) | (int)SOPType.SOP);

            //DATA_MSG_VDM
            dataBuffer.Insert(0, 0x0F);

            //PD Message config
            dataBuffer.Insert(0, 0x05);

            //PD System Settings
            dataBuffer.Insert(0, 0x03);

            //DUT FW update via CCline Commands
            dataBuffer.Insert(0, 0x06);

            //Payload Length
            dataBuffer.Insert(0, (byte)(dataBuffer.Count));

            // Set command for each port
            dataBuffer.Insert(0, (byte)((((int)portID) << 4) | _byteProgramming));

            bool retValue = comm_Read_Write.Write(dataBuffer.ToArray(), MethodBase.GetCurrentMethod().Name, 0x01, true);

            Sleep(70);
            if (retValue)
                return true;
            else
                return false;
        }

        #region V-TE General commands 

        private bool TI_Firmware_Programming_Command(TI_Firmware_Config tI_Firmware_Config, TI_Program_Config tI_Program_Config,
            string apiName, List<byte> buffer = null, PortID portID = PortID.NONE, Communication_Phy communication_Phy = Communication_Phy.Eth)
        {

            List<byte> dataBuffer = new List<byte>()
            {
                (byte)((((int)portID) << 4) | _byteProgramming),
                
                // Ti Firmware update command
                0x06,

                (byte)tI_Firmware_Config,

                (byte)tI_Program_Config,
            };
            if (buffer != null)
            {
                // Note byte length should be less than 0xFF
                dataBuffer.Add((byte)(buffer.Count & 0xFF));
                dataBuffer.Add((byte)((buffer.Count >> 8) & 0xFF));
                dataBuffer.AddRange(buffer);
            }
            dataBuffer.Insert(1, (byte)(dataBuffer.Count));
            return comm_Read_Write.Write(dataBuffer.ToArray(), apiName, 1, false, communication_Phy);
        }

        private bool TI_Firmware_Write_CM_Core(List<byte> data, int delay = 10)
        {

            uint maxLength = 14;
            bool retVal = false;
            StopFWUpdate = false;

            try
            {
                if (data.Count > maxLength)
                {
                    do
                    {
                        if (StopFWUpdate)
                        {
                            StopFWUpdate = false;
                            retVal = false;
                            break;
                        }

                        List<byte> tempByte = new List<byte>();
                        for (int i = 0; i < maxLength; i++)
                            tempByte.Add(data[i]);

                        retVal = TI_Firmware_Programming_Command(tempByte, "CM Core Firmware Update 16bytes chunk");
                        Sleep(delay);
                        data.RemoveRange(0, (int)maxLength);

                    } while (data.Count() > maxLength);
                }

                if (data.Count > 0 && retVal)
                {
                    TI_Firmware_Programming_Command(data, "CM Core Firmware Update 16bytes last chunk");
                    Sleep(delay);
                }
            }
            catch (Exception ex)
            {
                HelperModule.Debug(MethodBase.GetCurrentMethod().Name, ex);
            }


            return retVal;

        }

        private bool TI_Firmware_Programming_Command(List<byte> buffer = null, string apiName = "")
        {
            List<byte> dataBuffer = new List<byte>()
            {
               0xC2,

               // Payload length
               (byte)(buffer.Count),
            };

            if (buffer != null)
            {
                dataBuffer.AddRange(buffer);
            }
            return comm_Read_Write.Write(dataBuffer.ToArray(), apiName, 1, false, Communication_Phy.UART);
        }
        private FirmwareUpdateStatus FirmwareUpdateStatusDetails(TI_Firmware_Config hwSelection)
        {

            FirmwareUpdateStatus firmwareUpdateStatus = new FirmwareUpdateStatus();

            byte[] dataBuffer = new byte[] { _byteProgramming, 0x00, 0x07, (byte)hwSelection, 0x07 };

            bool status = comm_Read_Write.Read(ref dataBuffer, MethodBase.GetCurrentMethod().Name);
            if (status)
            {
                if (dataBuffer[0] == 0xd0)
                {
                    firmwareUpdateStatus.CodeWord = dataBuffer[0];
                    firmwareUpdateStatus.WriteStatus = (byte)(dataBuffer[1] & 0x01);
                    firmwareUpdateStatus.AddressMissmatchError = (byte)((dataBuffer[1] >> 1) & 0x01);
                    firmwareUpdateStatus.FlashWriteVerificationError = (byte)((dataBuffer[1] >> 2) & 0x01);
                    firmwareUpdateStatus.KeyError = (byte)((dataBuffer[1] >> 3) & 0x01);
                    firmwareUpdateStatus.AddressMismatchValue = dataBuffer[2];
                    firmwareUpdateStatus.FlashWriteVerificationErrorValue = dataBuffer[3];
                    firmwareUpdateStatus.KeyErrorHappenedValue = dataBuffer[4];
                }
                else
                {
                    HelperModule.Debug($"Error with FLASH error {dataBuffer[0]}");
                }

            }

            return new FirmwareUpdateStatus();
        }


        #endregion


        #endregion

        #region General
        private byte[] GetSerialNumber(PortID portID, SerialNumber serialNumber, ref bool retVal)
        {
            byte[] dataBuffer = new byte[]
            {
                // 07 02 0E 01 - ControlCard Command reference 
                // 07 02 0E 02 - Testecard  Command reference 
                 
                 //Byte 0 - Default value to get data app commands 
                (byte)((((int)portID) << 4) | _byteGet),

                 //byte 1 -  Length of the payload
                 0x02,

                 //byte 2 - Get serial number and board revision
                 0x0E,

                 (byte)serialNumber,

                 };

            retVal = comm_Read_Write.Read(ref dataBuffer, MethodBase.GetCurrentMethod().Name);
            return dataBuffer;
        }
        private SourceCapabilities DecodePDOs(int pdoCount, List<byte> SrcCapbytes)
        {
            SourceCapabilities srcCaps = new SourceCapabilities();
            for (int i = 0, j = 4; i < pdoCount; i++, j += 7)
            {
                PDO objPDO = new PDO();
                int PDOType = SrcCapbytes[j];
                objPDO.PdoType = (PDOSupplyType)((PDOType >> 4) & 0x0F);
                objPDO.PDO_Index = (((PDOType) & 0x0F));
                uint uiVolt = (uint)((SrcCapbytes[j + 2] << 8) | SrcCapbytes[j + 1]);
                uint uiMinVolt = (uint)((SrcCapbytes[j + 4] << 8) | SrcCapbytes[j + 3]);
                uint uiCurrent = (uint)((SrcCapbytes[j + 6] << 8) | SrcCapbytes[j + 5]);

                if (objPDO.PdoType == PDOSupplyType.FixedSupply)
                {
                    objPDO.Voltage = ((uiVolt * 50) / 1000.0);
                    objPDO.MinVoltage = ((uiMinVolt * 0) / 1000.0);
                    objPDO.Current = ((uiMinVolt * 10) / 1000.0);
                }
                else if (objPDO.PdoType == PDOSupplyType.Battery)
                {
                    objPDO.Voltage = ((uiVolt * 50) / 1000.0);
                    objPDO.MinVoltage = ((uiMinVolt * 50) / 1000.0);
                    objPDO.Current = ((uiCurrent * 0) / 1000.0);
                }
                else if (objPDO.PdoType == PDOSupplyType.VariableSupply)
                {
                    objPDO.Voltage = ((uiVolt * 50) / 1000.0);
                    objPDO.MinVoltage = ((uiMinVolt * 50) / 1000.0);
                    objPDO.Current = ((uiCurrent * 10) / 1000.0);
                }
                else if (objPDO.PdoType == PDOSupplyType.Augmented)
                {
                    objPDO.Voltage = ((uiVolt * 100) / 1000.0);
                    objPDO.MinVoltage = ((uiMinVolt * 100) / 1000.0);
                    objPDO.Current = ((uiCurrent * 50) / 1000.0);
                }
                srcCaps.PDOlist.Add(objPDO);

            }

            return srcCaps;
        }
        private int GPIOSValidation(PortID portID, GPIOS gPIOS, byte val4)
        {

            byte[] dataBuffer = new byte[]
                    {
                //Byte 0 - Default value to get data app commands 
                 (byte)((((int)portID) << 4) | _byteSet),

                 //byte 1 -  Length of the payload
                 0x04,

                 //byte 2 - Test function card commands
                 0x02,

                 //byte 3 - GPIO's Validating
                 0xF3,

                 // byte 4 - GPIO selection bit
                 (byte)gPIOS,

                 // byte 5 - Based on 4th byte selection this will change
                 val4,
                 };

            bool status = comm_Read_Write.Write(dataBuffer, MethodBase.GetCurrentMethod().Name);
            int retValue;
            if (status)
                retValue = (int)APIErrorEnum.NoError;
            else
                retValue = (int)APIErrorEnum.UnknownError;

            return retValue;

        }
        private byte[] CommanCommands(PortID portID, CommCommands byteValue, ref bool retVal, string methodName)
        {
            byte[] refBuffer = new byte[]
          {
                // byte 0 - Default get value or with port number 
                (byte)((((int)portID) << 4) | _byteGet),

                // byte 1 - Payload length
                0x01,

                // byte 2 - VBUS or VCONN or ADC data
                (byte)byteValue,
          };

            retVal = comm_Read_Write.Read(ref refBuffer, methodName);
            return refBuffer;

        }
        private int CommonCurrentCommand(PortID port, byte vbusVconSteps, byte stepIncDec, int cCline, int vconnEload, int typeCVbusEload, int vbusEload,
            int vCONNModeConfig, int vBUSModeConfig, byte eloadChannels, byte vbusVconnSelection)
        {
            byte[] dataBuffer = new byte[]
            {
                 //Byte 0 - Default value to set data app commands 
                (byte)((((int)port) << 4) | _byteSet),

                //Byte 1 - Length of the app command 
                10,

                //Byte 2 - Test Function Card Commands
                0x02,

                //Byte 3 - Load settings
                0x02,

                //Byte 4 - Decides step inc for VCONN or VBUS
                vbusVconSteps,

                //Byte 5 - Default value, delay between INC/DEC in Multiples of 10uSec 1 Count = 10 uSec
                0x01,

                //Byte 6 - Selects the steps inc or Decreasing 
                stepIncDec,

                //Byte 7 - Reserved
                0x00,

                //Byte 8 - Eload switch for VBUS , VCONN, CCline
                (byte)((cCline << 5) | (vconnEload << 4) | (typeCVbusEload << 1) | vbusEload ),

                //Byte 9 - VBUS , VCONN mode configuration
                (byte)((vCONNModeConfig << 4) | vBUSModeConfig ),

                //Byte 10 - Eload channels used only for calibration
                eloadChannels,

                // Byte 11 - Reserved 
                vbusVconnSelection,
            };

            bool status = comm_Read_Write.Write(dataBuffer, MethodBase.GetCurrentMethod().Name);
            int retValue;
            if (status)
                retValue = (int)APIErrorEnum.NoError;
            else
                retValue = (int)APIErrorEnum.UnknownError;
            return retValue;

        }
        private string AsciiConversion(char value)
        {
            switch (value)
            {
                case 'A':
                    return "10";
                case 'B':
                    return "11";
                case 'C':
                    return "12";
                case 'D':
                    return "13";
                case 'E':
                    return "14";
                case 'F':
                    return "15";
                default:
                    return value.ToString();
            }
        }
        private int CyBtldr_ParseRowData(uint bufSize, char[] buffer, ref byte arrayId, ref ushort rowNum, ref ushort size, ref byte checksum)
        {


            ushort hexSize = 0;
            byte[] hexData = new byte[MAX_BUFFER_SIZE];
            int err;
            if (bufSize <= MIN_SIZE)
            {
                err = CYRET_ERR_LENGTH;
            }
            else if (buffer[0] == ':')
            {
                char[] tempBuffer = null;
                try
                {
                    tempBuffer = new char[bufSize - 1];
                    for (int t = 0; t < buffer.Length - 1; t++)
                        tempBuffer[t] = buffer[t + 1];
                }
                catch (Exception ex)
                {
                    Debug(DebugType.STATUS, MethodBase.GetCurrentMethod().Name, ex);
                }
                err = CyBtldr_FromAscii(bufSize - 1, tempBuffer, ref hexSize, hexData);

                arrayId = hexData[0];
                rowNum = (ushort)((hexData[1] << 8) | (hexData[2]));
                size = (ushort)((hexData[3] << 8) | (hexData[4]));
                checksum = (hexData[hexSize - 1]);

                if ((size + MIN_SIZE) == hexSize)
                {
                    for (int i = 0; i < size; i++)
                    {
                        rowData[i] = (hexData[DATA_OFFSET + i]);
                    }
                }
                else
                {
                    err = CYRET_ERR_DATA;
                }
            }
            else
            {
                err = CYRET_ERR_CMD;
            }

            return err;
        }
        private int CyBtldr_FromAscii(uint bufSize, char[] buffer, ref ushort rowSize, byte[] rowData)
        {
            ushort i;
            int err = CYRET_SUCCESS;

            if ((bufSize & 1) != 0) // Make sure even number of bytes
            {
                err = CYRET_ERR_LENGTH;
            }
            else
            {
                for (i = 0; i < bufSize / 2; i++)
                {
                    rowData[i] = (byte)((CyBtldr_FromHex(buffer[i * 2]) << 4) | CyBtldr_FromHex(buffer[i * 2 + 1]));
                }
                rowSize = i;
            }

            return err;
        }
        private char CyBtldr_FromHex(char value)
        {
            if ('0' <= value && value <= '9')
                return (char)(value - '0');
            if ('a' <= value && value <= 'f')
                return (char)(10 + value - 'a');
            if ('A' <= value && value <= 'F')
                return (char)(10 + value - 'A');
            return '0';
        }
        private void Debug(DebugType debugType, string message, Exception ex = null)
        {
            DebugLogger.Instance.WriteToDebugLogger(debugType, message + " : ", ex);
        }
        private int GetMaxTemperatureValue(int value1, int value2, int value3)
        {

            if (value1 > value2)
            {
                if (value1 > value3)
                {
                    return value1;
                }
                else
                {
                    return value3;
                }

            }
            else if (value2 > value3)
            {
                return value2;
            }
            else
            {
                return value3;
            }
        }
        private bool VBUS_LimitCheck(double vbusValue, VBUSModeConfig vBUSModeConfig)
        {
            string temp;
            if (vBUSModeConfig == VBUSModeConfig.CCMode && vbusValue > 6100)
            {
                temp = $"Value is over the limit 6100 mA";
                API_Error = temp;
                return false;
            }

            if (vBUSModeConfig == VBUSModeConfig.CRMode && vbusValue > 65535)
            {
                temp = $"Value is over the limit 65535 ohms";
                API_Error = temp;
                return false;
            }
            return true;
        }
        private bool VCONN_LimitCheck(double vconnValue, VCONNModeConfig vCONNModeConfig)
        {
            string temp;
            if (vCONNModeConfig == VCONNModeConfig.CCMode && vconnValue > 1600)
            {
                temp = $"Value is over the limit 1600 mA";
                API_Error = temp;
                return false;
            }

            if (vCONNModeConfig == VCONNModeConfig.CRMode && vconnValue > 65535)
            {
                temp = $"Value is over the limit 65535 ohms";
                API_Error = temp;
                return false;
            }

            return true;
        }
        private bool GetReAdverticeOnPorts(int valueReAdvertise, int portID)
        {
            var val = (valueReAdvertise >> (portID - 1)) & 1;
            if (val == 1)
                return true;
            else if (val == 0)
                return false;

            return false;
        }
        private bool GetTesterCardSystemStatus()
        {
            bool retVal = false;
            if (HelperModule.AppType == ApplicationType.V_TE)
            {
                Sleep(100);
            }
            else
            {
                Sleep(100);
            }
            lock (padlockSystemStatus)
            {
                try
                {
                    // No API is required for reading polling data, since data is fetched from the different end point
                    byte[] dataBuffer = new byte[512];

                    if (HelperModule.AppType == ApplicationType.V_TE)
                    {
                        //dataBuffer = new byte[] { 0x17, 0x01, 0x83 };
                        dataBuffer = new byte[] { 0x17, 0x01, 0x0C };
                    }

                    bool status = comm_Read_Write.GetPollingData(ref dataBuffer, out bool runTimeCommand);
                    retVal = status;
                    if (status)
                    {
                        if (HelperModule.AppType == ApplicationType.V_TE)
                        {
                            PollingData_VTE(dataBuffer);
                        }
                        else if (HelperModule.AppType == ApplicationType.V_UP)
                        {
                            PollingData_VUP(dataBuffer);
                        }
                    }
                    else if (!status && runTimeCommand)
                    {
                        _ledSystemStatus.ReturnValue = runTimeCommand;
                        //_ledSystemStatus.ReturnValue = true;
                        HelperModule.Debug("Runtime command was executing......");
                        return GetTesterCardSystemStatus();
                    }
                    else
                    {
                        LEDSystemStatus lEDSystemStatus = new LEDSystemStatus
                        {
                            Error = "Ethernet connection lost"
                        };
                        _ledSystemStatus = lEDSystemStatus;

                        HelperModule.Debug("Read failed ");
                    }
                }
                catch (Exception ex)
                {
                    Debug(DebugType.DEBUG, MethodBase.GetCurrentMethod().Name, ex);
                }
            }
            return retVal;
        }
        private void PollingData_VUP(byte[] dataBuffer)
        {

            LEDSystemStatus lEDSystemStatus = new LEDSystemStatus();
            Dictionary<PortID, SystemStatus> systemStatusList = new Dictionary<PortID, SystemStatus>();
            HeatSinkValues heatSinkValues = new HeatSinkValues();
            lEDSystemStatus.ReturnValue = true;
            int incValue = 3;
            int NO_OF_PORT = 10;
            int totalBytes = dataBuffer[0];
            int TOTAL_PORT_VALUE = dataBuffer[1];
            int TEMP_CHECK = 0xAB;
            int TEMP_CHECK_INDEX = (TOTAL_PORT_VALUE * NO_OF_PORT) + incValue;
            int RE_ADVERT_INDEX = TEMP_CHECK_INDEX + 4;
            int valueReAdvertise = dataBuffer[RE_ADVERT_INDEX + 1] << 8 | dataBuffer[RE_ADVERT_INDEX];
            // Loop runs 10times to get all the 10 ports data of the LED and VBUS values
            for (int i = 0; i < NO_OF_PORT; i++)
            {
                // Port number 
                int value = dataBuffer[incValue];

                if (HelperModule.AppType == ApplicationType.V_UP)
                {
                    if (value != 0)
                    {
                        incValue += TOTAL_PORT_VALUE;
                        continue;
                    }

                }

                SystemStatus systemStatus = new SystemStatus
                {
                    Port = (PortID)value,
                    Re_Advertice = GetReAdverticeOnPorts(valueReAdvertise, value)
                };

                if (HelperModule.AppType == ApplicationType.V_TE)
                {
                    systemStatus.Port = PortID.Port1;
                }

                // PDC status and bi-color LED status
                value = dataBuffer[incValue + 1];
                systemStatus.PDC_Status = (value & 0x01);
                systemStatus.PDO_Index = ((value & 0x0E) >> 1);
                systemStatus.Power = (LED_Color)((value & 0x10) >> 4);
                systemStatus.DataLock = (LED_Color)((~value & 0x40) >> 6);
                systemStatus.VBUS = (LED_Color)(((~value & 0x20) >> 5) << 1);
                systemStatus.PD_N = (LED_Color)(((~value & 0x80) >> 7) << 1);


                // tricolor LED status 
                value = dataBuffer[incValue + 2];
                systemStatus.En_D_Tx = (LED_Color)(value & 0x03);
                systemStatus.LinkSpeed = (LED_Color)((value & 0xC) >> 2);
                systemStatus.PD_BC_12 = (LED_Color)((value & 0x30) >> 4);
                systemStatus.DataError = (LED_Color)((value & 0xC0) >> 6);

                // VBUS voltage 
                value = (dataBuffer[incValue + 4] << 8) | dataBuffer[incValue + 3];
                systemStatus.VBUS_Voltage = value;

                // VBUS current 
                value = (dataBuffer[incValue + 6] << 8) | dataBuffer[incValue + 5];
                systemStatus.VBUS_Current = value;

                // VCONN Voltage
                value = (dataBuffer[incValue + 8] << 8) | dataBuffer[incValue + 7];
                systemStatus.VCONN_Voltage = value;

                // VCONN Current
                value = (dataBuffer[incValue + 10] << 8) | dataBuffer[incValue + 9];
                systemStatus.VCONN_Current = value;


                // Get Time Stamp in ms and convert it in to Tick
                value = (dataBuffer[incValue + 12] << 8) | dataBuffer[incValue + 11];
                long ticks = (value) * 10000;
                value = (dataBuffer[incValue + 14] << 8) | dataBuffer[incValue + 13];

                // Get Time stamps in minutes and convert it in to Ticks and add it to main ticks
                ticks += value * 10000 * 1000 * 60;

                // Add timeStamps
                systemStatus.TimeSpan = new TimeSpan(ticks);

                // Reserve byte value.
                value = dataBuffer[incValue + 15];

                // Battery state of charge in kWh
                value = (dataBuffer[incValue + 17] << 8) | dataBuffer[incValue + 16];
                systemStatus.BatterySoC = value;

                // Battery charging status 
                value = dataBuffer[incValue + 18] & 3;
                systemStatus.BatteryStatus = (BatteryChargingStatus)value;

                // Temperature status
                value = (dataBuffer[incValue + 18] >> 2) & 3;
                systemStatus.TemperatureStatus = (BatteryTemperatureStatus)value;

                // Temperature status
                value = (dataBuffer[incValue + 19]);
                systemStatus.BatteryTemperature = value;

                if (!systemStatusList.ContainsKey(systemStatus.Port) && systemStatus.Port != PortID.NONE)
                    systemStatusList.Add(systemStatus.Port, systemStatus);

                //HelperModule.Debug(systemStatus.ToString());
            }


            // Get the temperature Value                  
            if (dataBuffer[TEMP_CHECK_INDEX] == TEMP_CHECK && HelperModule.AppType == ApplicationType.V_UP)
            {
                heatSinkValues.TempratureValue1 = dataBuffer[TEMP_CHECK_INDEX + 1];
                heatSinkValues.TempratureValue2 = dataBuffer[TEMP_CHECK_INDEX + 2];
                heatSinkValues.TempratureValue3 = dataBuffer[TEMP_CHECK_INDEX + 3];
                heatSinkValues.MaxTemperature = GetMaxTemperatureValue(heatSinkValues.TempratureValue1, heatSinkValues.TempratureValue2, heatSinkValues.TempratureValue3);
            }
            else if (dataBuffer[TEMP_CHECK_INDEX] == TEMP_CHECK && HelperModule.AppType == ApplicationType.V_TE)
            {
                heatSinkValues.TempratureValue1 = dataBuffer[TEMP_CHECK_INDEX + 1];
                heatSinkValues.TempratureValue2 = heatSinkValues.TempratureValue1;
                heatSinkValues.TempratureValue3 = heatSinkValues.TempratureValue1;
                heatSinkValues.MaxTemperature = GetMaxTemperatureValue(heatSinkValues.TempratureValue1, heatSinkValues.TempratureValue2, heatSinkValues.TempratureValue3);
            }


            lEDSystemStatus.SystemStatusList = systemStatusList;
            lEDSystemStatus.TemperatureValues = heatSinkValues;

            _ledSystemStatus = lEDSystemStatus;
        }
        private void PollingData_VTE(byte[] dataBuffer)
        {
            LEDSystemStatus lEDSystemStatus = new LEDSystemStatus();
            Dictionary<PortID, SystemStatus> systemStatusList = new Dictionary<PortID, SystemStatus>();
            HeatSinkValues heatSinkValues = new HeatSinkValues();
            lEDSystemStatus.ReturnValue = true;

            int incValue = 2;
            int NO_OF_PORT = 1;
            int valueReAdvertise = 0;
            int TOTAL_PORT_VALUE = dataBuffer[1] /*14 bytes*/;
            //int TEMP_CHECK_INDEX = (TOTAL_PORT_VALUE * NO_OF_PORT) + incValue + 1;
            int TEMP_CHECK_INDEX = (TOTAL_PORT_VALUE * NO_OF_PORT) + incValue;

            int portNumber = dataBuffer[02];
            SystemStatus systemStatus = new SystemStatus
            {
                Port = (PortID)portNumber,
            };

            // PDC status and bi-color LED status
            int value = dataBuffer[incValue + 1];
            systemStatus.PDC_Status = (value & 0x01);
            systemStatus.PDO_Index = ((value & 0x0E) >> 1);
            systemStatus.Power = (LED_Color)((value & 0x10) >> 4);
            systemStatus.DataLock = (LED_Color)((~value & 0x40) >> 6);
            systemStatus.VBUS = (LED_Color)(((~value & 0x20) >> 5) << 1);
            systemStatus.PD_N = (LED_Color)(((~value & 0x80) >> 7) << 1);

            // tricolor LED status 
            value = dataBuffer[incValue + 2];
            systemStatus.En_D_Tx = (LED_Color)(value & 0x03);
            systemStatus.LinkSpeed = (LED_Color)((value & 0xC) >> 2);
            systemStatus.PD_BC_12 = (LED_Color)((value & 0x30) >> 4);
            systemStatus.DataError = (LED_Color)((value & 0xC0) >> 6);

            // VBUS voltage 
            value = (dataBuffer[incValue + 4] << 8) | dataBuffer[incValue + 3];
            systemStatus.VBUS_Voltage = value;

            // VBUS current 
            value = (dataBuffer[incValue + 6] << 8) | dataBuffer[incValue + 5];
            systemStatus.VBUS_Current = value;

            // VCONN Voltage
            value = (dataBuffer[incValue + 8] << 8) | dataBuffer[incValue + 7];
            systemStatus.VCONN_Voltage = value;

            // VCONN Current
            value = (dataBuffer[incValue + 10] << 8) | dataBuffer[incValue + 9];
            systemStatus.VCONN_Current = value;

            // Get Time Stamp in ms and convert it in to Tick
            value = (dataBuffer[incValue + 12] << 8) | dataBuffer[incValue + 11];
            long ticks = (value) * 10000;
            value = (dataBuffer[incValue + 14] << 8) | dataBuffer[incValue + 13];

            // Get Time stamps in minutes and convert it in to Ticks and add it to main ticks
            ticks += value * 10000 * 1000 * 60;

            // Add timeStamps
            systemStatus.TimeSpan = new TimeSpan(ticks).Duration();

            // Reserve byte value.
            valueReAdvertise = dataBuffer[incValue + 15];
            systemStatus.Re_Advertice = GetReAdverticeOnPorts(valueReAdvertise, portNumber);

            // Battery SOC valid or not.
            value = ((dataBuffer[incValue + 15] >> 1) & 0x1);
            systemStatus.IsBatteryValueValid = (value == 1 ? true : false);

            // Battery state of charge in kWh
            value = (dataBuffer[incValue + 17] << 8) | dataBuffer[incValue + 16];
            systemStatus.BatterySoC = value;

            // Battery charging status 
            value = dataBuffer[incValue + 18] & 3;
            systemStatus.BatteryStatus = (BatteryChargingStatus)value;

            // Temperature status
            value = (dataBuffer[incValue + 18] >> 2) & 3;
            systemStatus.TemperatureStatus = (BatteryTemperatureStatus)value;

            // Temperature status
            value = (dataBuffer[incValue + 19]);
            systemStatus.BatteryTemperature = value;


            // DUT VID [22:23] 
            value = (dataBuffer[incValue + 21] << 8) | dataBuffer[incValue + 20];
            systemStatus.DUT_VID = value;

            // DUT PID [24:25] 
            value = (dataBuffer[incValue + 23] << 8) | dataBuffer[incValue + 22];
            systemStatus.DUT_PID = value;

            // Battery Design capacity [26:27] 
            value = (dataBuffer[incValue + 25] << 8) | dataBuffer[incValue + 24];
            systemStatus.BatteryDesignCapacity = value;

            // Battery Last full charge capacity [28:29] 
            value = (dataBuffer[incValue + 27] << 8) | dataBuffer[incValue + 26];
            systemStatus.BatteryPreviousChargeCapacity = value;

            // Battery Type [30] 
            value = dataBuffer[incValue + 28];
            systemStatus.BatteryType = value;


            if (!systemStatusList.ContainsKey(systemStatus.Port) && systemStatus.Port != PortID.NONE)
                systemStatusList.Add(systemStatus.Port, systemStatus);

            heatSinkValues.TempratureValue1 = dataBuffer[TEMP_CHECK_INDEX + 1];
            heatSinkValues.TempratureValue2 = heatSinkValues.TempratureValue1;
            heatSinkValues.TempratureValue3 = heatSinkValues.TempratureValue1;
            heatSinkValues.MaxTemperature = GetMaxTemperatureValue(heatSinkValues.TempratureValue1, heatSinkValues.TempratureValue2, heatSinkValues.TempratureValue3);
            lEDSystemStatus.SystemStatusList = systemStatusList;
            lEDSystemStatus.TemperatureValues = heatSinkValues;
            _ledSystemStatus = lEDSystemStatus;
        }
        private void ClearReadBuffer()
        {


            //bool bResult = true;
            //comm_Read_Write.USBLinkComm.USBCommunication.CyBulkEndPointIn.TimeOut = 10;
            //while (bResult)
            //{
            //    byte[] emptyBuf = new byte[0x10000];
            //    int byteLength = emptyBuf.Length;
            //    bResult = comm_Read_Write.USBLinkComm.USBCommunication.CyBulkEndPointIn.XferData(ref emptyBuf, ref byteLength, false);
            //    DebugLogger.Instance.WriteToDebugLogger(DebugType.STATUS, "Clearing read buffer");
            //}
            //comm_Read_Write.USBLinkComm.USBCommunication.CyBulkEndPointIn.TimeOut = USBLinkStatusInfo.IN_TIMEOUT;
        }
        private void Sleep(int waitTimeMilli)
        {
            if (waitTimeMilli <= 0)
                waitTimeMilli = 1;

            int i = 0;
            System.Timers.Timer delayTimer = new System.Timers.Timer(waitTimeMilli)
            {
                AutoReset = false //so that it only calls the method once
            };
            delayTimer.Elapsed += (s, args) => i = 1;
            delayTimer.Start();
            while (i == 0) { };
        }
        #endregion

        #region PPS Private Module 
        private GetPPSADCData Get_PPS_ADC_Data_Command()
        {
            GetPPSADCData getPPSDataResult = new GetPPSADCData();
            byte[] dataBuffer = new byte[] { (((int)PortID.Port1) << 4) | _byteGet, 0x00, 0xA1 };
            dataBuffer[1] = (byte)(dataBuffer.Length - 2);
            bool retVal = comm_Read_Write.Read(ref dataBuffer, MethodBase.GetCurrentMethod().Name);
            if (retVal)
            {
                if (dataBuffer.Length > 11)
                {
                    getPPSDataResult.ADC_Voltage_Data = (dataBuffer[4] << 8) | dataBuffer[3];
                    getPPSDataResult.ADC_Current_Data = (dataBuffer[6] << 8) | dataBuffer[5];
                    getPPSDataResult.DAC_Voltage_Data = (dataBuffer[8] << 8) | dataBuffer[7];
                    getPPSDataResult.FirmwareVersion = $"{AsciiConversion((char)dataBuffer[10])}.{AsciiConversion((char)dataBuffer[11])}";
                    getPPSDataResult.ReturnValue = true;
                }
            }
            return getPPSDataResult;
        }
        #endregion

        #endregion
    }
}
