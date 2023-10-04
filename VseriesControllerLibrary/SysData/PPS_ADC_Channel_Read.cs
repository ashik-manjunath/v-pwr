namespace VseriesControllerLibrary_V1
{
    public class PPS_ADC_Channel_Read
    {
        #region Public Properties

        /// <summary>
        /// This holds the respective channels
        /// </summary>
        public ChannelRead ADC { get; set; }
        public ChannelRead Current { get; set; }
        public ChannelRead DAC { get; set; }

        #endregion

        #region Constructor 
        /// <summary>
        /// Constructor
        /// </summary>
        public PPS_ADC_Channel_Read()
        {
            ADC = new ChannelRead();
            Current = new ChannelRead();
            DAC = new ChannelRead();
        }
        #endregion
    }
}
