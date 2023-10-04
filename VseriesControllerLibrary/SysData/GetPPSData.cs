namespace VseriesControllerLibrary_V1
{
    /// <summary>
    /// This class will contain the voltage, current , and firmware version data for PPS 
    /// </summary>
    public class GetPPSData
    {

        #region Public Properties 
        /// <summary>
        /// PPS VBUS voltage in mV
        /// </summary>
        public int Voltage { get; set; }

        /// <summary>
        /// PPS VBUS Current in mA
        /// </summary>
        public int Current { get; set; }

        /// <summary>
        /// PPS Firmware version
        /// </summary>
        public string FirmwareVersion { get; set; }

        /// <summary>
        /// Return value true / false based on the execution
        /// </summary>
        public bool ReturnValue { get; set; }

        #endregion

        #region Constructor 
        /// <summary>
        /// Constructor 
        /// </summary>
        public GetPPSData()
        {
            Voltage = 0;
            Current = 0;
            FirmwareVersion = "";
            ReturnValue = false;
        }

        /// <summary>
        /// To string
        /// </summary>
        /// <returns>string value </returns>
        public override string ToString()
        {

            return $"Voltage : {Voltage} mV" +
                $"\nADC Current : {Current} mA" +
                $"\nFirmware Version : {FirmwareVersion}";
        }

        #endregion

    }

}
