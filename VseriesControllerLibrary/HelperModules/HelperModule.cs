using System.Reflection;
using VseriesControllerLibrary_V1.HelperModules;

namespace VseriesControllerLibrary_V1 {
    public class HelperModule
    {
        #region Public Properties 

        /// <summary>
        /// This holds the application type V-UP / V-UP TE
        /// </summary>
        public static ApplicationType AppType { get; set; } = ApplicationType.V_TE;

        /// <summary>
        /// this holds the value to Debug logger enable or disable
        /// </summary>
        public static bool EnableDebugLogger { get; set; } = true;

        /// <summary>
        /// this holds the value to enable or disable logger for polling data 
        /// </summary>
        public static bool EnablePollingDebugLogger { get; set; } = false;

        /// <summary>
        /// This will update the status of any function or exception in the library. 
        /// </summary>
        public static StatusUpdate StatusUpdate { get; set; } = new StatusUpdate();

        internal static CableType SelectedCalbleType { get; set; }
        #endregion

        #region Public Module 
        /// <summary>
        /// This function will put the application on to sleep for giver period of time
        /// </summary>
        /// <param name="waitTimeMilli">Wait time in milliseconds</param>
        public static void Sleep(int waitTimeMilli)
        {
            if (waitTimeMilli <= 0)
                waitTimeMilli = 1;

            int i = 0;
            System.Timers.Timer delayTimer = new System.Timers.Timer(waitTimeMilli)
            {
                AutoReset = false //so that it only calls the method once
            };
            delayTimer.Elapsed += (s, args) =>  i = 1;
            delayTimer.Start();
            while (i == 0) { };
        }

        /// <summary>
        /// This function will convert the byte array value to int 
        /// </summary>
        /// <param name="byteValue">Byte array</param>
        /// <returns>Int value</returns>
        public static int GetIntFromByteArray(byte[] byteValue)
        {
            int dataObjectValue = 0;

            // converting byte to int value
            for (int i = 0; i < byteValue.Length; i++)
                dataObjectValue |= (byteValue[i] << (8 * i));

            return dataObjectValue;
        }

        /// <summary>
        /// This function will write the message to the logger file        
        /// </summary>
        /// <param name="databuffer">Data buffer ; any API command or could be data from the hardware</param>
        /// <param name="message">Message ; could be error message or status update message</param>
        public static void Debug(byte[] databuffer, string message)
        {

            int leng = databuffer.Length;
            if (leng > 270)
            {
                leng = 270;
            }

            string strPayload = "";
            for (int i = 0; i < leng; i++)
                strPayload += " 0x" + databuffer[i].ToString("X").PadLeft(2, '0');

            DebugLogger.Instance.WriteToDebugLogger(DebugType.DEBUG, $"{message} : {strPayload} ");

        }

        /// <summary>
        /// This function will write the message to the logger file
        /// </summary>
        /// <param name="message">Message ; could be error message or status update message</param>
        /// <param name="ex">Exception thrown by the function</param>
        public static void Debug(string message, Exception ex)
        {
            DebugLogger.Instance.WriteToDebugLogger(DebugType.DEBUG, message + " : ", ex);
        }

        /// <summary>
        /// This function will write the message to the logger file
        /// </summary>
        /// <param name="message">Message ; could be error message or status update message</param>
        public static void Debug(string message)
        {
            DebugLogger.Instance.WriteToDebugLogger(DebugType.DEBUG, message);
        }

        /// <summary>
        /// This function will add the status output on to a status buffer. 
        /// </summary>
        /// <param name="message">Status message</param>
        /// <param name="isWriteDebug">This will enable to write to logger file also</param>
        /// <param name="queueLength">Status buffer queue length Note : initial message will be discarded if the messages are not read</param>
        public static void AddStatusUpdate(string message, string ipAddress = "", bool isWriteDebug = false, int queueLength = 1000)
        {
            StatusUpdate.Add($"{ipAddress} : {message}", queueLength);
            if (isWriteDebug)
            {
                DebugLogger.Instance.WriteToDebugLogger(DebugType.DEBUG, $"{message}");
            }
        }

        /// <summary>
        /// This function will clear the status output  buffer. 
        /// </summary>
        public static void ClearStatusUpdate()
        {
            StatusUpdate.Clear();
        }

        /// <summary>
        /// Converts uint data to byte.
        /// </summary>
        /// <param name="dataobjs"> object to receive uint data. </param>
        /// <param name="byteLength"> byte length </param>
        /// <returns> Byte Data. </returns>
        public static List<byte> ConvertUNITtoBYTE(List<uint> dataobjs, int byteLength)
        {
            List<byte> itempbytes = new List<byte>();
            try
            {

                for (int i = 0; i < dataobjs.Count; i++)
                {
                    itempbytes.Add((byte)((dataobjs[i]) & 0xFF));
                    itempbytes.Add((byte)((dataobjs[i] >> 8) & 0xFF));
                    if (byteLength == 2)
                        continue;

                    itempbytes.Add((byte)((dataobjs[i] >> 16) & 0xFF));
                    itempbytes.Add((byte)((dataobjs[i] >> 24) & 0xFF));
                }
            }
            catch (Exception ex)
            {
                Debug(MethodBase.GetCurrentMethod().ToString() + " :", ex);
            }
            return itempbytes;
        }

        public static CableType GetSelectedCableType()
        {
            return SelectedCalbleType;
        }


        public static bool IsDigit09Dot(String value)
        {
            var regexItem = new System.Text.RegularExpressions.Regex("[^0-9.]");
            if (regexItem.IsMatch(value))
                return false;
            return true;
        }

        public static bool IsComPort(String value)
        {
            var regexItem = new System.Text.RegularExpressions.Regex("[^0-9COMcom.]");
            if (regexItem.IsMatch(value))
                return false;
            return true;
        }
       
        #endregion
    }
}
