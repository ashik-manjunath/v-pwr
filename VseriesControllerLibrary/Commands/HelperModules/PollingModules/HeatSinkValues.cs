namespace VseriesControllerLibrary_V1.Commands.HelperModules.PollingModules
{
    public class HeatSinkValues
    {
        #region Public Members 

        /// <summary>
        /// this holds the one of the temprature values 
        /// </summary>
        public int TempratureValue1 { get; set; }

        /// <summary>
        /// this holds the one of the temprature values 
        /// </summary>
        public int TempratureValue2 { get; set; }

        /// <summary>
        /// this holds the one of the temprature values 
        /// </summary>
        public int TempratureValue3 { get; set; }

        /// <summary>
        /// this holds the max temperature of the unit
        /// </summary>
        public int MaxTemperature { get; set; }

        /// <summary>
        /// this holds if the heat is over the limit or under the limit
        /// </summary>
        public Temperature TemperatureStatus { get; set; }

        #endregion

        #region Constructor 
        /// <summary>
        /// Constructor
        /// </summary>
        public HeatSinkValues()
        {
            TempratureValue1 = 0;
            TempratureValue2 = 0;
            TempratureValue3 = 0;
            MaxTemperature = 0;
            TemperatureStatus = Temperature.DefaultRange;
        }
        #endregion
    }
}
