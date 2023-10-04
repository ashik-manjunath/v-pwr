using VseriesControllerLibrary_V1.Commands.HelperModules.PollingModules;

namespace VseriesControllerLibrary_V1.SysData.CalData
{
    internal enum BlockID
    {
        NONE = -1,
        SYSTEM_DETAILS = 0,
        MANUFACTURING_DETAILS = 1,
        CALIBRATION_DETAILS = 2,
        TEMPORARY_LICENSE = 3,
    }

    internal class ControlCardCalib
    {
        #region Private Properties

        private readonly CalibHelper calibData;
        private readonly ControlCardSysInfo sysData;
        private const int RESERVED = 10;

        #endregion

        #region Public Properties 

        /// <summary>
        /// If this is true thn calibration data read correctly
        /// </summary>
        public bool IsCaldata { get; set; } = false;

        /// <summary>
        /// Contains system infomation 
        /// </summary>
        public ControlCardSysInfo SystemData { get { return sysData; } }

        /// <summary>
        /// Holds Revision 
        /// </summary>
        public int FramRevision { get; set; }

        #endregion


        #region Constructor 

        /// <summary>
        /// Default Constructor 
        /// </summary>
        /// <param name="vsCommandSets"></param>
        public ControlCardCalib(VsCommandSets vsCommandSets)
        {
            calibData = new CalibHelper(vsCommandSets);
            sysData = new ControlCardSysInfo();
        }
        #endregion

        #region Public Members

        /// <summary>
        ///  this function will load the calibration data from the controller
        /// </summary>
        /// <returns></returns>
        public bool LoadData()
        {
            bool retVal = false;
            try
            {
                // Get the calibratio data form the controller Default if will fetch the control card calibration  data 
                calibData.GetCalibrationData();

                // Set read off set to -1 to start decoding the FRAM data
                calibData.ReadOffset = -1;
                int blockStart = calibData.Get2ByteInt();

                // check if block start ic correct 
                if (blockStart != 0x0A0A)
                    return false;

                // Check if the calibratio  read data length is same as the actual length
                int toatlLength = calibData.Get2ByteInt();
                if (toatlLength != calibData.RawData.Count)
                    return false;
                else
                    retVal = true;

                // Start decoding each block
                int blockEnd = 0;
                do { retVal = DecodeBlock(calibData.ReadOffset, ref blockEnd); } while (retVal == true);
            }
            catch (Exception ex)
            {
                Debug(DebugType.DEBUG, "LoadSysDataFromFRAM : ", ex);
            }

            return (!retVal);
        }

        #endregion

        #region Private members

        /// <summary>
        /// Decode each block
        /// </summary>
        /// <param name="start"></param>
        /// <param name="blockEnd"></param>
        /// <returns></returns>
        private bool DecodeBlock(int start, ref int blockEnd)
        {
            bool retVal;
            if (start > calibData.RawData.Count)
                return false;

            bool blockStartFound = false;

            do
            {
                if (calibData.RawData[start + 1] == 0x0A && calibData.RawData[start + 2] == 0x0A)
                {
                    blockStartFound = true;
                    calibData.ReadOffset = start;
                }
                else
                {
                    start++;
                }
            } while (blockStartFound == false && start < calibData.RawData.Count - 1);

            int blockStart = calibData.Get2ByteInt();
            if (blockStart != 0x0A0A)
            {
                return false;
            }

            BlockID blockID = (BlockID)calibData.Get1ByteInt();
            retVal = false;
            
            switch (blockID)
            {
                case BlockID.NONE:
                    retVal = false;
                    blockEnd = calibData.RawData.Count;
                    break;
                case BlockID.SYSTEM_DETAILS:

                    if (calibData.RawData.Count > blockEnd)
                    {
                        FramRevision = calibData.Get1ByteInt(); // 1 bit
                        sysData.SysSerialNumber = calibData.Get2ByteInt();
                        sysData.CC_Serial_Number = calibData.Get2ByteInt();
                        sysData.CC_BOARD_REV = calibData.Get1ByteInt();
                        sysData.BP_SERIAL_NUMBER = calibData.Get2ByteInt();
                        sysData.BP_BOARD_REV = calibData.Get1ByteInt();
                        sysData.IDN = calibData.GetString(50).Trim();
                        sysData.License = calibData.Get2ByteInt();
                        sysData.Usage = calibData.Get2ByteInt();
                        if (calibData.RawData[calibData.ReadOffset + 1] == (int)(';'))
                            calibData.ReadOffset++;

                        blockEnd = calibData.ReadOffset;
                        retVal = true;
                    }
                    else
                    {
                        retVal = false;
                    }

                    break;
                case BlockID.MANUFACTURING_DETAILS:
                    if (calibData.RawData.Count > blockEnd)
                    {
                        int month = calibData.Get1ByteInt();
                        int year = calibData.Get2ByteInt();
                        sysData.Manufacturing_Date = new DateTime(year, month, 1);
                        sysData.MfrLocation = (ManufacturingLocation)calibData.Get1ByteInt();

                        if (calibData.RawData[calibData.ReadOffset + 1] == (int)(';'))
                            calibData.ReadOffset++;

                        blockEnd = calibData.ReadOffset;
                        retVal = true;
                    }
                    else
                    {
                        retVal = false;
                    }

                    break;
                case BlockID.CALIBRATION_DETAILS:

                    if (calibData.RawData.Count > blockEnd)
                    {
                        int date = calibData.Get1ByteInt();
                        int month = calibData.Get1ByteInt();
                        int year = calibData.Get2ByteInt();
                        sysData.Calibration_Date = new DateTime(year, month, date);

                        date = calibData.Get1ByteInt();
                        month = calibData.Get1ByteInt();
                        year = calibData.Get2ByteInt();
                        sysData.Calibration_Due = new DateTime(year, month, date);
                        sysData.NAME = calibData.GetString(20);

                        if (calibData.RawData[calibData.ReadOffset + 1] == (int)(';'))
                            calibData.ReadOffset++;

                        blockEnd = calibData.ReadOffset;
                        retVal = true;
                    }
                    else
                    {
                        retVal = false;
                    }


                    break;
                case BlockID.TEMPORARY_LICENSE:
                    if (calibData.RawData.Count > blockEnd)
                    {
                        sysData.TEMP_LICENSE = calibData.Get2ByteInt();
                        sysData.License_Start_Date = calibData.Get1ByteInt();
                        sysData.License_Start_Month = calibData.Get1ByteInt();
                        sysData.License_Start_Year = calibData.Get2ByteInt();
                        sysData.License_End_Date = calibData.Get1ByteInt();
                        sysData.License_End_Month = calibData.Get1ByteInt();
                        sysData.License_End_Year = calibData.Get2ByteInt();
                        sysData.License_LastRun_Date = calibData.Get1ByteInt();
                        sysData.License_LastRun_Month = calibData.Get1ByteInt();
                        sysData.License_LastRun_Year = calibData.Get2ByteInt();

                        calibData.ReadOffset += (RESERVED * 2);
                        if (calibData.RawData[calibData.ReadOffset + 1] == (int)(';'))
                            calibData.ReadOffset++;

                        blockEnd = calibData.ReadOffset;
                        retVal = true;
                    }
                    else
                    {
                        retVal = false;
                    }
                    break;
            }


            return retVal;
        }

        private void Debug(DebugType debugType, string message, Exception ex = null)
        {
            DebugLogger.Instance.WriteToDebugLogger(debugType, message, ex);
        }

        #endregion
    }
}
