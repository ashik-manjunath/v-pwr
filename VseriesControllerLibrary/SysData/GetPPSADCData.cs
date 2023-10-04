namespace VseriesControllerLibrary_V1
{
    /// <summary>
    /// This class will contain the ADC data for PPS 
    /// </summary>
    public class GetPPSADCData
    {

        #region Public Properties 
        /// <summary>
        /// PPS VBUS voltage ADC count 
        /// </summary>
        public int ADC_Voltage_Data { get; set; }

        /// <summary>
        /// PPS VBUS Current count 
        /// </summary>
        public int ADC_Current_Data { get; set; }

        /// <summary>
        /// PPS VBUS Voltage DAC count
        /// </summary>
        public int DAC_Voltage_Data { get; set; }

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
        public GetPPSADCData()
        {
            ADC_Voltage_Data = 0;
            ADC_Current_Data = 0;
            DAC_Voltage_Data = 0;
            FirmwareVersion = "";
            ReturnValue = false;
        }

        /// <summary>
        /// To string
        /// </summary>
        /// <returns>string value </returns>
        public override string ToString()
        {

            return $"ADC Voltage : {ADC_Voltage_Data}" +
                $"\nADC Current : {ADC_Current_Data}" +
                $"\nDAC Voltage : {DAC_Voltage_Data}" +
                $"\nFirmware Version : {FirmwareVersion}";
        }

        #endregion

    }
}
