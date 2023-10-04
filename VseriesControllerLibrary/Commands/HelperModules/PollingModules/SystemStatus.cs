namespace VseriesControllerLibrary_V1.Commands.HelperModules.PollingModules
{

    /// <summary>
    /// System status, 
    /// </summary>
    public class SystemStatus
    {
        #region Public properties 
        /// <summary>
        /// PD contract status
        /// </summary>
        public int PDC_Status { get; set; }

        /// <summary>
        /// PDO index
        /// </summary>
        public int PDO_Index { get; set; }

        /// <summary>
        /// Port number 
        /// </summary>
        public PortID Port { get; set; }

        /// <summary>
        /// Power LED
        /// </summary>
        public LED_Color Power { get; set; }

        /// <summary>
        /// VBUS LED
        /// </summary>
        public LED_Color VBUS { get; set; }

        /// <summary>
        /// Data Lock LED
        /// </summary>
        public LED_Color DataLock { get; set; }

        /// <summary>
        /// Data Error LED
        /// </summary>
        public LED_Color DataError { get; set; }

        /// <summary>
        /// Enumeration LED
        /// </summary>
        public LED_Color En_D_Tx { get; set; }

        /// <summary>
        /// PD or BC 1.2 LED
        /// </summary>
        public LED_Color PD_BC_12 { get; set; }

        /// <summary>
        /// PD negotiation LED
        /// </summary>
        public LED_Color PD_N { get; set; }

        /// <summary>
        /// Data Link speed
        /// </summary>
        public LED_Color LinkSpeed { get; set; }

        /// <summary>
        /// VBUS VOlateg 
        /// </summary>
        public int VBUS_Voltage { get; set; }

        /// <summary>
        /// VBUS Current 
        /// </summary>
        public int VBUS_Current { get; set; }

        /// <summary>
        /// VCONN Voltage 
        /// </summary>
        public int VCONN_Voltage { get; set; }

        /// <summary>
        /// VCONN Current
        /// </summary>
        public int VCONN_Current { get; set; }

        /// <summary>
        /// Source caps Re-Advertise or not, NOTE: this will become false again only after sending the source capabilities API.
        /// </summary>
        public bool Re_Advertice { get; set; }

        /// <summary>
        /// This holds if BatterySoC, BatteryStatus, TemperatureStatus value are valid or not, If true then values are valid else not valid.
        /// </summary>
        public bool IsBatteryValueValid { get; set; }


        /// <summary>
        /// Time span at which the data was read from the tester card.
        /// </summary>
        public TimeSpan TimeSpan { get; set; }

        /// <summary>
        /// Battery charging status - Charging or not charging 
        /// </summary>
        public BatteryChargingStatus BatteryStatus { get; set; }

        /// <summary>
        /// Battery charging status in kWh
        /// </summary>
        public int BatterySoC { get; set; }

        /// <summary>
        /// Battery temperature status - NotSupported = 0,
        ///  Normal, 
        ///  Warning ,
        ///  OverTemperature ,
        /// </summary>
        public BatteryTemperatureStatus TemperatureStatus { get; set; }

        /// <summary>
        /// Battery temperature in Degree Celsius
        /// </summary>
        public int BatteryTemperature { get; set; }

        /// <summary>
        /// This holds the DUT VID 
        /// </summary>
        public int DUT_VID { get; set; }

        /// <summary>
        /// This holds the DUT PID 
        /// </summary>
        public int DUT_PID { get; set; }

        /// <summary>
        /// This holds the battery design capasity 
        /// </summary>
        public int BatteryDesignCapacity { get; set; }

        /// <summary>
        /// This holds the previous charge capacity for the battery
        /// </summary>
        public int BatteryPreviousChargeCapacity { get; set; }

        /// <summary>
        /// This holds the battery Type 
        /// </summary>
        public int BatteryType { get; set; }



        #endregion

        #region Constructor 

        /// <summary>
        /// Default Constructor
        /// </summary>
        public SystemStatus()
        {
            PDC_Status = 0;
            PDO_Index = 0;
            Port = PortID.NONE;
            Power = LED_Color.none;
            VBUS = LED_Color.none;
            DataLock = LED_Color.none;
            DataError = LED_Color.none;
            En_D_Tx = LED_Color.none;
            PD_BC_12 = LED_Color.none;
            PD_N = LED_Color.none;
            LinkSpeed = LED_Color.none;
            VBUS_Voltage = 0;
            VBUS_Current = 0;
            VCONN_Voltage = 0;
            VCONN_Current = 0;
            BatteryStatus = BatteryChargingStatus.NotCharging;
            BatterySoC = 0;
            TemperatureStatus = BatteryTemperatureStatus.Normal;
            BatteryTemperature = 0;
            DUT_VID = 0;
            DUT_PID = 0;
            BatteryDesignCapacity = 0;
            BatteryPreviousChargeCapacity = 0;
            BatteryType = 0;
        }
        #endregion


        public override string ToString()
        {
            string value1 = $"\n-------------------------" +
             $"\nPDC Status : {PDC_Status}" +
             $"\nPDO Index : {PDO_Index}" +
             $"\nPort ID : {Port}" +
             $"\nPower LED : {Power}" +
             $"\nVBUS LED : {VBUS}" +
             $"\nDataLock  LED : {DataLock}" +
             $"\nDataError LED : {DataError}" +
             $"\nEn_D_Tx  LED : {En_D_Tx}" +
             $"\nPD_BC_12 : {PD_BC_12}" +
             $"\nPD_N : {PD_N}" +
             $"\nLinkSpeed : {LinkSpeed}" +
             $"\nLinkSpeed : {LinkSpeed}" +
             $"\n-------------------------" +
             $"\nVBUS Voltage in mV: {VBUS_Voltage}" +
             $"\nVBUS Current in mA: {VBUS_Current}" +
             $"\nVCONN Voltage in mV: {VCONN_Voltage}" +
             $"\nVCONN Current in mA: {VCONN_Current}" +
             $"\nRe Advertise : {Re_Advertice}" +
             $"\n-------------------------" +
             $"\nIs Battery details valid : {IsBatteryValueValid}" +
             $"\nBattery SoC : {BatterySoC}" +
             $"\nBattery Status : {BatteryStatus}" +
             $"\nBattery Temperature status : {TemperatureStatus}" +
             $"\nBattery Temperature : {BatteryTemperature} deg C" +
             $"\nDUT VID : {DUT_VID:X}" +
             $"\nDUT PID : {DUT_PID:X}" +
             $"\nBattery design capacity : {BatteryDesignCapacity}" +
             $"\nBattery previous charge capacity : {BatteryPreviousChargeCapacity}" +
             $"\nBattery type : {BatteryType}" +
             $"\n-------------------------";

            return value1;
        }
    }
}
