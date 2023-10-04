namespace VseriesControllerLibrary_V1.UnStructure_VDM_Message
{
    /// <summary>
    /// this class holds the unstructured VDM information 
    /// </summary>
    internal class Unstructured_VDM_Information
    {
        #region Public Memebers 

        public int NumberOfDataObjects { get; set; }

        /// <summary>
        /// this holds the the Unstructure_VDM_Data_Object_1
        /// </summary>
        public Unstructure_VDM_Data_Object_1 Unstrt_VDM_DataObject_1 { get; set; }
        public Unstructure_VDM_Data_Object_2 Unstrt_VDM_DataObject_2 { get; set; }
        public List<uint> DataObject_RawValue { get; private set; }

        #endregion

        #region Constructor 
        /// <summary>
        /// Default constructor
        /// </summary>
        public Unstructured_VDM_Information()
        {
            NumberOfDataObjects = 0;
            Unstrt_VDM_DataObject_1 = new Unstructure_VDM_Data_Object_1();
            Unstrt_VDM_DataObject_2 = new Unstructure_VDM_Data_Object_2();
        }

        public void AddDataObject(List<uint> value)
        {
            DataObject_RawValue = new List<uint>();
            DataObject_RawValue.AddRange(value);
        }

        #endregion
    }
}
