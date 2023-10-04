using System.Diagnostics;
using VseriesControllerLibrary_V1.Commands.HelperModules.PollingModules;
using VseriesControllerLibrary_V1.SysData.CalData;

namespace VseriesControllerLibrary_V1.SysData.SysRead
{


    internal class CalibReadDecoder
    {
        #region Private Members
        private DateTime dateTime;
        private StreamWriter fileWrite;
        private const int RESERVED = 10;
        private readonly CalibHelper _calibData;
        private readonly CalibrationSheet _calibSheet;
        #endregion

        #region Consturctor
        /// <summary>
        /// Default Constructor
        /// </summary>
        public CalibReadDecoder(VsCommandSets vsCommandSets)
        {
            _calibData = new CalibHelper(vsCommandSets);
            _calibSheet = new CalibrationSheet();
        }
        #endregion

        #region public 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fRAM"></param>
        /// <returns></returns>
        public bool Decode(Card card, PortID portID, int startAddress = 00, string calibrationSheet = "")
        {
            bool block_zero = true;
            int blockNo = 0;
            string FRAMvalue = "";
            string[] m_FRAMNewValues = new string[1];

            try
            {
                string OUTPUT_PATH = @"C:\GRL\GRL-VDPWR\Calibration\";
                dateTime = DateTime.Now;
                string timeStamp = string.Format("_{0}_{1}_{2}_{3}_{4}_{5}", dateTime.Year, dateTime.Month.ToString("D2"), dateTime.Day.ToString("D2"),
                    dateTime.Hour.ToString("D2"), dateTime.Minute.ToString("D2"), dateTime.Second.ToString("D2"));

                string finalPath = "";
                if (card == Card.Control)
                {
                    OUTPUT_PATH += @"ControlCard\";
                    if (!Directory.Exists(OUTPUT_PATH))
                        Directory.CreateDirectory(OUTPUT_PATH);
                    finalPath = OUTPUT_PATH + "Vseries_ControlCard_" + timeStamp + ".csv";

                }
                else if (card == Card.Tester)
                {
                    OUTPUT_PATH += @"TesterCard\";
                    if (!Directory.Exists(OUTPUT_PATH))
                        Directory.CreateDirectory(OUTPUT_PATH);
                    finalPath = OUTPUT_PATH + "Vseries_TesterCard_" + timeStamp + ".csv";
                }

                fileWrite = new StreamWriter(finalPath);
                string[] m_FRAMValues = new string[0];

                _calibData.RawData.Clear();
                _calibData.GetCalibrationData(card, startAddress, portID);
                _calibData.ReadOffset = -1;

                // Check the validity of the FRAM
                int blockStart = _calibData.Get2ByteInt();
                if (blockStart != 0x0A0A)
                    return false;
                _calibData.ReadOffset = -1;

                // Take the default C3 Baseboard FRAM sheet
                if (card == Card.Control)
                {
                    if (HelperModule.AppType == ApplicationType.V_UP)
                    {
                        string[] tempValues = _calibSheet.CCRev1Sheet.Split('|');
                        m_FRAMNewValues = new string[tempValues.Length];
                        m_FRAMValues = new string[tempValues.Length];
                        m_FRAMValues = tempValues;
                    }
                    else if (HelperModule.AppType == ApplicationType.V_TE)
                    {

                        string[] tempValues = _calibSheet.VTE_ControlRev1().Split('|');
                        m_FRAMNewValues = new string[tempValues.Length];
                        m_FRAMValues = new string[tempValues.Length];
                        m_FRAMValues = tempValues;
                    }

                }

                // Take the default C3 Eload FRAM sheet
                else if (card == Card.Tester)
                {
                    string[] tempValues = _calibSheet.TCRev1Sheet.Split('|');
                    if (calibrationSheet != "")
                        tempValues = calibrationSheet.Split('|');
                    m_FRAMNewValues = new string[tempValues.Length];
                    m_FRAMValues = new string[tempValues.Length];
                    m_FRAMValues = tempValues;
                }
                // Print message box that FRAM sheet was not found
                else
                {
                    HelperModule.Debug("Default Calibration sheet was not found");
                }

                for (int icount = 0; icount < m_FRAMValues.Length; icount++)
                {
                    try
                    {
                        string[] strtemp = m_FRAMValues[icount].Split(',');

                        if (strtemp.Length < 5)
                            continue;

                        // Skip the first line
                        if (strtemp[0] == "ID")
                        {
                            m_FRAMNewValues[icount] = m_FRAMValues[icount];
                            fileWrite.WriteLine(m_FRAMNewValues[icount]);
                            continue;
                        }

                        ID itempID = ID.NONE;
                        Enum.TryParse(strtemp[0], out itempID);
                        if (itempID == ID.BLOCK_START)
                        {
                            if (block_zero == false)
                                blockNo++;
                            else
                                block_zero = false;
                        }
                        uint utempoffset = Convert.ToUInt32(strtemp[1], 16);
                        int nobytes = Convert.ToInt32(strtemp[2], 10);
                        string strval = strtemp[3];
                        byte[] databuf = new byte[nobytes];
                        int paramtype = Convert.ToInt32(strtemp[5], 10);
                        if (paramtype == 1)
                        {
                            strval = strval.ToUpper();
                            if (strval == ";" || strval == "A" || strval == "V" || strval == "C")
                            {
                                FRAMvalue = strval;
                                _calibData.ReadOffset++;
                            }
                            else if (strval.Length > 1)
                            {
                                FRAMvalue = _calibData.GetString(nobytes);
                            }
                            else
                            {
                                HelperModule.Debug("Fram Data currupted : Line " + icount.ToString() + ", Value : " + strval);
                                return false;
                            }
                        }
                        else
                        {
                            if (strval == "")
                                strval = "0";
                            else if (strval == "0A0A")
                            {
                                FRAMvalue = strval;
                                _calibData.ReadOffset += 2;
                            }
                            else if (nobytes == 2 && strval != "0A0A")
                            {
                                FRAMvalue = _calibData.Get2ByteInt().ToString();
                            }
                            else if (nobytes == 1)
                            {
                                FRAMvalue = _calibData.Get1ByteInt().ToString();
                            }
                            else if (nobytes == RESERVED)
                            {
                                FRAMvalue = "0";
                                _calibData.ReadOffset += 10;
                            }
                            else
                            {
                                HelperModule.Debug("Fram Data currupted : Line " + icount.ToString() + ", Value : " + strval);
                                return false;
                            }
                        }
                        m_FRAMNewValues[icount] = strtemp[0] + "," + strtemp[1] + "," + strtemp[2] + "," + FRAMvalue + "," + strtemp[4] + "," + strtemp[5];
                        fileWrite.WriteLine(m_FRAMNewValues[icount]);
                    }
                    catch (Exception ex)
                    {
                        Debug(DebugType.STATUS, $"GrlUsbPdControllerLib : ExcelWrite() {icount} {m_FRAMValues[icount]}  {ex}");
                    }
                }
                fileWrite.Close();
                _calibData.ReadOffset = 0;
                HelperModule.Debug("Excel file is Genetrated - " + finalPath);
                Process.Start(OUTPUT_PATH);
            }
            catch (Exception ex)
            {
                Debug(DebugType.STATUS, "CalibReadDecoder : ExcelWrite() " + ex);
                HelperModule.Debug("Default Caibration File is Missing");
            }
            return true;
        }
        #endregion

        #region Private Modules
        private void Debug(DebugType debugType, string message, Exception ex = null)
        {
            DebugLogger.Instance.WriteToDebugLogger(debugType, message, ex);
        }
        #endregion
    }
}
