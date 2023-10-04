using System.Reflection;
using VseriesControllerLibrary_V1.Commands.HelperModules.PollingModules;
using VseriesControllerLibrary_V1.HelperModules.PDO_Decoder;
using VseriesControllerLibrary_V1.SysData;
using VseriesControllerLibrary_V1.SysData.SysRead;
using VseriesControllerLibrary_V1.SysData.SysWrite;

namespace VseriesControllerLibrary_V1
{

    /// <summary>
    /// This is singletone class library will access communicate with GRL-V-DPWR controllers for concurrent USB Power Delivery 3.0 negotiation, 1000W power loading, and USB 2.0 & USB 3.1 data loop-back testing.
    /// </summary>
    public sealed class GrlVPdApiLib
    {
        #region Private Members
        private VsCommandSets _vCommands = null;
        //private Firmware firmware_CMCore;
        private VsCommandSets _vCommands_LoopBack = null;
        private CalibReadDecoder calibReadDecoder;
        private CalibrationWrite _calibWrite;
        private static readonly object padlock = new object();
        private static GrlVPdApiLib instance = null;
        //private USBDeviceList usb_Devices_Lists;
        private ConnectDevices deviceHandler;

        private double vbusCurrent = 0;
        private double vconnCurrent = 0;
        private PortID port = PortID.NONE;
        private VbusEload vbusEload = VbusEload.Off;
        private TypeCVbusEload typeCVbusEload = TypeCVbusEload.Off;
        private VconnEload vconnEload = VconnEload.Off;
        private CCline cCline = CCline.CC1;
        private VBUSModeConfig vBUSModeConfig = VBUSModeConfig.CCMode;
        private VCONNModeConfig vCONNModeConfig = VCONNModeConfig.CCMode;

        #endregion

        #region Public Members 

        /// <summary>
        /// This will call a list containing details of the connected controller.
        /// </summary>
        public Dictionary<string, GRLDeviceList> GetGRLDeviceList
        {
            get
            {
                if (deviceHandler == null)
                    return null;
                return deviceHandler.VControllersList;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Default Constructor
        /// </summary>
        private GrlVPdApiLib()
        {
            deviceHandler = new ConnectDevices();
           // DebugLogger.Instance.Create();
        }

        /// <summary>
        /// This will call an Instance of the  GrlVPdApiLib class.
        /// </summary>
        public static GrlVPdApiLib Instance
        {
            get
            {

                if (instance == null)
                {
                    lock (padlock)
                    {
                        if (instance == null)
                        {
                            instance = new GrlVPdApiLib();
                        }
                    }
                }
                return instance;
            }
        }

        #endregion

        #region Connection 

        /// <summary>
        /// This function will detect all GRL USB devices that are connected to the Host PC and establish 
        /// communication with each of them and add to GetGRLDeviceList. Alternatively you can use 
        /// this "GetUSBDevices()" API manually and pass the parameter. Once all the devices are connected 
        /// using the "SelectController()" API, select the specific controller to communicate with.
        /// 
        /// GrlVPdApiLib.Instance.InitilizeController();
        /// </summary>
        /// <param name="uSBDeviceList">Pass the "USBDeviceList". If this is null then manually it will connect all connected devices to the Host PC.</param>
        /// <returns>boolean : True = Successful , False = Failed </returns>
        public bool InitilizeController()
        {
            try
            {
                //DetectDevice();
            }
            catch (Exception ex)
            {
                HelperModule.Debug(MethodBase.GetCurrentMethod().Name, ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// This function will Initialize controller over Ethernet IP address. 
        /// </summary>
        /// <param name="ipAddress">IP Address ex: 192.168.0.4</param>
        /// <returns>Boolean : True / False</returns>
        public bool InitilizeController(string ipAddress)
        {
            bool retValue;
            try
            {
                retValue = deviceHandler.Connect(ipAddress);
                SelectController(ipAddress);
                //DetectDevice();
            }
            catch (Exception ex)
            {
                HelperModule.Debug(MethodBase.GetCurrentMethod().Name, ex);
                return false;
            }

            return retValue;
        }

        /// <summary>
        /// This function will disconnect the controller from the list provided IP Address. 
        /// </summary>
        /// <param name="ipAddress">IP Address ex: 192.168.0.4</param>
        /// <returns>Boolean : True / False</returns>
        public bool DisconnectController(string ipAddress)
        {
            try
            {
                if (GetGRLDeviceList.ContainsKey(ipAddress))
                    deviceHandler.Disconnect(ipAddress);
            }
            catch (Exception ex)
            {
                HelperModule.Debug(MethodBase.GetCurrentMethod().Name, ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// This function will select the controller to communicate with.
        /// If multiple controllers are connected, obtain the controller's list using "GetGRLDeviceList" and 
        /// set "ControllerIndex" as "GetGRLControllersList.ControllerIndex". If multiple GRL-V-DPWR controllers 
        /// are connected, then select the controller that you want to communicate with.
        ///
        /// Here 0th Controller is selected
        ///
        /// foreach (var device in GrlVPdApiLib.Instance.GetGRLDeviceList)
        /// {
        ///     GrlVPdApiLib.Instance.SelectController(device.Value.USBSerialNumber);
        ///     Console.WriteLine("Controller Serial number : " + device.Value.USBSerialNumber);
        ///     break;
        /// }
        /// </summary>
        /// <param name="serialNumber">Device serial number ex: device.Value.USBSerialNumber</param>
        public void SelectController(string serialNumber)
        {
            try
            {
                if (!(GetGRLDeviceList != null && GetGRLDeviceList.Count > 0 && serialNumber != ""))
                {
                    return;
                }

                GetGRLDeviceList.TryGetValue(serialNumber, out GRLDeviceList gRLDeviceList);
                if (gRLDeviceList != null && !(gRLDeviceList.FriendlyName.Contains("LoopBack")))
                    _vCommands = gRLDeviceList.ControllerObject as VsCommandSets;
            }
            catch (Exception ex)
            {
                HelperModule.Debug(MethodBase.GetCurrentMethod().Name, ex);
            }
        }


        /// <summary>
        /// This function will select the controller for loop-back operation.
        /// </summary>
        /// <param name="serialNumber">Serial number</param>
        public void SelectLoopBackController(string serialNumber)
        {
            try
            {
                GRLDeviceList gRLDeviceList = null;
                if (GetGRLDeviceList != null && GetGRLDeviceList.Count > 0)
                {
                    GetGRLDeviceList.TryGetValue(serialNumber, out gRLDeviceList);
                    if (gRLDeviceList != null && gRLDeviceList.FriendlyName.Contains("LoopBack"))
                        _vCommands_LoopBack = gRLDeviceList.ControllerObject as VsCommandSets;
                }
            }
            catch (Exception ex)
            {
                HelperModule.Debug(MethodBase.GetCurrentMethod().Name, ex);
            }

        }

        /// <summary>
        /// This will call the connection status of the specific controller.
        /// </summary>
        public USBConnectionStatus USBConnectionStatus
        {
            get
            {
                return _vCommands.USBConnectionStatus;
            }
        }

        /// <summary>
        /// This holds the connection status
        /// </summary>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>return true or false based on the connection status</returns>
        public bool IsOpen(string serialNumber)
        {

            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).IsOpen;
            }
            return false;

        }

        /// <summary>
        /// This holds true when the connection check is in progress dll will be trying to reconnect to the hardware 
        /// If this is false and IsOpen is false then conneciton is lost
        /// </summary>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>return true or false based on the connection status</returns>
        public bool IsRetry(string serialNumber)
        {

            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).IsRetry;
            }
            return false;

        }

        #endregion

        #region Private Modules

        //private void DetectDevice(USBDeviceList uSBDeviceList = null)
        //{
        //    USBDeviceList devicesList = uSBDeviceList;
        //    if (devicesList == null)
        //        devicesList = GetUSBDevices();

        //    foreach (var device in devicesList)
        //        deviceHandler.Connect(device as CyFX3Device);

        //    // Default 0th Controller will be selected
        //    foreach (var device in GetGRLDeviceList)
        //    {
        //        if (!device.Value.FriendlyName.Contains("LoopBack"))
        //            SelectController(device.Value.USBSerialNumber);
        //    }
        //    //DictonaryEventHandler.OnController_DeviceAddedChanged(null);
        //    DictonaryEventHandler.OnLoopback_DeviceAddedChanged(null);
        //}
        //private void USBDevices_DeviceAttached(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        USBEventArgs uSBEventArgs = e as USBEventArgs;

        //        if (!GetGRLDeviceList.ContainsKey(uSBEventArgs.SerialNum))
        //        {
        //            var deviceList = sender as USBDeviceList;
        //            foreach (var device in deviceList)
        //                deviceHandler.AddDevice(device as CyFX3Device);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug(DebugType.DEBUG, "USBDevices_DeviceAttached :", ex);
        //    }
        //}
        //private void USBDevices_DeviceRemoved(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        USBEventArgs uSBEventArgs = e as USBEventArgs;
        //        if (GetGRLDeviceList.ContainsKey(""))
        //            deviceHandler.RemoveDevice("");

        //        if (GetGRLDeviceList.ContainsKey(uSBEventArgs.SerialNum))
        //            deviceHandler.RemoveDevice(uSBEventArgs.SerialNum);

        //    }
        //    catch (Exception ex)
        //    {
        //        Debug(DebugType.DEBUG, "USBDevices_DeviceAttached :", ex);
        //    }
        //}

        #endregion

        #region External API

        #region Get Commands

        /// <summary>
        /// This function will return the Attach / Detach status of each DUT connected to individual port.
        /// </summary>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>Dictionary<port, Attach_Detach_Status_Enum></returns>
        public Dictionary<PortID, Attach_Detach_Status_Enum> AttachDetachStatus(string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).AttachDetachStatus();
            }
            else
            {
                return new Dictionary<PortID, Attach_Detach_Status_Enum>();
            }
        }

        /// <summary>
        /// This function will provide information on the physical Attach status of the tester card on the GRL-V-DPWR unit.
        /// </summary>
        /// <param name = "serialNumber" > IP address of the controller ex - 192.168.0.4</param>
        /// <returns>Dictionary<port, Attach_Detach_Status_Enum></returns>
        public Dictionary<PortID, Attach_Detach_Status_Enum> Physical_TesterCardStatus(string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).PhysicalTesterCardStatus();
            }
            else
            {
                return new Dictionary<PortID, Attach_Detach_Status_Enum>();
            }

        }

        /// <summary>
        /// This function will return the system status of all tester cards on the GRL-V-DPWR unit, which includes the following: 
        /// PDO_Index, PDC_Status, Port number, Power LED status, VBUS LED status, DataLock LED status, 
        /// DataError LED status, En_D_Tx LED status, PD_BC_12 LED status, PD_N LED status, LinkSpeed LED status, 
        /// VBUS_Voltage, VBUS_Current, VCONN_Voltage, VCONN_Current , Source caps Re-Advertised Timestamps
        /// 
        /// NOTE: Source caps Re-Advertised will return as "False"  again only after sending the source capabilities API.
        /// </summary>
        /// <param name = "serialNumber" > IP address of the controller ex - 192.168.0.4</param>
        /// <returns></returns>
        public LEDSystemStatus TesterCard_System_Status(string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).TesterCardSystemStatus();
            }
            else
            {
                return new LEDSystemStatus();
            }

        }

        /// <summary>
        /// This function will get the system information
        /// </summary>
        /// <param name="port"> Refer to Port Number Port1.</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>Returns string value of the system information</returns>
        public string GetSystemInfo(PortID port, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).FWSystemInfo(port);
            }
            else
            {
                return "Not Initialized";
            }
        }

        /// <summary>
        /// Depending on the controller mode, this function will return the VBUS current that 
        /// the DUT supplies to the tester for the respective port. 
        /// </summary>
        /// <param name="port"> Refer to Port Number Port1.</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>VBUS current in milliampere</returns>
        public int GetVbusCurrent(PortID port, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).GetData(port, Voltage_Current_Data.VBUS, VBUS_VCONN_Data.Current);
            }
            else
            {
                return -1;
            }

        }

        /// <summary>
        ///  Depending on the controller mode, this function will return the VBUS voltage and current that the DUT supplies to the tester in mA for the respective port.
        /// </summary>
        /// <param name="port"> Refer to Port Number Port1.</param>
        /// <param name="vbusVoltage">VBUS Voltage in mA</param>
        /// <param name="vbusCurrent">VBUS current in mA</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>Boolean : True = Successful , False = Failed </returns>
        public bool Get_VBUS_Voltage_Current(PortID port, out int vbusVoltage, out int vbusCurrent, string serialNumber)
        {
            vbusVoltage = 0;
            vbusCurrent = 0;

            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).GetData(port, out vbusVoltage, out vbusCurrent);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Depending on the controller mode, this function will return the VCONN voltage that the DUT supplies to the tester for the respective port.
        /// </summary>
        /// <param name="port"> Refer to Port Number Port1.</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>VCONN voltage in millivolts</returns>
        public int GetVconnVoltage(PortID port, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).GetData(port, Voltage_Current_Data.VCONN, VBUS_VCONN_Data.Voltage);
            }
            else
            {
                return -1;
            }

        }

        /// <summary>
        /// This function will return the VBUS voltage that the DUT supplies to the tester for the respective port. 
        /// It is recommended to verify the DUT Attach status before calling the "GetVbusVoltage" API.
        /// </summary>
        /// <param name="port"> Refer to Port Number Port1.</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>VBUS voltage in millivolts</returns>
        public int GetVbusVoltage(PortID port, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).GetData(port, Voltage_Current_Data.VBUS, VBUS_VCONN_Data.Voltage);
            }
            else
            {
                return -1;
            }

        }

        /// <summary>         
        /// This function will return the VBUS voltage and current that the DUT supplies to the tester for the respective port. 
        /// It is recommended to verify the DUT Attach status before calling the "GetVBUSData" API.
        /// </summary>
        /// <param name="port"> Refer to Port Number Port1.</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>String value - Voltage in volts , Current in amps</returns>
        public string GetVBUSData(PortID port, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).GetData(port, Voltage_Current_Data.VBUS);
            }
            else
            {
                return "Not Initialized";
            }
        }

        /// <summary>
        /// Depending on the controller mode, this function will return the VCONN voltage and current that the DUT supplies to the tester for the respective port.
        /// </summary>
        /// <param name="port"> Refer to Port Number Port1.</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>String value - Voltage in volts, Current in amps</returns>
        public string GetVCONNData(PortID port, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).GetData(port, Voltage_Current_Data.VCONN);
            }
            else
            {
                return "Not Initialized";
            }
        }

        /// <summary>
        /// This function will set the polling iteration count.
        /// </summary>
        /// <param name="port"> Refer to port Number Port1,Port2,..., Port10.</param>
        /// <param name="InitiateBatteryStatus"> Initiate Battery status if required</param>
        /// <param name="pollingTimeinMilliSeconds"> Polling time in millisecond </param>
        /// <param name="serialNumber">IF V-DPWR then controller serial number else if V-DPWR then IP address</param>
        /// <returns>Boolean : True = Successful , False = Failed </returns>
        public bool Polling_Iteration_Control(PortID port, bool InitiateBatteryStatus, uint pollingTimeinMilliSeconds, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).Polling_Iteration_Control(port, InitiateBatteryStatus, pollingTimeinMilliSeconds);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// This function will turn ON/OFF the controller fan.
        /// </summary>
        /// <param name="fan">FanControl.On , FanControl.Of</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns> APIErrorEnum.NoError = 0, APIErrorEnum.UnknownError = -1, APIErrorEnum.USBLinkError = -2,</returns>
        public int FanTurnOnOff(FanControl fan, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).FanTurnOnOff(fan);
            }
            else
            {
                return -1;
            }

        }

        /// <summary>
        /// This function will turn ON/OFF the Tester-card fans.
        /// </summary>
        /// <param name="fan">FanControl.On , FanControl.Of</param>
        /// <param name="port"> Refer to Port Number Port1.</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns> NoError = 0, UnknownError = -1, USBLinkError = -2,</returns>
        public int TestercardFanControl(PortID port, TCFans fan, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).TestercardFanControl(port, fan);
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Depending on the controller mode, this function will return the VCONN current that the DUT supplies to the tester for the respective port. 
        /// It is recommended to verify the DUT Attach status before calling the "GetVconnCurrent" API.
        /// </summary>
        /// <param name="port"> Refer to Port Number Port1.</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>VCONN current in Milli-amps</returns>
        public double GetVconnCurrent(PortID port, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).GetData(port, Voltage_Current_Data.VCONN, VBUS_VCONN_Data.Current);
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Depending on the controller mode, this function will return the Link Speed for the respective port.
        /// It is recommended to verify the DUT Attach status before calling the "GetLinkSpeed" API.
        /// </summary>
        /// <param name="port"> Refer to Port Number Port1.</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>LinkSpeed </returns>
        public double GetLinkSpeed(PortID port, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).LinkSpeed(port);
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// This function will return the tester card firmware version for the respective port.
        /// </summary>
        /// <param name="port"> Refer to Port Number Port1.</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>string : Tester card firmware version</returns>
        public string Get_TesterCard_Firmware_Version(PortID port, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {

                return (gRLDeviceList.ControllerObject as VsCommandSets).GetFirmwareVersion(port, FirmwareName.TesterCard);
            }
            else
            {
                return "Not Initialized ";
            }
        }

        /// <summary>
        /// This function will return the PPS firmware version for the respective port.
        /// </summary>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>string : PPS firmware version</returns>
        public string Get_PPS_Firmware_Version(string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).GetFirmwareVersion(PortID.NONE, FirmwareName.PPS);

            }
            else
            {
                return "Not Initialized";
            }
        }

        /// <summary>
        /// This function will return the Connectivity Manager firmware version for the respective port.
        /// </summary>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>string : Connectivity Manager firmware version</returns>
        public string Get_Connectivity_Manager_Firmware_Version(string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).GetFirmwareVersion(PortID.NONE, FirmwareName.Connectivity_Manager);
            }
            else
            {
                return "Not Initialized";
            }

        }

        /// <summary>
        /// This function will return the controller card firmware version.
        /// </summary>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>string : Controller card firmware version</returns>
        public string Get_ControlCard_Firmware_Version(string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).GetFirmwareVersion(PortID.NONE, FirmwareName.Controller);

            }
            else
            {
                return "Not Initialized";
            }

        }

        /// <summary>
        /// This function will return the E-Load firmware version for the respective port.
        /// </summary>
        /// <param name="port"> Refer to Port Number Port1.</param>
        ///  <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>string : E-Load firmware version</returns>
        public string Get_Eload_Firmware_Version(PortID port, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).GetFirmwareVersion(port, FirmwareName.Eload);
            }
            else
            {
                return "Not Initialized";
            }
        }

        /// <summary>
        /// This function will return the PD Controller Firmware Version for the respective port.
        /// </summary>
        /// <param name="port"> Refer to Port Number Port1.</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>string : PD_Controller Firmware version</returns>
        public string Get_PD_Controller_Firmware_Version(PortID port, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).GetFirmwareVersion(port, FirmwareName.PD_Controller);
            }
            else
            {
                return "Not Initialized";
            }
        }

        /// <summary>
        /// This function will return the tester card serial number and board number for the respective port.
        /// </summary>
        /// <param name="port"> Refer to Port Number Port1.</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>string : Tester Card Serial Number ,  BaseBoard Revision </returns>
        public string Get_TesterCard_SerialNumber(PortID port, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).GetTesterCardSerialNumber(port);
            }
            else
            {
                return "Not Initialized";
            }

        }

        /// <summary>
        /// This function will return the tester card serial number and board number for the respective port
        /// </summary>
        /// <param name="port">Refer to Port Number Port1.</param>
        /// <param name="testerCardSerialNumber">out int : Tester card serial number</param>
        /// <param name="testerCardBoradNumber">out int : Tester card board revision number</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>Boolean : True = Successful , False = Failed </returns>
        public bool Get_TesterCard_SerialNumber(PortID port, out int testerCardSerialNumber, out int testerCardBoradNumber, string serialNumber)
        {
            testerCardSerialNumber = 0;
            testerCardBoradNumber = 0;
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).GetTesterCardSerialNumber(port, out testerCardSerialNumber, out testerCardBoradNumber);
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// This function will return the controller card serial number.
        /// </summary>
        /// <param name="controlCardSerialNumber">out int: control card serial number</param>
        /// <param name="controlCardBoradNumber">out int: control card board revision number</param>
        /// <param name="backPanelSerialNumber">out int: back panel serial number</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>Boolean : True = Successful , False = Failed </returns>
        public string Get_ControlCard_SerialNumber(string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {

                return (gRLDeviceList.ControllerObject as VsCommandSets).GetControlCardSerialNumber();
            }
            else
            {
                return "Not Initialized";
            }

        }

        /// <summary>
        /// This function will return the controller card serial number.
        /// </summary>
        /// <param name="controlCardSerialNumber">out int: control card serial number</param>
        /// <param name="controlCardBoradNumber">out int: control card board revision number</param>
        /// <param name="ppsSerialNumber">out int: pps serial number</param>
        /// <param name="ppsBoardNumber">out int: pps board rev number</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>Boolean : True = Successful , False = Failed </returns>
        public bool Get_ControlCard_SerialNumber(out int controlCardSerialNumber, out int controlCardBoradNumber, out int ppsSerialNumber, out int ppsBoardNumber, string serialNumber)
        {
            controlCardSerialNumber = 0;
            controlCardBoradNumber = 0;
            ppsSerialNumber = 0;
            ppsBoardNumber = 0;
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).GetControlCardSerialNumber(out controlCardSerialNumber, out controlCardBoradNumber, out ppsSerialNumber, out ppsBoardNumber);
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// This function will return the E-Load version for the respective port.
        /// </summary>
        /// <param name="port">Refer to Port Number Port1.</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>String : E-Load version</returns>
        public string Get_ELoad_Frimware_Version(PortID port, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).FWELoad(port);
            }
            else
            {
                return "Not Initialized";
            }

        }

        /// <summary>
        /// This function will return the DUT capabilities for the respective port.
        /// </summary>
        /// <param name="port"> Refer to Port Number Port1.</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>String : DUT capabilities ; It will return empty string if you select PortAll</returns>
        public string GetDutCapabilities(PortID port, string serialNumber)
        {
            if (port == PortID.PortAll)
                return "Not Initialized";

            //SourceCapabilities sourceCaps = new SourceCapabilities();
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {

                SourceCapabilities sourceCaps = (gRLDeviceList.ControllerObject as VsCommandSets).DecoderSourceCaps(port);
                return DecodeToStringSrcCaps(sourceCaps);
            }
            else
            {
                return "Not Initialized";
            }

        }

        /// <summary>
        /// This function will return the Source DUT capabilities for the respective port.
        /// </summary>
        /// <param name="port"> Refer to Port Number Port1.</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>SourceCapabilities : DUT capabilities </returns>
        public SourceCapabilities SourceCapabilities(PortID port, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).DecoderSourceCaps(port);
            }
            else
            {
                return new SourceCapabilities();
            }
        }

        /// <summary>
        /// This function will return the Source DUT capabilities for the respective port.
        /// </summary>
        /// <param name="port"> Refer to Port Number Port1.</param>
        /// <param name="SrcCaps">out string : SourceCapabilities </param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>SourceCapabilities : DUT capabilities ; </returns>
        public SourceCapabilities SourceCapabilities(PortID port, out string SrcCaps, string serialNumber)
        {

            SrcCaps = "";
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                SourceCapabilities sourceCaps = (gRLDeviceList.ControllerObject as VsCommandSets).DecoderSourceCaps(port);
                SrcCaps = DecodeToStringSrcCaps(sourceCaps);
                return sourceCaps;

            }
            else
            {
                return new SourceCapabilities();
            }

        }

        /// <summary>
        /// This function will return the Source PDP rating for the respective port.
        /// </summary>
        /// <param name="port"> Refer to Port Number Port1.</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>int value of Source PDP rating in watts</returns>
        public int SourceCapabilities_Extended(PortID port, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).Get_SRC_CAPS_Extended(port);
            }
            else
            {
                return 0;
            }

        }

        /// <summary>
        /// This function will return the status of the PD negotiation for the respective port.
        /// </summary>
        /// <param name="port"> Refer to Port Number Port1.</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>PD negotiation status- Requested PDO, Operating current, Maximum Operating current, 
        /// VBUS voltage and Communication Line. If there is no status update, it will return as Failed.
        /// </returns>        
        public string GetPDContractNegotationStatus(PortID port, string serialNumber)
        {

            string strData = "Failed to get status";
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                PDC_Status objstatus = (gRLDeviceList.ControllerObject as VsCommandSets).DecoderPdcStatus(port);
                if (objstatus != null)
                {
                    return strData = objstatus.ToString();
                }
                else
                {
                    return strData;
                }

            }
            else
            {
                return "Not Initialized";
            }

        }

        /// <summary>
        /// This function will return the decoder PD negotiation status of the respective port.
        /// </summary>
        /// <param name="port">Refer to Port Number Port1.</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>PDC_Status : Status</returns>
        public PDC_Status DecoderPDContractNegotationStatus(PortID port, string serialNumber)
        {

            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).DecoderPdcStatus(port);
            }
            else
            {
                return new PDC_Status();
            }

        }

        /// <summary>
        /// This function will switch between the USB Type-C VBUS and VBUS Sense for the respective port.
        /// </summary>
        /// <param name="port"> Refer to Port Number Port1.</param>
        /// <param name="vBUS_SENSE_VOLT_EN"> ExternalVbus = 0, TypeCVbus = 1,
        ///  <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// </param>
        /// <returns> 
        /// NoError = 0,
        /// UnknownError = -1,
        /// USBLinkError = -2,
        /// </returns>
        public int VBUS_Selection(PortID port, VBUS_SENSE_VOLT_EN vBUS_SENSE_VOLT_EN, string serialNumber)
        {

            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).VBUS_SENSE_VOLT_EN(port, vBUS_SENSE_VOLT_EN);
            }
            else
            {
                return -1;
            }

        }

        /// <summary>
        /// This function will return the Physical and Link errors- 
        /// Physical Error, Total Physical Error, Link Error, Total Link Error, Iteration Count, Present USB 2.0, Total USB 2.0 for the respective port.
        /// </summary>
        /// <param name="port">Refer to Port Number Port1.</param>
        ///  <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>PhysicalLinkError</returns>
        public PhysicalLinkError Get_Physical_Link_ErrorCount(PortID port, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).GetPhysicalLinkErrorCount(port);
            }
            else
            {
                return new PhysicalLinkError();
            }

        }

        /// <summary>
        /// This function will return the temperature status of the GRL-V-DPWR unit.
        /// </summary>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>HeatSinkValues - TemperatureValue1, TemperatureValue2, TemperatureValue3, Overall temperature and Temperature status</returns>       
        public HeatSinkValues Get_HeatSink_TemperatureValue(string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).GetHeatSinkTemprature();
            }
            else
            {
                return new HeatSinkValues();
            }

        }

        /// <summary>
        /// This function will return the Full Speed Swing and De-emphasis data from the USB register for the respective port.
        /// </summary>
        /// <param name="port">Refer to Port Number Port1.</param>
        ///  <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>
        /// SwingDeEmphasis - Tx_De_Emphasis_3p5dB, TX_De_Emphasis_6dB, Tx_Amp_Full_Swing, Tx_Amp_Low_Swing
        /// </returns>
        public SwingDeEmphasis USB_Swing_DeEmphasis_RegisterRead_3p0(PortID port, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).USBSwingDeEmphasisRegisterRead(port, TxConfigType.USB_3_0_PHY);
            }
            else
            {
                return new SwingDeEmphasis();
            }

        }

        /// <summary>
        /// This function will return the High Speed Swing and De-emphasis data from the USB register for the respective port.
        /// </summary>
        /// <param name="port">Refer to Port Number Port1.</param>
        ///  <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>
        /// SwingDeEmphasis - Tx_De_Emphasis_3p5dB, TX_De_Emphasis_6dB, Tx_Amp_Full_Swing, Tx_Amp_Low_Swing
        /// </returns>
        public SwingDeEmphasis USB_Swing_DeEmphasis_RegisterRead_2p0(PortID port, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).USBSwingDeEmphasisRegisterRead(port, TxConfigType.USB_2_0_PHY);
            }
            else
            {
                return new SwingDeEmphasis();
            }

        }

        /// <summary>
        /// This function will return the error of the latest API set to the GRL-V-DPWR unit.
        /// </summary>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>String : Detailed error description</returns>
        public string Get_API_Error_Status(string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).API_Error;
            }
            else
            {
                return "No Error";
            }

        }

        /// <summary>
        /// This function will return the PD Controller Event log data for the respective port and the result will be printed in the debug logger.
        /// </summary>
        /// <param name="port">Refer to Port Number Port1.</param>
        ///  <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>Boolean : True = Successful , False = Failed </returns>
        public bool Get_PDController_Log_Data(PortID port, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).Get_PDControllerLogData(port);
            }
            else
            {
                return false;

            }
        }

        /// <summary>
        /// This function will return any error that occurs on the GRL-V-DPWR unit.
        /// </summary>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>
        /// string - Error Notes
        /// </returns>
        public string Get_System_Error_Status(string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).Get_System_Error_Status();
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// This function will return an DUT firmware version in string format
        /// </summary>
        /// <param name="port">Refer to Port Number Port1.</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>string : firmware version </returns>
        public string Get_DUT_Firmware_Version(PortID port, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).Get_DUT_Firmware_Version(port);
            }
            else
            {
                return "";
            }

        }

        /// <summary>
        /// This function will get the cable capabilities, Before this function make sure cable tester mode is enabled in  V-DPWR controller by using this
        ///  ConfigureCableTester() function and then send Detach() and Attach() function.
        /// </summary>
        /// <param name="port">Refer to Port Number Port1.</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>CableData : cable capabilities</returns>
        public CableData Get_Cable_Capabilities(PortID port, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).GetCableCapabilities(port);
            }
            else
            {
                return new CableData();
            }

        }

        /// <summary>
        /// This function contains the calibration expiry date details for Tester-cards 
        /// </summary>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>CalibrationExpiryDetailsList</returns>
        public CalibrationExpiryDetailsList Calibration_Expire_Details(string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).Calibration_Expire_Details();
            }
            else
            {
                return new CalibrationExpiryDetailsList();
            }

        }

        /// <summary>
        /// This function will assert RA on active cc line and also commence Detach(), Attach(), and DecoderPDContractNegotationStatus(). 
        ///  For Verification send DecoderPDContractNegotationStatus() before CableFlip()
        /// </summary>
        /// <param name="port">Refer to Port Number Port1.</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>PD negotiation status- Requested PDO, Operating current, Maximum Operating current, 
        /// VBUS voltage and Communication Line. If there is no status update, it will return as Failed. </returns>
        public PDC_Status CableFlip(PortID port, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).CableFlip(port);
            }
            else
            {
                return new PDC_Status();
            }

        }

        /// <summary>
        /// This function will return the controller mode which is sink or source or DRP
        /// </summary>
        /// <param name="port">Refer to Port Number Port1.</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>Object of DataPowerRole </returns>
        public DataPowerRole Get_Controller_Mode(PortID port, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).Get_Data_Power_Role(port);
            }
            else
            {
                return new DataPowerRole();
            }
        }

        #endregion

        #region Set Commands

        /// <summary>
        /// This function will set the E-Load VBUS current based on the input values for the respective port.
        /// </summary>
        /// <param name="vbusCurrent">Double value VBUS current in amps </param>
        /// <param name="port"> Refer to Port Number Port1.</param>
        /// <param name="vbusEload">VBUS load on off</param>
        /// <param name="vBUSModeConfig">
        /// VBUS eLoad mode configuration
        /// CCMode = 0 Constant current Mode,
        /// VbusCRMode = 1 CR Mode - Constant resistance mode, 
        /// </param>   
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns> NoError = 0 , UnknownError = -1,</returns>
        public int SetVbusCurrent(double vbusCurrent, PortID port, VbusEload vbusEload, VBUSModeConfig vBUSModeConfig, string serialNumber)
        {

            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                this.vbusCurrent = vbusCurrent;
                this.port = port;
                this.vbusEload = vbusEload;
                this.vBUSModeConfig = vBUSModeConfig;
                return (gRLDeviceList.ControllerObject as VsCommandSets).SetCurrent(this.vbusCurrent, this.vconnCurrent, this.port, this.vbusEload,
               this.typeCVbusEload, this.vconnEload, this.cCline, this.vBUSModeConfig, this.vCONNModeConfig, EloadChannels.None);
            }
            else
            {
                return -1;
            }

        }

        /// <summary>
        /// This function will set the E-Load VCONN current based on the input values for the respective port.
        /// </summary>
        /// <param name="vconnCurrent">Double value VCONN current in amps</param>
        /// <param name="port"> Refer to Port Number Port1.</param>
        /// <param name="vconnEload">VCONN load on off</param>
        /// <param name="cCline"> CC line selection
        /// CC1 = 0,
        /// CC2 = 1
        /// </param>
        /// <param name="vCONNModeConfig">
        /// CCMode = 0,
        /// CRMode = 1,
        /// </param>
        /// <returns> Int  -1 - Error, 0 - No Error,</returns>
        public int SetVCONNCurrent(double vconnCurrent, PortID port, VconnEload vconnEload, CCline cCline, VCONNModeConfig vCONNModeConfig)
        {
            if (_vCommands == null)
                return -1;
            this.vconnCurrent = vconnCurrent;
            this.port = port;
            this.vconnEload = vconnEload;
            this.cCline = cCline;
            this.vCONNModeConfig = vCONNModeConfig;

            return _vCommands.SetCurrent(this.vbusCurrent, this.vconnCurrent, this.port, this.vbusEload,
            this.typeCVbusEload, this.vconnEload, this.cCline, this.vBUSModeConfig, this.vCONNModeConfig, EloadChannels.None);
        }
        /// <summary>
        /// This function will set the E-Load VCONN current based on the input values for the respective port.
        /// </summary>
        /// <param name="vconnCurrent">Double value VCONN current in amps</param>
        /// <param name="port"> Refer to Port Number Port1.</param>
        /// <param name="vconnEload">VCONN load on off</param>
        /// <param name="cCline"> CC line selection
        /// CC1 = 0,
        /// CC2 = 1
        /// </param>
        /// <param name="vCONNModeConfig">
        /// CCMode = 0,
        /// CRMode = 1,
        /// </param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns> Int  -1 - Error, 0 - No Error,</returns>
        public int SetVCONNCurrent(double vconnCurrent, PortID port, VconnEload vconnEload, CCline cCline, VCONNModeConfig vCONNModeConfig, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                this.vconnCurrent = vconnCurrent;
                this.port = port;
                this.vconnEload = vconnEload;
                this.cCline = cCline;
                this.vCONNModeConfig = vCONNModeConfig;
                return (gRLDeviceList.ControllerObject as VsCommandSets).SetCurrent(this.vbusCurrent, this.vconnCurrent, this.port, this.vbusEload,
              this.typeCVbusEload, this.vconnEload, this.cCline, this.vBUSModeConfig, this.vCONNModeConfig, EloadChannels.None);
            }
            else
            {
                return -1;
            }

        }

        /// <summary>
        /// This function will set the cable type for the respective port. 
        /// </summary>
        /// <param name="port">Refer to Port Number Port1.</param>
        /// <param name="cableType">
        ///  Special_Cable,
        ///  TypeC_Cable,
        /// </param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>Int : 0 = Successful , -1 = Failed </returns>
        public int CableSelection(PortID port, CableType cableType, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).CableSelection(port, cableType);
            }
            else
            {
                return -1;
            }

        }

        /// <summary>
        /// This function will return the loop-back Information and Link Speed that the DUT supplies to the tester for the respective port.
        /// </summary>
        /// <param name="port"> Refer to Port Number Port1.</param>
        /// <returns> VBUS voltage in volts</returns>
        public LoopBackInfo GetLoopBackInfo(PortID port)
        {
            if (_vCommands == null)
                return new LoopBackInfo();
            return _vCommands.GetLoopBackInfo(port);
        }
        /// <summary>
        /// This function will return the loop-back Information and Link Speed that the DUT supplies to the tester for the respective port.
        /// </summary>
        /// <param name="port"> Refer to Port Number Port1.</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns> VBUS voltage in volts</returns>
        public LoopBackInfo GetLoopBackInfo(PortID port, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).GetLoopBackInfo(port);
            }
            else
            {
                return new LoopBackInfo();
            }


        }

        /// <summary>
        /// This function will send the default PDO request messages during PD negotiation for the respective port.
        /// </summary>
        /// <param name="port"> Refer to Port Number Port1.</param>
        /// <returns>-1 = Unknown Error on request, 0 = No Error on request</returns>
        public int SetDefaultRequest(PortID port)
        {
            if (_vCommands == null)
                return -1;
            return _vCommands.PDMessageConfig(port, PDOIndex.PDO1, 0.1, 0.1);
        }

        /// <summary>
        /// This function will send the default PDO request messages during PD negotiation for the respective port.
        /// </summary>
        /// <param name="port"> Refer to Port Number Port1.</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>-1 = Unknown Error on request, 0 = No Error on request</returns>
        public int SetDefaultRequest(PortID port, string serialNumber)
        {

            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).PDMessageConfig(port, PDOIndex.PDO1, 0.1, 0.1);
            }
            else
            {
                return -1;
            }

        }


        /// <summary>
        /// This function will send the PDO Request message to perform voltage/current transition. 
        /// Example: If using PDO 4, then the command will be "RequestPDO(PortID.Port1, PDO4, 1.5 (Maximum operating current), 1.5 (Operating Current))"
        /// </summary>
        /// <param name="port"> Refer to Port Number Port1.</param>
        /// <param name="pDOIndex">PDO index : PDO1,PDO2,...,PDO7</param>
        /// <param name="maxCurrent_Amps">Double: Maximum operating current</param>
        /// <param name="oppCurrent_Amps">Operating current </param>     
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>
        /// -1 = Unknown Error,
        /// 0 = No Error
        /// </returns>
        public int RequestPDO(PortID port, PDOIndex pDOIndex, double maxCurrent_Amps, double oppCurrent_Amps, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).PDMessageConfig(port, pDOIndex, maxCurrent_Amps, oppCurrent_Amps);
            }
            else
            {
                return -1;
            }
        }
        /// <summary>
        /// This function will send the PPS APDO Request message to perform voltage/current transition for the respective port.
        ///
        /// Example: If using PDO 4, then the command will be "RequestAPDO(PortID.Port1, PDO4, 0 (Maximum operating current), 5 (Voltage))"
        /// </summary>
        /// <param name="port"> Refer to Port Number Port1.</param>
        /// <param name="pDOIndex">PDO index : PDO1,PDO2,...,PDO7</param>
        /// <param name="oppCurrent_Amps">Maximum operating current</param>
        /// <param name="voltage_Volt">Voltage</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>
        /// -1 = Unknown Error,
        /// 0 = No Error
        /// </returns>
        public int RequestAPDO(PortID port, PDOIndex pDOIndex, double oppCurrent_Amps, double voltage_Volt, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).PDMessageConfig(port, pDOIndex, voltage_Volt, oppCurrent_Amps);
            }
            else
            {
                return -1;
            }

        }


        /// <summary>
        /// This function will return the DUT's Attach mode status for the respective port.        
        /// </summary>
        /// <param name="port"> Refer to Port Number Port1.</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>
        /// 0 = DUT is attached successfully, 
        /// -1 = DUT is not attached,
        /// </returns>
        public int Attach(PortID port, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).Attach(port);
            }
            else
            {
                return -1;
            }


        }
        /// <summary>
        /// This function will start / stop updating the system LED status, VBUS, VCONN and temperature data.
        /// </summary>
        /// <param name="startStops">StartStop.Start / StartStop.Stop</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>
        /// 0 =  Successful, 
        /// -1 = Failed
        /// </returns>
        public int PollingTimerControl(StartStop startStops, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {

                return (gRLDeviceList.ControllerObject as VsCommandSets).InitiatePollingTimer(startStops);
            }
            else
            {
                return -1;
            }
        }
        /// <summary>
        /// This function will detach the DUT from the respective port. 
        /// </summary>
        /// <param name="port"> Refer to Port Number Port1.</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns> 
        /// 0 = Detached successfully, 
        /// -1 = DUT is not Detached
        /// </returns>
        public int Detach(PortID port, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {

                return (gRLDeviceList.ControllerObject as VsCommandSets).Detach(port);
            }
            else
            {
                return -1;
            }
        }


        /// <summary>
        /// This function will Assert Ra on CC1 or CC2, or Assert Ra on a active CC line for the respective port.
        /// </summary>
        /// <param name="port"> Refer to Port Number Port1.</param>
        /// <param name="ra_Selection">
        ///  RaDisable = 0x00,
        /// RaAssert_CC1 = 0x01,
        /// RaAssert_CC2 = 0x02,
        /// RaAssert_ActiveCC = 0x03,
        /// </param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>0 = Ra Selection is successful,
        /// -1 = Ra Selection has failed
        /// </returns>
        public int RaSelection(PortID port, Ra_Selection ra_Selection, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).RaSelection(port, ra_Selection);
            }
            else
            {
                return -1;
            }

        }


        /// <summary>
        /// This function will indicate to which controller we are communicating 
        /// </summary>
        /// <param name="port">Refer to Port Number Port1.</param>
        /// <param name="controlSwitch">LEDControl.On , LEDControl.Off </param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>
        /// 0 = Successful  , 
        /// -1 = Failed
        /// </returns>
        public int AppCommandLEDCheck(PortID port, LEDControl controlSwitch, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {

                return (gRLDeviceList.ControllerObject as VsCommandSets).RackReferenceLEDControl(port, controlSwitch);
            }
            else
            {
                return -1;
            }

        }
        /// <summary>
        /// This function will initiate loop-back data transfer with the respective tester card for the respective port. 
        /// This function should be called before starting data transaction.
        /// </summary>
        /// <param name="port">Refer to Port Number Port1.</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>
        /// 0 = Successful  , 
        /// -1 = Failed
        /// </returns>
        public int Start_LoopBack(PortID port, string serialNumber)
        {

            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {

                return (gRLDeviceList.ControllerObject as VsCommandSets).USBLoopBackCommands(port, LoopbackCommands.Start);
            }
            else
            {
                return -1;
            }
        }
        /// <summary>
        /// This function will terminate loop-back data transfer with the respective tester card for the respective port. 
        /// This function should be called after loop-back data testing is completed.
        /// </summary>
        /// <param name="port">Refer to Port Number Port1.</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>
        /// 0 = Successful, 
        /// -1 = Failed
        /// </returns>
        public int Stop_LoopBack(PortID port, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {

                return (gRLDeviceList.ControllerObject as VsCommandSets).USBLoopBackCommands(port, LoopbackCommands.Stop);

            }
            else
            {
                return -1;
            }

        }

        /// <summary>
        /// This function will disconnect and stop the device enumeration for the respective port. This function will apply mostly when using the USB Type-A to Type-C cable.
        /// </summary>
        /// <param name="port">Refer to Port Number Port1.</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>
        /// 0 = Successful  , 
        /// -1 = Failed
        /// </returns>
        public int USB_Soft_Disconnect(PortID port, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).USBLoopBackCommands(port, LoopbackCommands.USB_Soft_Disconnect);

            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// This function will connect and initiate the device enumeration for the respective port. This function will apply mostly when using the USB Type-A to Type-C cable.
        /// </summary>
        /// <param name="port">Refer to Port Number Port1.</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>
        /// 0 = Successful  , 
        /// -1 = Failed
        /// </returns>
        public int USB_Soft_Connect(PortID port, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).USBLoopBackCommands(port, LoopbackCommands.USB_Soft_Connect);
            }
            else
            {
                return -1;
            }

        }

        /// <summary>
        /// This function will revert the device operating mode from USB 3.0 to USB 2.0 for the respective port.
        /// </summary>
        /// <param name="port">Refer to Port Number Port1.</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>
        /// 0 = Successful  , 
        /// -1 = Failed
        /// </returns>
        public int USB_DeviceMode_2p0(PortID port, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).USBLoopBackCommands(port, LoopbackCommands.USB_2p0_FallBack);
            }
            else
            {
                return -1;
            }

        }

        /// <summary>
        /// This function will set the device operating mode to USB 3.0 for the respective port.
        /// </summary>
        /// <param name="port">Refer to Port Number Port1.</param>
        ///  <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>
        /// 0 = Successful  , 
        /// -1 = Failed
        /// </returns>
        public int USB_DeviceMode_3p0(PortID port, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).USBLoopBackCommands(port, LoopbackCommands.USB_3p0_FallBack);
            }
            else
            {
                return -1;
            }

        }


        /// <summary>
        /// This function will reset the physical and link error count for the respective port.
        /// </summary>
        /// <param name="port">Refer to Port Number Port1.</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>
        /// 0 = Successful  , 
        /// -1 = Failed
        /// </returns>
        public int Reset_ErrorCount(PortID port, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).USBLoopBackCommands(port, LoopbackCommands.Reset_Error_Count);
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// This function will set the USB Swing and De-Emphasis for the respective port.
        /// </summary>
        /// <param name="port">Refer to Port Number Port1.</param>
        /// <param name="uSBSpeed">USB_3_0 = 0,
        /// USB_2_0 = 1</param>
        /// <param name="swingType"> NoConfiguration = 0,
        /// FullSwing = 1,
        /// LowSwing = 2,</param>
        /// <param name="swingValue">int value</param>
        /// <param name="deEmphasisType"> Noconfiguration = 0,
        /// _3_5_db = 1,
        /// _6_db = 2</param>
        /// <param name="deEmphasisValue">int value</param>
        /// <param name="preEmphasisType">
        ///  Noconfiguration = 0,
        /// Enable = 1,
        /// Disable = 2</param>
        /// <param name="preEmphasisValue"> Enable = 1,
        /// Disable = 2</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>Boolean : True = Successful , False = Failed </returns>
        public bool Configure_USB_Swing_DeEmphasis(PortID port, USBSpeed uSBSpeed, SwingType swingType, int swingValue,
            DeEmphasisType deEmphasisType, int deEmphasisValue, PreEmphasisType preEmphasisType, PreEmphasisValue preEmphasisValue, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).ConfigureUSBswing_deEmphasis(port, uSBSpeed, swingType, swingValue,
            deEmphasisType, deEmphasisValue, preEmphasisType, preEmphasisValue);
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// This function will switch the VCONN to CC1/CC2 or to a active CC line or disable VCONN for the respective port.
        /// </summary>
        /// <param name="port">Refer to Port Number Port1.</param>
        /// <param name="vCONN_Load_Switch">
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        ///  VCONN_Load_Disable ,
        /// VCONN_Load_CC1 ,
        /// VCONN_Load_CC2 ,
        /// VCONN_Load_ActiveCC ,</param>
        /// <returns>
        /// 0 = Successful  , 
        /// -1 = Failed
        /// </returns>

        public int VCONNLoadSwitch(PortID port, VCONN_Load_Switch vCONN_Load_Switch, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).VCONNLoadSwitch(port, vCONN_Load_Switch);
            }
            else
            {
                return -1;
            }

        }
        /// <summary>
        /// This function will set the temperature limit for the GRL-V-DPWR unit.
        /// Note: Any value above 60 degrees and below 90 degrees is valid.
        /// </summary>
        /// <param name="temperatureLimit">int Temperature value</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>Boolean : True = Successful , False = Failed </returns>
        public bool Set_HeatSink_Temperature_Limit(int temperatureLimit, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).SetTemperatureLimit(temperatureLimit);
            }
            else
            {
                return false;
            }

        }


        /// <summary>
        /// This function will enable or disable the E-Load automatically based on the PDO for the respective port.
        /// </summary>
        /// <param name="port">Refer to Port Number Port1.</param>
        /// <param name="autoEload">Enable or disable the function</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>Boolean : True = Successful , False = Failed </returns>
        public bool Set_Auto_Eload_On_PDO(PortID port, Command autoEload, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).SetEloadAutomaticaly(port, autoEload);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// This function will clear the PD controller event log data for the respective port.
        /// </summary>
        /// <param name="port">Refer to Port Number Port1.</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>

        /// <returns>Boolean : True = Successful , False = Failed </returns>
        public bool Clear_PD_Controller_Event_Log_Data(PortID port, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).ClearPDControllerLogData(port);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// This function will configure the vendor define message to emulate GRL V-DPWR as cable
        /// </summary>
        /// <param name="port">Refer to Port Number Port1.</param>
        /// <param name="msg">new VendorDefinedMessage()</param>
        /// <param name="rESPONSE_TYPE">
        ///  IGNORE = 0,
        ///  ACK = 1,
        ///  NAK = 2</param>
        /// <param name="serialNumber">IF V-DPWR then controller serial number if V-DPWR then IP Aaddress</param>
        /// <returns>Boolean : True = Successful , False = Failed </returns>
        public bool Configure_VDM_Response(PortID port, VendorDefinedMessage msg, RESPONSE_TYPE rESPONSE_TYPE, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).ConfigureVDMResponse(port, msg, rESPONSE_TYPE);
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// This function will be used to enable CCLines while performing port verification
        /// NOTE: Once the port verification is done make sure this function is disabled
        /// </summary>
        /// <param name="port">Refer to Port Number Port1.</param>
        /// <param name="portVerifyEnableDisable">
        /// Disable = 0,
        /// Enable = 1</param>
        /// <param name="cCline"> 
        /// CC1 = 0,
        /// CC2 = 1,</param>
        /// <returns>
        /// 0 = Successful, 
        /// -1 = Failed
        /// <param name="serialNumber">IF V-DPWR then controller serial number if V-DPWR then IP Aaddress</param>
        /// </returns>
        public int Port_Verification_On_CCLine(PortID port, PortVerifyEnableDisable portVerifyEnableDisable, CCline cCline, PowerRoleType powerRoleType, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).PortVerification(port, portVerifyEnableDisable, cCline, powerRoleType);
            }
            else
            {
                return -1;
            }

        }

        /// <summary>
        /// This function will configure V-DPWR controller as cable tester.
        ///  Note : After sending this command please send Detach() and then Attach() function and then Get_Cable_capabilities() to read the cable capabilities.
        /// </summary>
        /// <param name="port">Refer to Port Number Port1.</param>
        /// <param name="command"> Disable = 0,
        /// Enable = 1,</param>
        /// <param name="serialNumber">IF V-DPWR then controller serial number if V-DPWR then IP Aaddress</param>
        /// <returns>boolean : True = Successful , False = Failed </returns>
        public bool ConfigureCableTester(PortID port, Command command, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).ConfigureCableTester(port, command);
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// This function will set the controller mode to Source , Sink , or DRP 
        /// </summary>
        /// <param name="controllerMode">ControllerMode : Sink , Source , DRP</param>
        /// <param name="serialNumber">IF V-DPWR then controller serial number if V-DPWR then IP Aaddress</param>
        /// <returns>Boolean : True if the command  is successful else false</returns>
        public bool Set_Controller_Mode(ControllerMode controllerMode, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).Set_Controller_Mode(controllerMode);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// This function will set the PPS voltage
        /// </summary>
        /// <param name="Vbus_in_Volt">VBUS voltage in Volt</param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>Boolean : True if the command  is successful else false</returns>
        public bool Set_Voltage_Pps(double Vbus_in_Volt, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).Set_Voltage_Pps(Vbus_in_Volt);
            }
            else
            {

                return false;
            }

        }

        /// <summary>
        /// This function will allow the controller to set the source caps
        /// </summary>
        /// <param name="port">Refer to Port Number Port1.</param>
        /// <param name="objSourceCapability">ConfigureSourceCapability </param>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <returns>boolean : true for successful else false</returns>
        public bool SetSourceCapability(PortID port, ConfigureSourceCapability objSourceCapability, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).SetSourceCapability(port, objSourceCapability);
            }
            else
            {
                return false;

            }

        }

        /// <summary>
        /// This function will allow the controller to set the source caps
        /// </summary>
        /// <param name="port">Refer to Port Number Port1.</param>
        /// <param name="objSourceCapability">ConfigureSourceCapability </param>
        /// <param name="serialNumber">IF V-DPWR then controller serial number if V-DPWR then IP Aaddress</param>
        /// <returns>boolean : true for successful else false</returns>
        public bool SetSinkCapability(PortID port, ConfigureSinkCapability objSourceCapability, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).SetSinkCapability(port, objSourceCapability);
            }
            else
            {
                return false;
            }

        }



        #region QC 2.0 / 3.0 


        /// <summary>
        /// This function will enable or disable PD / QC Sink Mode. After enable use Set_VBUS_Voltage_QC_Mode_2p0() API for QC 2.0 and Set_VBUS_Voltage_QC_Mode_3p0() API for QC 3.0
        /// </summary>
        /// <param name="port">Refer to Port Number Port1.</param>
        /// <param name="pD_QC_SwtichModes">PD or QC mode switch</param>
        /// <param name="qC_ModeSwitch">QC mode 2.0 / 3.0 Note If PD is selected then this parameter should be QC_ModeSwitch.None</param>
        ///  <param name = "serialNumber" > IF V - UP then controller serial number if V - DPWR then IP Aaddress </ param >
        /// <returns>boolean : true for successful else false</returns>
        public bool PD_QC_SwtichMode(PD_QC_Mode pD_QC_SwtichModes, string serialNumber, QC_ModeSwitch qC_ModeSwitch = QC_ModeSwitch.None)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).PD_QC_SwtichMode(PortID.Port1, ControllerMode.Sink, pD_QC_SwtichModes, qC_ModeSwitch);
            }
            return false;
        }

        /// <summary>
        /// This function will set voltage on VBUS line in QC 2.0 mode and note before this API send PD_QC_SwtichMode() ex: PD_QC_SwtichMode(PortID.Port1, PD_QC_ModeSwitch.QC, QC_ModeSwitch.QC2p0)
        /// </summary>
        /// <param name="port">Refer to Port Number Port1.</param>
        /// <param name="qc2P0_VBUS">  _5V = 5 Volts on VBUS ,
        /// _9V = 9 Volts on VBUS , 
        /// _12V = 12 Volts on VBUS , 
        /// _20V = 20  Volts on VBUS ,</param>
        /// <param name = "serialNumber" > IF V - UP then controller serial number if V - DPWR then IP Aaddress </ param >
        /// <returns>boolean : true for successful else false</returns>
        public bool Set_VBUS_Voltage_QC_Mode_2p0(Qc_VBUS qc_VBUS, string serialNumber)
        {

            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).Set_VBUS_Voltage_QC_Mode_2p0(PortID.Port1, qc_VBUS);
            }
            return false;

        }

        /// <summary>
        /// This function will set voltage on VBUS line in QC 3.0 mode and note before this API send PD_QC_SwtichMode() ex: PD_QC_SwtichMode(PortID.Port1, PD_QC_ModeSwitch.QC, QC_ModeSwitch.QC3p0)
        /// </summary>
        /// <param name="port">Refer to Port Number Port1.</param>
        /// <param name="voltage_in_V">double Voltage value in V (Limit from 3.7V to 20V)</param>
        /// <param name = "serialNumber" > IF V - UP then controller serial number if V - DPWR then IP Aaddress </ param >
        /// <returns>boolean : true for successful else false</returns>
        public bool Set_VBUS_Voltage_QC_Mode_3p0(double voltage_in_V, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).Set_VBUS_Voltage_QC_Mode_3p0(PortID.Port1, voltage_in_V);
            }
            return false;
        }

        /// <summary>
        /// This function will enable or disable PD / QC Source Mode. After enable use Source_PD_QCSwtich() API for QC 2.0 
        /// </summary>
        /// <param name="sourcePD_QC_ModeSWitch"></param>
        /// <param name="serialNumber"></param>
        /// <returns></returns>
        public bool Source_PD_QC_SwitchMode(PD_QC_Mode sourcePD_QC_ModeSWitch, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                QC_ModeSwitch qC_ModeSwitch = QC_ModeSwitch.None;
                if (sourcePD_QC_ModeSWitch == PD_QC_Mode.QC)
                    qC_ModeSwitch = QC_ModeSwitch.QC2p0;

                return (gRLDeviceList.ControllerObject as VsCommandSets).PD_QC_SwtichMode(PortID.Port1, ControllerMode.Source, sourcePD_QC_ModeSWitch, qC_ModeSwitch);
            }
            return false;
        }

        /// <summary>
        /// This function will set the value for OCP , value should be in percentage from 0 - 100, anything above not acceptable return false<
        /// </summary>
        /// <param name="ocpValueInPercentage">value in percentage from 0 - 100, anything above not acceptable return false</param>
        /// <param name="oCP_Switch">OCP swith enable or disable this fucntion</param>
        /// <param name = "serialNumber" > IF V - UP then controller serial number if V - DPWR then IP Aaddress "192.168.0.4"</ param >
        /// <returns></returns>
        public bool Set_OCP_Trigger_Value(uint ocpValueInPercentage, OCP_Switch oCP_Switch, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).Set_OCP_Trigger_Value(PortID.Port1, ocpValueInPercentage, oCP_Switch);
            }
            return false;
        }
        #endregion

        #endregion


        #region Function Programing Mode


        /// <summary>
        /// This function will act as a switch for all tester cards for the respective port.
        /// </summary>
        /// <param name="port"> Refer to Port Number Port1.</param>
        /// <param name="switchOnOff">Switch On/Off</param>
        /// <param name = "serialNumber" > IP address of the controller ex - 192.168.0.4</param>
        /// <returns> 
        /// NoError = 0,
        /// UnknownError = -1,
        /// USBLinkError = -2,
        /// </returns>
        public int TesterCard_PowerControlCommand(PortID port, PowerSwitch switchOnOff, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).PowerControlCommand(port, switchOnOff);
            }
            else
            {
                return -1;
            }

        }

        /// <summary>
        /// This function will reset the GRL-V-DPWR controller.
        /// </summary>
        /// <param name = "serialNumber" > IP address of the controller ex - 192.168.0.4</param>
        /// <returns>Boolean : True = Successful , False = Failed </returns>
        public bool Controller_Reset(string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).ControllerReset();
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// This function will set the tester card to Sink mode for the respective port.
        /// </summary>
        /// <param name="port"> Refer to Port Number Port1.</param>
        /// <param name = "serialNumber" > IP address of the controller ex - 192.168.0.4</param>
        /// <returns>
        /// 0 = Successful, 
        /// -1 = Failed
        /// </returns>
        public int SinkConfigure(PortID port, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).SinkConfigure(port);
            }
            else
            {
                return -1;
            }

        }
        #endregion

        //#region Firmware Update 


        ///// <summary>
        ///// This function will update the tester card firmware for the respective port or all ports at a time.
        ///// </summary>
        ///// <param name="fileName"> string : File location</param>
        ///// <param name="port">Refer to Port Number Port1.</param>
        ///// <param name = "serialNumber" > IP address of the controller ex - 192.168.0.4</param>
        ///// <returns>Boolean : True = Successful , False = Failed </returns>
        //public bool Update_TesterCard_Firmware(string serialNumber, string fileName, PortID port = PortID.PortAll)
        //{
        //    if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
        //    {
        //        return (gRLDeviceList.FirmwareUpdate as Firmware).Update(fileName, port, FirmwareName.TesterCard);
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        ///// <summary>
        ///// This function will terminate firmware update for the tester card and PD controller ONLY.
        ///// </summary>
        ///// <param name = "serialNumber" > IP address of the controller ex - 192.168.0.4</param>
        //public bool Stop_FirmwareUpdate(string serialNumber)
        //{
        //    if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
        //    {
        //        return (gRLDeviceList.FirmwareUpdate as Firmware).Stop();
        //    }
        //    else
        //    {
        //        return firmware_CMCore.Stop();
        //    }
        //}

        ///// <summary>
        ///// This function will update the controller card firmware.
        ///// </summary>
        ///// <param name="fileName"> string : File location</param>
        /////   /// <param name = "serialNumber" > IP address of the controller ex - 192.168.0.4</param>
        ///// <returns>Boolean : True = Successful , False = Failed </returns>
        //public bool Update_ControlCard_Firmware(string fileName, string serialNumber, string bootLoaderFileName = "")
        //{
        //    if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
        //    {
        //        return (gRLDeviceList.FirmwareUpdate as Firmware).Update(fileName, PortID.NONE, FirmwareName.Controller, bootLoaderFileName);
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        ///// <summary>
        ///// This function will update the second stage boot loader firmware on one tester card port at a time ONLY.
        ///// </summary>
        ///// <param name="fileName"> string : File location</param>
        ///// <param name="port">Refer to Port Number Port1.</param>
        /////   /// <param name = "serialNumber" > IP address of the controller ex - 192.168.0.4</param>
        ///// <returns>Boolean : True = Successful , False = Failed </returns>
        //public bool Update_SecondStageBootLoader_Firmware(string fileName, PortID port, string serialNumber)
        //{
        //    if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
        //    {
        //        return (gRLDeviceList.FirmwareUpdate as Firmware).Update(fileName, port, FirmwareName.SSBL);
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        ///// <summary>
        ///// This function will update the PD controller firmware on one port or all ports at a time.       
        ///// </summary>
        ///// <param name="fileName"> string : File location</param>
        ///// <param name="port">Refer to Port Number Port1.</param>
        ///// <param name = "serialNumber" > IP address of the controller ex - 192.168.0.4</param>
        ///// <returns>Boolean : True = Successful , False = Failed </returns>
        //public bool Update_PD_Controller_Firmware(string fileName, PortID port = PortID.PortAll, string serialNumber = "")
        //{
        //    if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
        //    {
        //        return (gRLDeviceList.FirmwareUpdate as Firmware).Update(fileName, port, FirmwareName.PD_Controller);
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        ///// <summary>
        ///// This function will update the CC line firmware for the respective port or all ports at a time.
        ///// </summary>
        ///// <param name="fileName"> string : File location</param>
        ///// <param name="port">Refer to Port Number Port1.</param>
        ///// <param name = "serialNumber" > IP address of the controller ex - 192.168.0.4</param>
        ///// <returns>Boolean : True = Successful , False = Failed </returns>
        //public bool Update_USB_C_Provider_Firmware(string serialNumber, string fileName, PortID port = PortID.PortAll)
        //{

        //    if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
        //    {
        //        return (gRLDeviceList.FirmwareUpdate as Firmware).Update(fileName, port, FirmwareName.USB_C_Provider);
        //    }
        //    else
        //    {
        //        return false;
        //    }

        //}

        ///// <summary>
        ///// This function will update the E-Load firmware.
        ///// </summary>
        ///// <param name="fileName"> string : File location</param>
        ///// <param name="comPort">Refer to Port Number Port1.</param>
        ///// <param name = "serialNumber" > IP address of the controller ex - 192.168.0.4</param>
        ///// <returns>Boolean : True = Successful , False = Failed </returns>
        //public bool Update_Eload_Firmware(string comPort, string fileName)
        //{
        //    return new EloadFirmware().Update(comPort, fileName);
        //}

        ///// <summary>
        ///// This will call the Firmware Update status in percentage.
        ///// </summary>
        //public uint Get_Firmware_ProgressStatus(string serialNumber)
        //{

        //    if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
        //    {
        //        return (gRLDeviceList.FirmwareUpdate as Firmware).ProgressStatus();
        //    }
        //    else
        //    {
        //        if (firmware_CMCore != null)
        //        {

        //            return firmware_CMCore.ProgressStatus();
        //        }
        //        else
        //        {
        //            return 0;
        //        }
        //    }

        //}

        ///// <summary>
        ///// Internl purpose only. Please do not access this API
        ///// </summary>
        //public FirmwareUpdateIndication Get_Firmware_Status(string serialNumber)
        //{
        //    if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
        //    {
        //        return (gRLDeviceList.FirmwareUpdate as Firmware).IsFirmwareUpdating;
        //    }
        //    else
        //    {
        //        if (firmware_CMCore != null)
        //        {

        //            return firmware_CMCore.IsFirmwareUpdating;
        //        }
        //        else
        //        {
        //            return FirmwareUpdateIndication.None; ;
        //        }
        //    }
        //}

        ///// <summary>
        /////  This function will update PPS firmware.
        ///// </summary>
        ///// <param name="fileName">File with folder path</param>
        ///// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        ///// <returns>BOOLEAN: True = Successful,false = Failed</returns>
        //public bool Update_PPS_Firmware(string fileName, string serialNumber)
        //{

        //    if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
        //    {
        //        return (gRLDeviceList.FirmwareUpdate as Firmware).Update(fileName, port, FirmwareName.PPS);
        //    }
        //    else
        //    {
        //        return false;
        //    }

        //}

        ///// <summary>
        ///// This function will update connectivity manager firmware
        ///// </summary>
        ///// <param name="fileName">file name with folder path </param>
        ///// <param name="comPort">Comport number</param>
        ///// <param name="baudRate">Baurd rate</param>
        ///// <returns>If then firmware update is successfull the return true else false</returns>
        //public bool Update_Connectivity_Manager_Firmware(string fileName, string comPort, int baudRate)
        //{
        //    _vCommands = new VsCommandSets();
        //    firmware_CMCore = new Firmware(_vCommands);
        //    return firmware_CMCore.Update(fileName, port, FirmwareName.Connectivity_Manager, "", comPort, baudRate);
        //}



        //#endregion

        #region loop-back API

        /// <summary>
        /// This function will read data from the Loop-back device.
        /// <param name="dataBuffer">Data buffer</param>
        /// <param name="byteCount">Buffer count</param>
        /// <param name = "serialNumber" > USB serial number. Get  USBAddress from GetGRLDeviceList ex - 202200223-01</param>
        /// <returns>Boolean : True = Successful , False = Failed </returns>
        /// </summary>
        public bool Read(ref byte[] dataBuffer, ref int byteCount, uint timeOut, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).Read(ref dataBuffer, ref byteCount, timeOut);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// This function will write data to the Loop-back device
        /// <param name="buffer">Data buffer</param>
        /// <param name="byteCount">Data buffer count</param>
        /// <param name = "serialNumber" > USB serial number. Get  USBAddress from GetGRLDeviceList ex - 202200223-01</param>
        /// <returns>Boolean : True = Successful , False = Failed </returns>
        /// </summary>
        public bool Write(ref byte[] buffer, ref int byteCount, uint timeOut, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).Write(ref buffer, ref byteCount, timeOut);
            }
            else
            {
                return false;
            }

        }

        #endregion

        #endregion

        #region Internal Commands

        /// <summary>
        /// Please do not use this API, Not for customer usage
        /// </summary>
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <param name="vbusCurrent"></param>
        /// <param name="port"></param>
        /// <param name="vbusEload"></param>
        /// <param name="vBUSModeConfig"></param>
        /// <param name="eloadChannels"></param>
        /// <returns></returns>        
        public int SetVbusCurrent_Internal(string serialNumber, double vbusCurrent, PortID port, VbusEload vbusEload, VBUSModeConfig vBUSModeConfig, EloadChannels eloadChannels = EloadChannels.None)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).SetCurrent(vbusCurrent, 0, port, vbusEload, TypeCVbusEload.Off, VconnEload.Off, CCline.CC1, vBUSModeConfig, VCONNModeConfig.CCMode, eloadChannels);
            }
            else
            {
                return -1;
            }

        }

        /// <summary>
        /// Please do not use this API, Not for customer usage
        /// <param name="serialNumber">IP address of the controller ex - 192.168.0.4</param>
        /// <param name="vconnCurrent"></param>
        /// <param name="port"></param>
        /// <param name="vconnEload"></param>
        /// <param name="cCline"></param>
        /// <param name="vCONNModeConfig"></param>
        /// <param name="eloadChannels"></param>
        /// <returns></returns>
        /// </summary>
        public int SetVCONNCurrent_Internal(string serialNumber, double vconnCurrent, PortID port, VconnEload vconnEload, CCline cCline, VCONNModeConfig vCONNModeConfig, EloadChannels eloadChannels = EloadChannels.None)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).SetCurrent(0, vconnCurrent, port, VbusEload.Off, TypeCVbusEload.Off, vconnEload, cCline, VBUSModeConfig.CCMode, vCONNModeConfig, eloadChannels);
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Please do not use this API, Not for customer usage.
        /// </summary>
        /// <param name="card">Card.Control = 0x01, Card.Tester = 0x02,</param>
        /// <param name="port">Refer to Port Number Port1.</param>
        /// <param name="startAddress">start address</param>
        /// <param name="calibrationSheet">calibration sheet ex: CalibrationSheetExternal.TCRev1Sheet</param>  
        ///   /// <param name = "serialNumber" > IP address of the controller ex - 192.168.0.4</param>
        /// <returns>Boolean : True = Successful , False = Failed </returns>
        public bool Decode(string serialNumber, Card card, PortID port, int startAddress = 00, string calibrationSheet = "")
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                _vCommands = gRLDeviceList.ControllerObject as VsCommandSets;
                calibReadDecoder = new CalibReadDecoder(_vCommands);

                return calibReadDecoder.Decode(card, port, startAddress, calibrationSheet);
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// Please do not use this API, Not for customer usage.
        /// </summary>
        /// <param name="fileName">file path</param>
        /// <param name="startAddress">start address</param>
        /// <param name="inputData"> InputData.File / InputData.String </param>
        /// <param name="card">Card.Control = 0x01, Card.Tester = 0x02,</param>s
        /// <param name="port">Refer to Port Number Port1.</param>  
        /// <param name = "serialNumber" > IP address of the controller ex - 192.168.0.4</param>
        /// <returns></returns>
        public int Decode(string fileName, int startAddress, InputData inputData, Card card, PortID port, string serialNumber)
        {

            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                _vCommands = gRLDeviceList.ControllerObject as VsCommandSets;
                _calibWrite = new CalibrationWrite(_vCommands);
                return _calibWrite.Decode(fileName, startAddress, inputData, card, port);
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Please do not use this API, Not for customer usage
        /// </summary>
        /// <param name="port"></param>
        /// <param name = "serialNumber" > IP address of the controller ex - 192.168.0.4</param>
        /// <returns></returns>
        public int EloadOff(PortID port, string serialNumber)
        {
            if (_vCommands == null)
                return -1;

            this.vbusCurrent = 0;
            this.vconnCurrent = 0;
            this.port = port;
            this.vbusEload = VbusEload.Off;
            this.typeCVbusEload = TypeCVbusEload.Off;
            this.vconnEload = VconnEload.Off;
            this.cCline = CCline.CC1;
            this.vBUSModeConfig = VBUSModeConfig.CCMode;
            this.vCONNModeConfig = VCONNModeConfig.CCMode;

            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).SetCurrent(0, 0, this.port, this.vbusEload, this.typeCVbusEload, this.vconnEload, CCline.CC1, VBUSModeConfig.CCMode, VCONNModeConfig.CCMode, EloadChannels.None);
            }
            else
            {
                return -1;
            }


        }
        /// <summary>
        /// Please do not use this API, Not for customer usage
        /// </summary>
        /// <param name="stepIncDec"></param>
        /// <param name="port">Refer to Port Number Port1.</param>
        /// <param name="vbusEload"></param>
        /// <param name="vBUSModeConfig"></param>
        /// <param name="eloadChannels"></param>
        /// <returns></returns>
        public int VbusCurrentStepIncDec(string serialNumber, StepIncDec stepIncDec, PortID port, VbusEload vbusEload, VBUSModeConfig vBUSModeConfig, EloadChannels eloadChannels = EloadChannels.None)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).SetCurrent(StepSelection.VBUS, stepIncDec, port, vbusEload, TypeCVbusEload.Off, VconnEload.Off, CCline.CC1, vBUSModeConfig, VCONNModeConfig.CCMode, eloadChannels, VbusVconnSelection.VBUS);
            }
            else
            {
                return -1;
            }

        }

        /// <summary>
        /// Please do not use this API, Not for customer usage
        /// </summary>
        /// <param name="stepIncDec"></param>
        /// <param name="port">Refer to Port Number Port1.</param>
        /// <param name="vbusEload"></param>
        /// <param name="vBUSModeConfig"></param>
        /// <param name="eloadChannels"></param>
        /// <param name = "serialNumber" > IP address of the controller ex - 192.168.0.4</param>
        /// <returns></returns>
        public int VbusCurrentStepIncDec(string serialNumber, StepIncDec stepIncDec, PortID port, VbusEload vbusEload, VBUSModeConfig vBUSModeConfig, List<EloadChannels> eloadChannels = null)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).SetCurrent(StepSelection.VBUS, stepIncDec, port, vbusEload, TypeCVbusEload.Off, VconnEload.Off, CCline.CC1, vBUSModeConfig, VCONNModeConfig.CCMode, eloadChannels, VbusVconnSelection.VBUS);
            }
            else
            {
                return -1;
            }


        }

        /// <summary>
        /// Please do not use this API, Not for customer usage
        /// </summary>
        /// <param name="stepIncDec"></param>
        /// <param name="port">Refer to Port Number Port1.</param>
        /// <param name="vconnEload"></param>
        /// <param name="cCline"></param>
        /// <param name="vCONNModeConfig"></param>
        /// <param name="eloadChannels"></param>
        /// <param name = "serialNumber" > IP address of the controller ex - 192.168.0.4</param>
        /// <returns></returns>
        public int VconnCurrentStepIncDec(string serialNumber, StepIncDec stepIncDec, PortID port, VconnEload vconnEload, CCline cCline, VCONNModeConfig vCONNModeConfig, EloadChannels eloadChannels = EloadChannels.None)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).SetCurrent(StepSelection.VCONN, stepIncDec, port, VbusEload.Off, TypeCVbusEload.Off, vconnEload, cCline, VBUSModeConfig.CCMode, vCONNModeConfig, eloadChannels, VbusVconnSelection.VCONN);
            }
            else
            {
                return -1;

            }
        }

        /// <summary>
        /// Please do not use this API, Not for customer usage
        /// </summary>
        /// <param name="stepIncDec"></param>
        /// <param name="port">Refer to Port Number Port1.</param>
        /// <param name="vconnEload"></param>
        /// <param name="cCline"></param>
        /// <param name="vCONNModeConfig"></param>
        /// <param name="eloadChannels"></param>
        /// <returns></returns>
        public int VconnCurrentStepIncDec(string serialNumber, StepIncDec stepIncDec, PortID port, VconnEload vconnEload, CCline cCline, VCONNModeConfig vCONNModeConfig, List<EloadChannels> eloadChannels = null)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).SetCurrent(StepSelection.VCONN, stepIncDec, port, VbusEload.Off, TypeCVbusEload.Off, vconnEload, cCline, VBUSModeConfig.CCMode, vCONNModeConfig, eloadChannels, VbusVconnSelection.VCONN);
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Please do not use this API, Not for customer usage
        /// </summary>
        /// <param name="port">Refer to Port Number Port1.</param>
        /// <param name="eloadChannels"></param>
        /// <param name="vbusEload"></param>
        /// <param name="typeCVbusEload"></param>
        /// <param name="vconnEload"></param>
        /// <param name="cCline"></param>
        /// <param name="vBUSModeConfig"></param>
        /// <param name="vCONNModeConfig"></param>
        /// <returns></returns>
        public int ConfigADCVoltage(PortID port, string serialNumber, EloadChannels eloadChannels = EloadChannels.None, VbusEload vbusEload = VbusEload.Off, TypeCVbusEload typeCVbusEload = TypeCVbusEload.Off,
        VconnEload vconnEload = VconnEload.Off, CCline cCline = CCline.CC1, VBUSModeConfig vBUSModeConfig = VBUSModeConfig.CCMode, VCONNModeConfig vCONNModeConfig = VCONNModeConfig.CCMode)
        {

            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).ConfigADCVoltage(port, eloadChannels, vbusEload, typeCVbusEload, vconnEload, cCline, vBUSModeConfig, vCONNModeConfig);
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Please do not use this API, Not for customer usage
        /// </summary>
        /// <param name="port">Refer to Port Number Port1.</param>
        /// <param name="eloadChannels"></param>
        /// <param name="vbusEload"></param>
        /// <param name="typeCVbusEload"></param>
        /// <param name="vconnEload"></param>
        /// <param name="cCline"></param>
        /// <param name="vBUSModeConfig"></param>
        /// <param name="vCONNModeConfig"></param>
        ///  <param name="serialNumber">IF V-DPWR then controller serial number if V-DPWR then IP Aaddress</param>
        /// <returns></returns>
        public int ConfigADCVoltage(PortID port, string serialNumber, List<EloadChannels> eloadChannels = null, VbusEload vbusEload = VbusEload.Off, TypeCVbusEload typeCVbusEload = TypeCVbusEload.Off,
            VconnEload vconnEload = VconnEload.Off, CCline cCline = CCline.CC1, VBUSModeConfig vBUSModeConfig = VBUSModeConfig.CCMode, VCONNModeConfig vCONNModeConfig = VCONNModeConfig.CCMode)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).ConfigADCVoltage(port, eloadChannels, vbusEload, typeCVbusEload, vconnEload, cCline, vBUSModeConfig, vCONNModeConfig);
            }
            else
            {
                return -1;
            }

        }

        /// <summary>
        /// Please do not use this API, Not for customer usage
        /// <param name="port">Refer to Port Number Port1.</param>
        /// <param name="eloadChannels"></param>
        /// <param name="vbusEload"></param>
        /// <param name="typeCVbusEload"></param>
        /// <param name="vconnEload"></param>
        /// <param name="cCline"></param>
        /// <param name="vBUSModeConfig"></param>
        /// <param name="vCONNModeConfig"></param>
        /// <param name="serialNumber">IF V-DPWR then controller serial number if V-DPWR then IP Aaddress</param>
        /// <returns></returns>
        /// </summary>
        public int GetADCData(PortID port, string serialNumber, EloadChannels eloadChannels = EloadChannels.None, VbusEload vbusEload = VbusEload.Off, TypeCVbusEload typeCVbusEload = TypeCVbusEload.Off,
           VconnEload vconnEload = VconnEload.Off, CCline cCline = CCline.CC1, VBUSModeConfig vBUSModeConfig = VBUSModeConfig.CCMode, VCONNModeConfig vCONNModeConfig = VCONNModeConfig.CCMode)
        {
            if (_vCommands == null)
                return -1;
            return _vCommands.ConfigADCVoltage(port, eloadChannels, vbusEload, typeCVbusEload, vconnEload, cCline, vBUSModeConfig, vCONNModeConfig);
        }

        /// <summary>
        /// Please do not use this API, Not for customer usage
        /// <param name="port">Refer to Port Number Port1.</param>
        /// <param name="eloadPort"></param>
        /// <param name="eloadMode"></param>
        /// <param name = "serialNumber" > IF V - UP then controller serial number if V - DPWR then IP Aaddress </ param >
        /// <returns></returns>
        /// </summary>
        public int EloadPorgramingMode(PortID port, EloadProgramingPort eloadPort, EloadProgramingMode eloadMode, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).EloadProgrammingModeSelection(port, eloadPort, eloadMode);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Please do not use this API, Not for customer usage
        /// </summary>
        /// <param name="port">Refer to Port Number Port1.</param>
        /// <param name="eloadPort"></param>
        /// <param name="eloadMode"></param>
        /// <param name = "serialNumber" > IF V - UP then controller serial number if V - DPWR then IP Aaddress </ param >
        /// <returns></returns>
        public int EloadPorgramingMode(PortID port, Command autoEload, string serialNumber)
        {

            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).EloadGPIOControl(port, autoEload);
            }
            else
            {
                return 0;
            }
        }
        ///<summary>
        /// Please do not use this API, Not for customer usage
        /// <param name = "serialNumber" > IF V - UP then controller serial number if V - DPWR then IP Aaddress </ param >
        /// <return></return>
        ///</summary>

        public string GetSerialNumber(string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).GetSystemSerialNumber();
            }
            else
            {
                return "";
            }
        }

        ///// <summary>
        ///// This function will search for all the USB devices connected to the Host PC.
        ///// </summary>
        ///// <returns>List of connected USB devices </returns>
        //private USBDeviceList GetUSBDevices()
        //{
        //    if (usb_Devices_Lists != null)
        //    {
        //        usb_Devices_Lists.DeviceRemoved -= USBDevices_DeviceRemoved;
        //        usb_Devices_Lists.DeviceAttached -= USBDevices_DeviceAttached;
        //        try
        //        {
        //            // This will execute if there is different thread accessing this function 
        //            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
        //           {
        //               usb_Devices_Lists.Dispose();
        //           }));

        //        }
        //        catch (Exception ex)
        //        {
        //            HelperModule.Debug("GetUSBDevices() : ", ex);
        //        }
        //    }
        //    usb_Devices_Lists = new USBDeviceList(CyConst.DEVICES_CYUSB);
        //    usb_Devices_Lists.DeviceAttached += new EventHandler(USBDevices_DeviceAttached);
        //    usb_Devices_Lists.DeviceRemoved += new EventHandler(USBDevices_DeviceRemoved);
        //    return usb_Devices_Lists;
        //}


        #region Move to Firmware module after testing 
        /// <summary>
        /// Please do not use this API, Not for customer usage
        /// <param name="serialNumber"> IF V - UP then controller serial number if V - DPWR then IP Aaddress</param>
        /// <param name="ittiteration"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        /// </summary>

        public PPS_ADC_Channel_Read Get_PPS_ADC_Data(string serialNumber, int ittiteration = 20, int delay = 1000)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).Get_PPS_ADC_Data(ittiteration, delay);
            }
            else
            {
                return new PPS_ADC_Channel_Read();
            }
        }

        /// <summary>
        /// please do  no
        /// </summary>
        /// <param name="serialNumber">IF V - UP then controller serial number if V - DPWR then IP Address</param>
        /// <returns></returns>
        public GetPPSData Get_PPS_Data(string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).Get_PPS_Data();
            }
            else
            {
                return new GetPPSData();
            }
        }
        /// <summary>
        /// Please do not use this API, Not for customer usage
        /// <param name="Vbus_in_Volt"></param>
        /// <param name="pPSChannels"></param>
        /// <param name="serialNumber"> IF V - UP then controller serial number if V - DPWR then IP Aaddress </ param ><param>
        /// <returns></returns>
        /// </summary>
        /// 
        public bool PPS_Calibration_Set_VBUS_Voltage(double Vbus_in_Volt, PPSChannels pPSChannels, string serialNumber)
        {
            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).Set_Voltage_Pps(Vbus_in_Volt, pPSChannels);
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Please do not use this API, Not for customer usage.
        /// Enable VBUS short not mode than a second
        /// </summary>
        /// <param name="port">Refer to Port Number Port1, Port2,..., Port10.</param>
        /// <param VBUS_SHORT="vBUS_SHORT"></param>
        /// <param string="vBUS_SHORT"></param>
        /// <returns></returns>
        public bool Set_Vbus_Short(VBUS_SHORT vBUS_SHORT, string serialNumber)
        {

            if (GetDeviceHandler(serialNumber, out GRLDeviceList gRLDeviceList))
            {
                return (gRLDeviceList.ControllerObject as VsCommandSets).VBUS_SHORT(PortID.Port1, vBUS_SHORT) == 0;
            }
            else
            {
                return false;

            }

        }

        #endregion

        #endregion

        #region Private Module 
        private void Debug(DebugType debugType, string message, Exception ex = null)
        {
            DebugLogger.Instance.WriteToDebugLogger(debugType, message, ex);
        }

        private string DecodeToStringSrcCaps(SourceCapabilities sourceCaps)
        {
            string strData = "";
            if (sourceCaps != null)
            {
                for (int i = 0; i < sourceCaps.PDOlist.Count; i++)
                {
                    strData += sourceCaps.PDOlist[i].PDO_Index + ". ";
                    strData += sourceCaps.PDOlist[i].PdoType + ", ";
                    if (sourceCaps.PDOlist[i].PdoType == PDOSupplyType.FixedSupply)
                    {
                        strData += sourceCaps.PDOlist[i].Voltage + "V, ";
                        strData += sourceCaps.PDOlist[i].Current + "A, \n";
                    }
                    else if (sourceCaps.PDOlist[i].PdoType == PDOSupplyType.Battery)
                    {
                        strData += sourceCaps.PDOlist[i].Voltage + "V, Min Volt:";
                        strData += sourceCaps.PDOlist[i].MinVoltage + "V, \n";
                    }
                    else if (sourceCaps.PDOlist[i].PdoType == PDOSupplyType.VariableSupply)
                    {
                        strData += sourceCaps.PDOlist[i].Voltage + "V, Min Volt:";
                        strData += sourceCaps.PDOlist[i].MinVoltage + "V, ";
                        strData += sourceCaps.PDOlist[i].Current + "A, \n";
                    }
                    else if (sourceCaps.PDOlist[i].PdoType == PDOSupplyType.Augmented)
                    {
                        strData += sourceCaps.PDOlist[i].Voltage + "V, Min Volt:";
                        strData += sourceCaps.PDOlist[i].MinVoltage + "V, ";
                        strData += sourceCaps.PDOlist[i].Current + "A, \n";
                    }
                }
            }
            return strData;
        }

        private bool GetDeviceHandler(string serialNumber, out GRLDeviceList gRLDeviceList)
        {
            return GetGRLDeviceList.TryGetValue(serialNumber, out gRLDeviceList);
        }
        #endregion

    }

}