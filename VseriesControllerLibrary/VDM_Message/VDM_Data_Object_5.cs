namespace VseriesControllerLibrary_V1.VDM_Message
{
    /// <summary>
    /// this class creates the VDM Data Object 2
    /// </summary>
    internal class VDM_Data_Object_5
    {
        #region Public Properties 

        /// <summary>
        /// this holds the command value 0..4
        /// </summary>
        public byte Command { get; set; }

        /// <summary>
        /// this holds the Reserved value at byte 5
        /// </summary>
        public byte Reserved_Byte_5 { get; set; }

        /// <summary>
        /// this holds the Command type value 6..7
        /// </summary>
        public byte CommandType { get; set; }

        /// <summary>
        /// this holds the Object Position 8..10
        /// </summary>
        public byte ObjectPosition { get; set; }

        /// <summary>
        /// this holds the Reserved value at bytes 11..12
        /// </summary>
        public byte Reserved_Byte_11_12 { get; set; }

        /// <summary>
        /// this holds the Structure VDM Version 13..14
        /// </summary>
        public byte StructureVDMVersion { get; set; }

        /// <summary>
        /// this holds the VDM Type 15
        /// </summary>
        public byte VDMType { get; set; }

        /// <summary>
        /// this holds the Standard or VerdorID 16..31
        /// </summary>
        public int StandardORVerdorID { get; set; }
        #endregion

        #region Constructor 
        /// <summary>
        /// default constructor
        /// </summary>
        public VDM_Data_Object_5()
        {
            Command = 0;
            Reserved_Byte_5 = 0;
            CommandType = 0;
            ObjectPosition = 0;
            Reserved_Byte_11_12 = 0;
            StructureVDMVersion = 0;
            VDMType = 0;
            StandardORVerdorID = 0;

        }
        #endregion

    }
}
