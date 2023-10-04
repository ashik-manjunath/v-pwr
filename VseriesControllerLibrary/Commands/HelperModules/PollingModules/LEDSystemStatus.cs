using System.Collections.Generic;

namespace VseriesControllerLibrary_V1.Commands.HelperModules.PollingModules
{
    /// <summary>
    /// this class contains the Dictionary list of all 10 ports of LED status , VBU, VCONN data
    /// also Heat sink temperature value
    /// </summary>
    public class LEDSystemStatus
    {
        #region public Properties

        /// <summary>
        /// this holds all 10 ports of LED status , VBU, VCONN data
        /// </summary>
        public Dictionary<PortID, SystemStatus> SystemStatusList { get; set; }

        /// <summary>
        /// this holds the heat sink temperature value
        /// </summary>
        public HeatSinkValues TemperatureValues { get; set; }

        /// <summary>
        /// if polling is not running then this string gets updated
        /// </summary>
        public string Error { get; set; } = "No Error";

        /// <summary>
        /// if there are any error this will return false or else it will return true
        /// </summary>
        public bool ReturnValue { get; set; } = false;
        #endregion

        #region Constructor 
        /// <summary>
        /// default constructor
        /// </summary>
        public LEDSystemStatus()
        {
            SystemStatusList = new Dictionary<PortID, SystemStatus>();
            TemperatureValues = new HeatSinkValues();
        }
        #endregion

        #region Public Module 
        public bool GetIndividualPort(PortID portID, out SystemStatus systemStatus)
        {
            systemStatus = new SystemStatus();
            if (SystemStatusList.Count > 0)
            {
                return SystemStatusList.TryGetValue(portID, out systemStatus);
            }
            return false;
        }
        #endregion
    }
}
