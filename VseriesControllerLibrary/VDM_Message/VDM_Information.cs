namespace VseriesControllerLibrary_V1.VDM_Message
{
    /// <summary>
    /// this class constains all the VDM data object
    /// </summary>
    internal class VDM_Information
    {
        #region 
        /// <summary>
        /// this holds the number of data objects present 
        /// </summary>
        public int NumberOfDataObjects { get; set; }

        /// <summary>
        /// this holds the values for vdm data object 1
        /// </summary>
        public VDM_Data_Object_1 VDM_Data_Object_1 { get; set; }

        /// <summary>
        /// this holds the values for vdm data object 2        
        /// </summary>
        public VDM_Data_Object_2 VDM_Data_Object_2 { get; set; }

        /// <summary>
        /// this holds the values for vdm data object 3
        /// </summary>
        public VDM_Data_Object_3 VDM_Data_Object_3 { get; set; }

        /// <summary>
        /// this holds the values for vdm data object 4        
        /// </summary>
        public VDM_Data_Object_4 VDM_Data_Object_4 { get; set; }

        /// <summary>
        /// this holds the values for vdm data object 5
        /// </summary>
        public VDM_Data_Object_5 VDM_Data_Object_5 { get; set; }

        /// <summary>
        /// this holds the values for vdm data object 6
        /// </summary>
        public VDM_Data_Object_6 VDM_Data_Object_6 { get; set; }

        /// <summary>
        /// this holds the values for vdm data object 7
        /// </summary>
        public VDM_Data_Object_7 VDM_Data_Object_7 { get; set; }

        #endregion

        #region Constructor 
        /// <summary>
        /// Default constructor
        /// </summary>
        public VDM_Information()
        {
            NumberOfDataObjects = 0;
            VDM_Data_Object_1 = new VDM_Data_Object_1();
            VDM_Data_Object_2 = new VDM_Data_Object_2();
            VDM_Data_Object_3 = new VDM_Data_Object_3();
            VDM_Data_Object_4 = new VDM_Data_Object_4();
            VDM_Data_Object_5 = new VDM_Data_Object_5();
            VDM_Data_Object_6 = new VDM_Data_Object_6();
            VDM_Data_Object_7 = new VDM_Data_Object_7();
        }
        #endregion
    }
}
