namespace VseriesControllerLibrary_V1.SysData
{
    public class CableData
    {
        #region Public properties 
        public int NumberDataObject { get; set; }

        public SOPType SOP_Type { get; set; }

        public List<uint> DataObject { get; set; }

        public uint MessageHeader { get; set; }

        public RESPONSE_TYPE ResposeType { get; set; }

        public bool IsEnabled { get; set; }
        #endregion

        #region Constructor 
        /// <summary>
        /// Constructor 
        /// </summary>
        public CableData()
        {
            MessageHeader = 0;
            DataObject = new List<uint>();
        }

        #endregion

        #region Public Member
        public new string ToString()
        {
            string updateValue = $"SOP type {SOP_Type}\n";
            updateValue += $"Response type {ResposeType}\n";
            updateValue += $"Message Header = {MessageHeader:X}h\n";

            for (int i = 0; i < DataObject.Count; i++)
            {
                updateValue += $"Data Object [{i + 1}] = {DataObject[i].ToString("X").PadLeft(8, '0')}h\n";
            }

            return updateValue;
        }
        #endregion

    }
}
