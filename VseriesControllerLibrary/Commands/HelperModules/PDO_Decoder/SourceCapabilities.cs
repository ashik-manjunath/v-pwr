using System.Collections.Generic;

namespace VseriesControllerLibrary_V1.HelperModules.PDO_Decoder {
    /// <summary>
    /// Source capabilities fetch all the source Capabilities 
    /// </summary>
    public class SourceCapabilities
    {
        #region Public Members
        
        /// <summary>
        /// PDO list 
        /// </summary>
        public List<PDO> PDOlist { get; set; }

        /// <summary>
        /// Port number 
        /// </summary>
        public PortID Port { get; set; }
        #endregion

        #region Constructor 

        /// <summary>
        /// Default Constructor 
        /// </summary>
        public SourceCapabilities()
        {
            Port = PortID.NONE;
            PDOlist = new List<PDO>();
        }
        #endregion
    }
}
