namespace VseriesControllerLibrary_V1.SysData
{
    internal class FCReworkInformation
    {
        #region Public Properties

        /// <summary>
        /// This property holds the FC board rework information location 1
        /// </summary>
        public int FC_Board_Rework1 { get; set; }

        /// <summary>
        /// This property holds the FC board rework information location 2
        /// </summary>
        public int FC_Board_Rework2 { get; set; }

        /// <summary>
        /// This property holds the FC board rework information location 3
        /// </summary>
        public int FC_Board_Rework3 { get; set; }

        /// <summary>
        /// This property holds the FC board rework information location 4
        /// </summary>
        public int FC_Board_Rework4 { get; set; }
        
        #endregion

        #region Constructor
        /// <summary>
        /// Default Constructor
        /// </summary>
        public FCReworkInformation()
        {

        }
        #endregion
    }
}