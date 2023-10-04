using VseriesControllerLibrary_V1.Commands.HelperModules.PollingModules;

namespace VseriesControllerLibrary_V1 {

    /// <summary>
    ///  Connection to the device
    /// </summary>
    internal class ConnectDevices
    {
        #region Private Members
        // Commands object
        private VsCommandSets _vCommands;

        // list of the device connected 
        private Dictionary<string, GRLDeviceList> _vControllersList;

        // unique USB serial number 
        private const int USB_SERIAL_NUMBER_LENGTH = 11;

        private const int USB_SERIAL_NUMBER_PREV_LENGTH = 9;
      
        private System.Timers.Timer aTimer;


        #endregion

        #region Public Members
        /// <summary>
        /// List of controller connected to the HOST PC 
        /// </summary>
        public Dictionary<string, GRLDeviceList> VControllersList
        {
            get
            {
                return _vControllersList;
            }
            private set
            {
                if (_vControllersList == value)
                    return;
                _vControllersList = value;

            }
        }

        #endregion

        #region Constructor
        /// <summary>
        /// default constructor
        /// </summary>
        public ConnectDevices()
        {
            VControllersList = new Dictionary<string, GRLDeviceList>();
            //TimerEvent();
        }

        #endregion

        #region Public Modules

        public bool Connect(string ipAddress = "192.168.0.4")
        {
            bool retValue = false;
            try
            {
                if (!VControllersList.ContainsKey(ipAddress))
                {
                    _vCommands = new VsCommandSets();
                    if (_vCommands.InitilizeController(ipAddress))
                    {
                        GRLDeviceList gRLDeviceList = new GRLDeviceList
                        {
                            ControllerObject = _vCommands as VsCommandSets,
                            //EloadFirmwareUpdate = new EloadFirmware(),
                            //FirmwareUpdate = new Firmware(_vCommands),
                            IPAddress = ipAddress,
                        };
                        VControllersList.Add(ipAddress, gRLDeviceList);
                        retValue = true;
                    }
                    else
                    {
                        retValue = false;
                    }

                }
                else
                {
                    // Handle Second time connection
                    VControllersList.Remove(ipAddress);
                    retValue = Connect(ipAddress);
                    return retValue;
                }
            }
            catch (Exception ex)
            {
                Debug(DebugType.DEBUG, " ConnectDevices AddDevice : ", ex);
            }

            return retValue;
        }


        /// <summary>
        /// this function will remove the device when device remove event is triggered
        /// </summary>
        /// <param name="usbSerialNumber">unique USB serial number</param>
        public void RemoveDevice(string usbSerialNumber)
        {
            try
            {
                if (VControllersList.ContainsKey(usbSerialNumber))
                {
                    VControllersList.TryGetValue(usbSerialNumber, out GRLDeviceList devices);
                    var deviceInfo = devices;
                    var name = deviceInfo.FriendlyName;
                    byte usbaddress = deviceInfo.USBAddress;
                    usbSerialNumber = deviceInfo.USBSerialNumber;
                    VsCommandSets _vCommands = deviceInfo.ControllerObject as VsCommandSets;
                    VControllersList.Remove(usbSerialNumber);
                    if ((name.Contains("Manufacture Tester") || name.Contains("BootLoader") || name.Contains("BootProgrammer")) && HelperModule.AppType == ApplicationType.V_UP)
                    {
                        DictonaryEventHandler.OnController_DeviceRemovedChanged(devices);
                    }
                    else if (devices.FriendlyName.Contains("LoopBack") || devices.FriendlyName.Contains("PassMark"))
                    {
                        DictonaryEventHandler.OnLoopback_DeviceRemovedChanged(devices);
                    }
                }

            }
            catch (Exception ex)
            {
                Debug(DebugType.DEBUG, " RemoveDevice : ", ex);
            }
        }

        public bool Disconnect(string usbSerialNumber)
        {
            if (VControllersList.ContainsKey(usbSerialNumber))
            {
                VControllersList.TryGetValue(usbSerialNumber, out GRLDeviceList devices);
                VsCommandSets _vCommands = devices.ControllerObject as VsCommandSets;
                if (_vCommands.DisconnectContoller())
                {
                    if (VControllersList.Remove(usbSerialNumber))
                    {
                        HelperModule.Debug($"Dispose succesfull {usbSerialNumber}");
                        return true;
                    }
                    else
                    {
                        HelperModule.Debug($"Dispose unsuccesfull {usbSerialNumber}");
                        return false;
                    }
                }
                else
                {
                    HelperModule.Debug($"Dispose unsuccesfull {usbSerialNumber}");
                    return false;
                }
            }

            return false;
        }

        #endregion

        #region Private Modules
        private void Debug(DebugType debugType, string message, Exception ex = null)
        {
            DebugLogger.Instance.WriteToDebugLogger(debugType, message, ex);
        }

        private void TimerEvent()
        {
            // Create a timer and set a two second interval.
            aTimer = new System.Timers.Timer();
            aTimer.Interval = 1000 * 60 * 1;

            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;

            // Have the timer fire repeated events (true is the default)
            aTimer.AutoReset = true;

            // Start the timer
            aTimer.Enabled = true;

        }


        private void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                HelperModule.Debug($"The Elapsed event was raised at {e.SignalTime}");
                if (VControllersList.Count > 0)
                {
                    foreach (var eachDevice in VControllersList)
                    {
                        VsCommandSets vsCommandSets = eachDevice.Value.ControllerObject as VsCommandSets;
                        DateTime internalTimer = DateTime.Now;
                        TimeSpan differenc = internalTimer - vsCommandSets.GetLastCommandTime();
                        if (differenc.TotalSeconds > (5 * 60 * 1000)) // 5 mins
                        {
                                vsCommandSets.DisconnectContoller();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HelperModule.Debug("OnTimedEvent ", ex);
            }
        }

        #endregion
    }

}


public class GRLDeviceList
{
    #region Public Properties

    /// <summary>
    /// Controller index 
    /// </summary>
    public int ControllerIndex { get; set; }

    /// <summary>
    /// Serial number of the hardware 
    /// </summary>
    public string SerialNumber { get; set; }

    /// <summary>
    /// Commands object as VsCommandSets
    /// </summary>
    public object ControllerObject { get; set; }

    /// <summary>
    /// Unique USB address
    /// </summary>
    public byte USBAddress { get; set; }

    /// <summary>
    /// Unique USB serial number
    /// </summary>
    public string USBSerialNumber { get; set; }


    /// <summary>
    /// Unique IP Address
    /// </summary>
    public string IPAddress { get; set; }


    /// <summary>
    /// Name of the connecting device
    /// </summary>
    public string FriendlyName { get; set; }

    /// <summary>
    /// Data speed
    /// </summary>
    public byte Speed { get; set; }

    /// <summary>
    /// This will contain the manufacturing year
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// This will contain the manufacturing month
    /// </summary>
    public int Month { get; set; }

    /// <summary>
    /// This will contain the firmware update instance
    /// </summary>
    public object FirmwareUpdate { get; set; }

    /// <summary>
    /// This will contain the eload firmware update instance
    /// </summary>
    public object EloadFirmwareUpdate { get; set; }

    #endregion

    #region Constructor 
    /// <summary>
    /// Constructor
    /// </summary>
    public GRLDeviceList()
    {
        ControllerIndex = 0;
        USBAddress = 0;
        Speed = 0;
        USBSerialNumber = "";
        IPAddress = "";
        FriendlyName = "";
        SerialNumber = "";
        ControllerObject = null;
        FirmwareUpdate = null;
        EloadFirmwareUpdate = null;
        Year = 0;
        Month = 0;
    }
    #endregion

}


