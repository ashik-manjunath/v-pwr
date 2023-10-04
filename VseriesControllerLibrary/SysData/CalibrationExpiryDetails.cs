namespace VseriesControllerLibrary_V1.SysData
{
    public class CalibrationExpiryDetails
    {
        #region Public Properties 
        /// <summary>
        /// This holds the calibration expiry date 
        /// </summary>
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// This holds the present date 
        /// </summary>
        public DateTime PresentDate { get; set; }

        /// <summary>
        /// This holds boolean value of weather the tester card calibration is expired or not.
        /// </summary>
        public bool IsExpired { get; set; }

        /// <summary>
        /// This holds the port number
        /// </summary>
        public PortID Port { get; set; }

        /// <summary>
        /// This holds if any error related to this data for that particular port
        /// </summary>
        public string Error { get; set; }

        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public CalibrationExpiryDetails()
        {
            ExpiryDate = DateTime.Now;
            PresentDate = DateTime.Now;
            IsExpired = false;
            Error = string.Empty;
            Port = PortID.NONE;
        }
        #endregion

    }

    public class CalibrationExpiryDetailsList
    {
        #region 
        public Dictionary<PortID, CalibrationExpiryDetails> List { get; set; }

        public bool Error { get; set; }

        public bool IsExpiredCardPresent { get; set; }

        #endregion

        #region Constructor 
        public CalibrationExpiryDetailsList()
        {
            List = new Dictionary<PortID, CalibrationExpiryDetails>();
            IsExpiredCardPresent = false;
            Error = false;
        }
        #endregion

        #region Public Module 
        public override string ToString()
        {
            string value = string.Empty;
            foreach (var details in List)
            {
                if (details.Key == PortID.NONE)
                    continue;
                var eachCard = details.Value;

                string tempValue = string.Empty;
                if (eachCard.IsExpired)
                {
                    tempValue = "Calibration is Expired";
                }
                else
                {
                    tempValue = "Calibration is up to date";

                }

                value += $"\n{eachCard.Port} : {eachCard.ExpiryDate} : {tempValue}";
            }

            return value;
        }
        #endregion
    }
}
