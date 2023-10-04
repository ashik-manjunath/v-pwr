namespace VseriesControllerLibrary_V1.SysData
{
    internal class ControlCardSysInfo
    {
        #region Public Properties

        #region BLock 1
        public int SysSerialNumber { get; set; }
        public int CC_Serial_Number { get; set; }
        public int CC_BOARD_REV { get; set; }
        public int BP_SERIAL_NUMBER { get; set; }
        public int BP_BOARD_REV { get; set; }
        public String IDN { get; set; }
        public int License { get; set; }
        public int Usage { get; set; }
        #endregion

        #region Block 2
        public DateTime Manufacturing_Date { get; set; }
        public ManufacturingLocation MfrLocation { get; set; }
        #endregion

        #region Block 3 
        public DateTime Calibration_Date { get; set; }
        public DateTime Calibration_Due { get; set; }
        public String NAME { get; set; }
        #endregion

        #region Block 4 
        public int TEMP_LICENSE { get; set; }
        public int License_Start_Date { get; set; }
        public int License_Start_Month { get; set; }
        public int License_Start_Year { get; set; }
        public int License_End_Date { get; set; }
        public int License_End_Month { get; set; }
        public int License_End_Year { get; set; }
        public int License_LastRun_Date { get; set; }
        public int License_LastRun_Month { get; set; }
        public int License_LastRun_Year { get; set; }
        #endregion

        #endregion

        #region Default Constructor 
        public ControlCardSysInfo()
        {
            
        }
        #endregion

    }
}