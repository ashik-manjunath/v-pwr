namespace VseriesControllerLibrary_V1.UnStructure_VDM_Message
{
    /// <summary>
    /// this class holds the Unstructure VDM Data Object 1
    /// </summary>
    public  class Unstructure_VDM_Data_Object_2
    {
        #region Public Properties 

        /// <summary>
        /// this property holds the data object value
        /// </summary>
        public int DataObject { get; set; }
        #endregion

        #region 
        /// <summary>
        /// default constructor
        /// </summary>
        public Unstructure_VDM_Data_Object_2()
        {
            DataObject = 0;
        }
        #endregion

        #region Public Members 
        /// <summary>
        /// this function will decode the assigned value to byte array
        /// </summary>
        /// <param name="avaliable_Vendor_Use"></param>
        /// <returns></returns>
        public static byte[] GetByteValues(int avaliable_Vendor_Use)
        {
            return BitConverter.GetBytes(avaliable_Vendor_Use);
        }

        public static Unstructure_VDM_Data_Object_2 DecodeByteValue(byte[] byteValue)
        {
            int dataObjectValue = HelperModule.GetIntFromByteArray(byteValue);
            return new Unstructure_VDM_Data_Object_2()
            {
                DataObject = dataObjectValue,
            };
        }
        #endregion
    }
}
