namespace VseriesControllerLibrary_V1.VDM_Message
{
    /// <summary>
    /// this class creates the VDM Messgae Header
    /// </summary>
    internal class VDM_Message_Header
    {

        #region Public Properties 

        /// <summary>
        /// this holds the messgae type 
        /// </summary>
        public byte MessageType { get; set; }

        /// <summary>
        /// this holds the reserved value at byte 4
        /// </summary>
        public byte Reserved_Byte4 { get; set; }

        /// <summary>
        /// this holds the port data role
        /// </summary>
        public byte PortDataRole { get; set; }

        /// <summary>
        /// this holds the Specification Revision
        /// </summary>
        public byte SpecificationRevision { get; set; }

        /// <summary>
        /// this holds the Port Power Role
        /// </summary>
        public byte PortPowerRole { get; set; }

        /// <summary>
        /// this holds the MessageID
        /// </summary>
        public byte MessageID { get; set; }

        /// <summary>
        /// this holds the Number Of Data Object 
        /// </summary>
        public byte NumberOfDataObject { get; set; }

        /// <summary>
        /// this holds the Reserved value at Byte15 
        /// </summary>
        public byte Reserved_Byte15 { get; set; }


        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor
        /// </summary>
        public VDM_Message_Header()
        {
            MessageType = 0;
            Reserved_Byte4 = 0;
            PortDataRole = 0;
            SpecificationRevision = 0;
            PortPowerRole = 0;
            MessageID = 0;
            NumberOfDataObject = 0;
            Reserved_Byte15 = 0;
        }

        #endregion
    }
}
