namespace VseriesControllerLibrary_V1
{
    /// <summary>
    /// This class contains loop-back information
    /// </summary>
    public class LoopBackInfo
    {
        #region Public Members 
        /// <summary>
        /// holds the loop back status information
        /// </summary>
        public Loopbackstatus LoopbackstatusInfo { get; }

        /// <summary>
        /// holds the link speed
        /// </summary>
        public LinkSpeed Speed { get; }
        #endregion

        #region Default constructor 
        public LoopBackInfo(Loopbackstatus loopbackstatus = Loopbackstatus.NotActive, LinkSpeed speed = LinkSpeed.NotConnected)
        {
            LoopbackstatusInfo = loopbackstatus;
            Speed = speed;
        }
        #endregion


    }

}
