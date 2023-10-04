using VseriesControllerLibrary_V1.Commands.HelperModules.PollingModules;
using VseriesControllerLibrary_V1.SysData.CalData;
using VseriesControllerLibrary_V1.SysData.SysRead;

namespace VseriesControllerLibrary_V1.SysData.SysWrite
{
    internal class CalibrationWrite
    {
        #region Private Members

        // Instance of FRAM data Read / Write
        private readonly CalibHelper _calibData;
        private readonly CalibrationSheet calibrationSheet;
        #endregion

        #region Contructor

        /// <summary>
        /// Default Constructor
        /// </summary>
        public CalibrationWrite(VsCommandSets vsCommandSets)
        {
            _calibData = new CalibHelper(vsCommandSets);
            calibrationSheet = new CalibrationSheet();
        }

        #endregion

        #region Public Methods 

        /// <summary>
        /// Decoed FRAM sheet row by row
        /// </summary>
        /// <param name="fileName">fram sheet full path</param>
        /// <param name="fRAM">FRAM enum C2, C3 or C3 eLoad</param>
        /// <returns></returns>
        public int Decode(string fileName, int startAddress, InputData inputData, Card card, PortID portID)
        {

            int framlength = 0;
            List<byte> bytetemp = new List<byte>();

            if(card == Card.Default)
            {
                fileName = calibrationSheet.Default;
                card = Card.Tester;
                inputData = InputData.String;
            }

            // Decode the provided FRAM sheet and return total number of bytes
            List<RowData> _calibDataList = _calibData.DecodeSheet(fileName, inputData);

            for (int i = 0; i < _calibDataList.Count; i++)
            {
                var data = _calibDataList[i];
                bytetemp.AddRange(data.ByteValue);
                framlength += data.ByteValue.Length;
            }
            bool retVal = _calibData.Write(bytetemp, (uint)startAddress, card, portID);
            if (!retVal)
                return -1;

            return framlength;
        }

        #endregion

    }
}
