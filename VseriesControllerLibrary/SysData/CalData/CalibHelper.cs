using System.Text;
using VseriesControllerLibrary_V1.Commands.HelperModules.PollingModules;

namespace VseriesControllerLibrary_V1.SysData.CalData
{
    /// <summary>
    /// Contains Sheet data
    /// </summary>
    internal class CalibHelper
    {
        #region Public Members

        /// <summary>
        /// Offset for Rwadata list (present Location)
        /// </summary>
        public int ReadOffset { get; set; }

        /// <summary>
        /// Offset for SetRawData List (Present location)
        /// </summary>
        public int SetOffset { get; set; }

        /// <summary>
        /// Contains all the raw data form the Sheet after you call Read API
        /// </summary>
        public List<byte> RawData { get; private set; }

        /// <summary>
        /// You can set the byte values and write to Sheet 
        /// </summary>
        public List<byte> SetRawData { get; private set; }
        #endregion

        #region Private Members

        private readonly VsCommandSets _vCommand;

        #endregion

        #region Default Constructor

        /// <summary>
        /// Default Constructor
        /// </summary>
        public CalibHelper(VsCommandSets vsCommandSets)
        {
            _vCommand = vsCommandSets;
            RawData = new List<byte>();
            ReadOffset = 0;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get all the Sheet Data from the board
        /// </summary>
        /// <param name="SheetEnum">enum based on the required Sheet</param>
        /// <returns>List<byte> -List of byte data</returns>
        public List<byte> GetCalibrationData(Card card = Card.Control, int startAddress = 00, PortID portID = PortID.NONE)
        {
            int readStartIndex = startAddress;
            int defaultLength = 246;
            int count = 0;
            int NUMBEROFCYCLE = 5;

            RawData = new List<byte>();
            bool retVal;
            do
            {
                retVal = Read(readStartIndex, defaultLength, card, portID);
                readStartIndex += defaultLength;
                count++;
            } while (retVal == true && count <= NUMBEROFCYCLE);

            return RawData;
        }

        /// <summary>
        /// Read Sheet data from the tester 
        /// </summary>
        /// <param name="startOffset">Start Address</param>
        /// <param name="length">Total length</param>
        /// <param name="SheetEnum">Which Sheet C2, C3, C3 eLoad</param>
        /// <returns></returns>
        public bool Read(int startOffset, int length, Card card = Card.Control, PortID portID = PortID.NONE)
        {
            byte[] dataBuffer = new byte[1024];
            bool retVal = true;
            try
            {
                if (!(_vCommand.ReadCalibrationControlCard(ref dataBuffer, (uint)startOffset, (uint)length, card, portID)))
                {
                    Debug(DebugType.DEBUG, "Read Failed");
                    return false;
                }


                int semicolonCount = 0;
                for (int i = 0; i < length; i++)
                {
                    RawData.Add(dataBuffer[i + 6]);
                    if (dataBuffer[i + 6] == 59)
                    {
                        semicolonCount++;
                        if (semicolonCount == 4)
                        {
                            retVal = false;
                            break;
                        }
                    }
                    else
                    {
                        semicolonCount = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug(DebugType.STATUS, "WriteToFile : ReadValuesFromSheet()" + ex);
            }
            return retVal;
        }

        /// <summary>
        /// App command to write to C2 Baseboard Sheet
        /// </summary>
        /// <param name="value">Sheet Data</param>
        /// <param name="inofbytes">Number of bytes</param>
        /// <param name="uioffset">Start Address</param>
        /// <returns></returns>
        public bool Write(List<byte> data, uint uioffset, Card card = Card.Control, PortID portID = PortID.NONE)
        {
            bool retVal;
            try
            {
                retVal = _vCommand.WriteCalibrationControlCard(data, uioffset, card, portID);
            }
            catch (Exception ex)
            {
                retVal = false;
                Debug(DebugType.STATUS, "CalibrationData : Write()" + ex);
            }

            return retVal;
        }

        /// <summary>
        /// Get 2 byts integer value from RawData
        /// </summary>
        /// <returns>Int value</returns>
        public int Get2ByteInt()
        {
            ReadOffset++;
            int retVal = 0;
            try
            {
                retVal = (int)(((int)(RawData[ReadOffset + 1]) << 8) + (int)RawData[ReadOffset]);
                ReadOffset++;
            }
            catch (Exception ex)
            {
                Debug(DebugType.STATUS, "C2SystemData : Get2ByteInt()" + ex);
            }


            return retVal;
        }

        /// <summary>
        /// Set 2 bytes integer value. Value will be added to SetRawData
        /// </summary>
        /// <returns>Int value</returns>
        public void Set2ByteInt(int value)
        {
            SetRawData.Add((byte)(value & 0xFF));
            SetRawData.Add((byte)((value >> 8) & 0xFF));
        }

        /// <summary>
        /// Set 2 bytes integer value. Value will be added to SetRawData
        /// </summary>
        /// <param name="value"></param>
        public void Set1ByteInt(int value)
        {
            SetRawData.Add((byte)(value));
        }

        /// <summary>
        /// Set the string value. Value will be added in SetRawData
        /// </summary>
        /// <param name="value"></param>
        public void SetString(string value, int length)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(value);
                if (data.Length > length)
                    return;
                byte[] data2 = new byte[data.Length + (length - data.Length)];
                for (int i = 0; i < data.Length; i++)
                    data2[i] = data[i];

                for (int i = 0; i < data2.Length; i++)
                    SetRawData.Add(data2[i]);

            }
            catch (Exception ex)
            {
                Debug(DebugType.STATUS, "C2SystemData : SetString()" + ex);
            }
        }

        /// <summary>
        /// Get 1 byte integer value from RawData
        /// </summary>
        /// <returns>Int value</returns>
        public int Get1ByteInt()
        {
            ReadOffset++;
            return RawData[ReadOffset];
        }

        /// <summary>
        /// Convets bytes values to Strings 
        /// </summary>
        /// <param name="length"></param>
        /// <returns>String value</returns>
        public string GetString(int length)
        {
            string retVal = " ";
            byte[] byteArray = new byte[length];
            int index = 0;
            try
            {
                for (int i = ReadOffset; ((i < RawData.Count) && index < length); i++, index++)
                {
                    ReadOffset++;
                    byteArray[index] = RawData[i];

                }
                var outputBytes = Encoding.Convert(Encoding.UTF8, Encoding.ASCII, byteArray);
                retVal = ((Encoding.ASCII.GetString(outputBytes).Replace("\0", "")).Replace("\u0002", "")).Replace("\a", "").Replace("\u0003", "");

            }
            catch (Exception ex)
            {
                Debug(DebugType.STATUS, "WriteToTextFile : ConvertString()" + ex);
            }
            return retVal;
        }

        /// <summary>
        /// Clear SetRawData and SetOffset = 0;
        /// </summary>
        public void ClearSetRawData()
        {
            SetRawData = new List<byte>();
            SetOffset = 0;
        }

        /// <summary>
        /// Clear the RewData and ReadOffset
        /// </summary>
        public void ClearRawData()
        {
            RawData = new List<byte>();
            ReadOffset = 0;
        }

        /// <summary>
        /// Decode the provied Sheet sheet. Extrated the only data coloumn
        /// </summary>
        /// <param name="strFileName">File Path</param>
        /// <returns>Int value, total number of bytes</returns>
        public List<RowData> DecodeSheet(string strFileName, InputData InputData = InputData.File)
        {
            int tBytes = 0;
            int blockNo = 0;
            List<RowData> SheetDataList = new List<RowData>();

            try
            {
                // Read all the line in the Sheet sheet            
                string[] m_SheetValues = new string[0];
                if (InputData == InputData.File)
                {
                    string[] values = File.ReadAllLines(strFileName);
                    m_SheetValues = new string[values.Length];
                    m_SheetValues = values;
                }
                else if (InputData == InputData.String)
                {
                    string[] values = strFileName.Split('|');
                    m_SheetValues = new string[values.Length];
                    m_SheetValues = values;
                }


                // Start decoding line by line 
                for (int icount = 0; icount < m_SheetValues.Length; icount++)
                {
                    try
                    {
                        // Split each line based on comma
                        string[] strtemp = m_SheetValues[icount].Split(',');

                        // skip if the coloumns are less than 5
                        if (strtemp.Length < 5)
                            continue;

                        if (strtemp[0] == "ID")
                            continue;

                        // Decode each Row
                        ID itempID = ID.NONE;
                        Enum.TryParse(strtemp[0], out itempID);
                        if (itempID == ID.BLOCK_ID)
                            int.TryParse(strtemp[3], out blockNo);

                        uint utempoffset = Convert.ToUInt32(strtemp[1], 16);
                        int nobytes = Convert.ToInt32(strtemp[2], 10);
                        tBytes += nobytes;
                        string strval = strtemp[3];
                        byte[] databuf = new byte[nobytes];
                        int paramtype = Convert.ToInt32(strtemp[5], 10);
                        if (paramtype == 1)
                        {
                            var data = Encoding.UTF8.GetBytes(strval);
                            var outputBytes = Encoding.Convert(Encoding.UTF8, Encoding.ASCII, data);
                            for (int p = 0; p < outputBytes.Length; p++)
                                databuf[p] = outputBytes[p];
                        }
                        else
                        {
                            if (strval == "")
                                strval = "0";
                            if (strval == "0A0A")
                            {
                                uint itempda = Convert.ToUInt32(strval, 16);
                                if (nobytes > 0)
                                    databuf[0] = (byte)(itempda & 0xFF);
                                if (nobytes > 1)
                                    databuf[1] = (byte)((itempda >> 8) & 0xFF);
                            }
                            else
                            {
                                uint itempda = Convert.ToUInt32(strval, 10);
                                if (nobytes > 0)
                                    databuf[0] = (byte)(itempda & 0xFF);
                                itempda >>= 8;
                                if (nobytes > 1)
                                    databuf[1] = (byte)(itempda & 0xFF);
                            }

                        }

                        // Create new instance for each row
                        RowData itempdata = new RowData(itempID, utempoffset, nobytes, strval, databuf)
                        {
                            SheetBlock = (Blocks)(blockNo),
                            Description = strtemp[4]
                        };
                        SheetDataList.Add(itempdata);
                    }
                    catch(Exception ex)
                    {
                        Debug(DebugType.DEBUG, "DecodeSheet ", ex);

                    }
                }
            }
            catch (Exception ex)
            {
                Debug(DebugType.DEBUG, "DecodeSheet ", ex);
            }



            // Return thr total number of bytes
            return SheetDataList;
        }



        #endregion

        #region Private Members 

        private void Debug(DebugType debugType, string message, Exception ex = null)
        {
            DebugLogger.Instance.WriteToDebugLogger(debugType, message, ex);
        }

        #endregion
    }

    /// <summary>
    /// Each row of Sheet sheet data 
    /// </summary>
    internal class RowData
    {
        #region Public Members

        /// <summary>
        /// This holds Sheet ID
        /// </summary>
        public ID SheetID { get; set; } = ID.NONE;

        /// <summary>
        /// This holds Sheet Block
        /// </summary>
        public Blocks SheetBlock { get; set; } = Blocks.NONE;

        /// <summary>
        /// This holds the start address
        /// </summary>
        public uint StartAddress { get; set; } = 0;

        /// <summary>
        /// This holds the number of bytes
        /// </summary>
        public int NumberOfBytes { get; set; } = 0;

        /// <summary>
        /// this holds the string value
        /// </summary>
        public string StringValue { get; set; } = "";

        /// <summary>
        /// This holds the byte value
        /// </summary>
        public byte[] ByteValue { get; set; } = null;

        /// <summary>
        /// This holds the Discription
        /// </summary>
        public string Description { get; set; } = "";

        #endregion

        #region Default Constrictor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="SheetID">Sheet ID</param>
        /// <param name="startAddress">Start address</param>
        /// <param name="noOfBytes">Numner of bytes</param>
        /// <param name="stringValue">String vlaue</param>
        /// <param name="byteValue">Byte value</param>
        public RowData(ID sheetID, uint startAddress, int noOfBytes, string stringValue, byte[] byteValue)
        {
            this.SheetID = sheetID;
            this.StartAddress = startAddress;
            this.NumberOfBytes = noOfBytes;
            this.StringValue = stringValue;
            this.ByteValue = byteValue;
        }

        #endregion
    }
}
