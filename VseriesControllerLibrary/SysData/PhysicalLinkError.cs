namespace VseriesControllerLibrary_V1
{
    public class PhysicalLinkError
    {
        #region Public Members

        /// <summary>
        /// holds the value of physical error count 
        /// </summary>
        public int PhysicalError { get; set; }

        /// <summary>
        /// holds the value of total physical error count 
        /// </summary>
        public int Total_PhysicalError { get; set; }

        /// <summary>
        /// holds the value of link error count 
        /// </summary>
        public int LinkError { get; set; }

        /// <summary>
        /// holds the value of total link error count
        /// </summary>
        public int Total_LinkError { get; set; }

        /// <summary>
        /// holds the vlaue of iteration count 
        /// </summary>
        public int IterationCount {get ; set;}

        /// <summary>
        /// holds the value of present USB 2.0 error count 
        /// </summary>
        public int Present_USB_2p0 { get; set; }

        /// <summary>
        /// holds the value of total USB 2.0 error count 
        /// </summary>
        public int Total_USB_2p0 { get; set; }
        #endregion

        #region Default Constructor 
        /// <summary>
        /// Constructor
        /// </summary>
        public PhysicalLinkError()
        {
            PhysicalError = 0;
            Total_PhysicalError = 0;
            LinkError = 0;
            Total_LinkError = 0;
            IterationCount = 0;
            Present_USB_2p0 = 0;
            Total_USB_2p0 = 0;
        }

        #endregion
    }
}
