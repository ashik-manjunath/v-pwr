namespace VseriesControllerLibrary_V1.HelperModules.PDO_Decoder {
    /// <summary>
    /// PDO 
    /// </summary>
    public class PDO
    {
        #region Public Properties 
        /// <summary>
        /// PDO index
        /// </summary>
        public int PDO_Index { get; set; }

        /// <summary>
        /// PDO type
        /// </summary>
        public PDOSupplyType PdoType { get; set; }

        /// <summary>
        /// Voltage
        /// </summary>
        public double Voltage { get; set; }

        /// <summary>
        /// Current
        /// </summary>
        public double Current { get; set; }

        /// <summary>
        /// Minimum volatage
        /// </summary>
        public double MinVoltage { get; set; }

        #endregion
        
        #region Constructor 
        /// <summary>
        /// Default Constructor
        /// </summary>
        public PDO()
        {
            PDO_Index = 0;
            Voltage = -1;
            Current = -1;
            MinVoltage = -1;
        }
        #endregion
    }
}
