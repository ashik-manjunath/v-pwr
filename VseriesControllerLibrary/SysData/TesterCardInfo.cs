namespace VseriesControllerLibrary_V1.SysData
{
    internal class TesterCardInfo
    {
        #region Public Properties

        /// <summary>
        /// This property holds system product ID
        /// </summary>
        public String IDN { get; set; }

        /// <summary>
        /// This property holds serial number of system 
        /// </summary>
        public int SysSerialNumber { get; set; }

        /// <summary>
        /// This property holds serial number of control card
        /// </summary>
        public int CC_SerialNumber { get; set; }

        /// <summary>
        /// This property holds control card board revision 
        /// </summary>
        public int CC_Revision { get; set; }

        /// <summary>
        /// This property holds serial number of back plane Board
        /// </summary>
        public int BP_SerialNumber { get; set; }

        /// <summary>
        /// This property holds back plane board revision 
        /// </summary>
        public int BP_Revision { get; set; }

        /// <summary>
        /// This property holds license verification
        /// </summary>
        public int License { get; set; }

        /// <summary>
        /// This property holds license repeat for verification
        /// </summary>
        public int Usage { get; set; }

        /// <summary>
        /// This property holds manufacturing date in Number
        /// </summary>
        public DateTime Manufacturing_Date { get; set; }

        /// <summary>
        /// This property holds the location of manufacturing ; 1 = BANGALORE / 2 = Taiwan / 3= USA
        /// </summary>
        public ManufacturingLocation MfrLocation { get; set; }

        /// <summary>
        /// This property holds the date of calibration
        /// </summary>
        public DateTime Calibration_Date { get; set; }

        /// <summary>
        /// This property holds the date of next calibration to be done
        /// </summary>
        public DateTime Calibration_Due { get; set; }

        /// <summary>
        /// This property holds the name of the calibrated person
        /// </summary>
        public String NAME { get; set; }

        /// <summary>
        /// This property holds the temporary license 
        /// </summary>
        public int TEMP_LICENSE { get; set; }

        /// <summary>
        /// This property holds the temporary license start date
        /// </summary>
        public DateTime TempLicenseStartDate { get; set; }

        /// <summary>
        /// This property holds the temporary license end date
        /// </summary>
        public DateTime TempLicenseEndDate { get; set; }

        /// <summary>
        /// This property holds the last run date
        /// </summary>
        public DateTime LastRunDate { get; set; }

        #endregion

        #region Constructor
        /// <summary>
        /// Default Constructor
        /// </summary>
        public TesterCardInfo()
        {
            IDN = "";
            SysSerialNumber = 0;
            CC_SerialNumber = 0;
            CC_Revision = 0;
            BP_SerialNumber = 0;
            BP_Revision = 0;
            License = 0;
            Usage = 0;
            NAME = "";
            TEMP_LICENSE = 0;
        }

        #endregion
    }
}