namespace VseriesControllerLibrary_V1.HelperModules.PDO_Decoder {
    /// <summary>
    /// PD status 
    /// </summary>
    public class PDC_Status
    {
        #region Public Members 
        /// <summary>
        /// PD contract status
        /// </summary>
        public PDContractNegotation Pdcstatus { get; set; }

        /// <summary>
        /// PDO Index
        /// </summary>
        public PDOIndex PdoIndex { get; set; }

        /// <summary>
        ///  Requested current or power
        /// </summary>
        public double RequestedCurrentOrPower { get; set; }

        /// <summary>
        /// Requested maximum voltage 
        /// </summary>
        public double RequestedMaxVoltage { get; set; }

        /// <summary>
        /// Requested minimum voltage 
        /// </summary>
        public double RequestedMinVoltage { get; set; }

        /// <summary>
        /// VBUS voltage
        /// </summary>
        public double VbusVoltage { get; set; }

        /// <summary>
        /// Active CC line
        /// </summary>
        public CCline ActiveCcLine { get; set; }

        #endregion

        #region Constructor
        /// <summary>
        /// Default Constructor
        /// </summary>
        public PDC_Status()
        {
            RequestedCurrentOrPower = 0;
            RequestedMaxVoltage = 0;
            RequestedMinVoltage = 0;
            VbusVoltage = 0;
            ActiveCcLine = CCline.NONE;
        }
        #endregion

        #region Public Module 
        public override string ToString()
        {
            return "Status : " + Pdcstatus.ToString() + ", \n"
            + "Requested PDO : " + PdoIndex + ", \n"
            + "Requested Power/Current : " + RequestedCurrentOrPower.ToString("#0.###") + "A, \n"
            + "Max Voltage : " + RequestedMaxVoltage.ToString("#0.###") + "V, \n"
            + "Min Voltage : " + RequestedMinVoltage.ToString("#0.###") + "V, \n"
            + "Vbus Voltage: " + VbusVoltage.ToString("#0.###") + "V, \n"
            + "Communication Line :" + ActiveCcLine.ToString();
        }
        #endregion
    }
}
