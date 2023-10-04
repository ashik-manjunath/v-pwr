namespace VseriesControllerLibrary_V1.SysData
{
    /// <summary>
    /// Swing and de-emphasis register read
    /// </summary>
    public class SwingDeEmphasis
    {
        #region public Members 
        /// <summary>
        /// Full speed  Tx De-Emphasis at 3.5 dB in amps 
        /// </summary>
        public int Tx_De_Emphasis_3p5dB_3p0 { get; set; }

        /// <summary>
        /// Full speed  Tx De-Emphasis at 6 dB in amps
        /// </summary>
        public int TX_De_Emphasis_6dB_3p0 { get; set; }

        /// <summary>
        ///  Full speed TX Amplitude (Full Swing Mode) in 10 mv units 
        /// </summary>
        public int Tx_Amp_Full_Swing_3p0 { get; set; }

        /// <summary>
        /// Full speed TX Amplitude (Low Swing Mode) in 10 mv units 
        /// </summary>
        public int Tx_Amp_Low_Swing_3p0 { get; set; }

        /// <summary>
        /// High Speed Driver Pre-emphasis Enabled or disabled
        /// </summary>
        public int Pre_Emphasis_2p0 { get; set; }
        #endregion

        #region Default Constructor 

        /// <summary>
        /// Constructor 
        /// </summary>
        public SwingDeEmphasis()
        {
            Tx_De_Emphasis_3p5dB_3p0 = 0;
            TX_De_Emphasis_6dB_3p0 = 0;
            Tx_Amp_Full_Swing_3p0 = 0;
            Tx_Amp_Low_Swing_3p0 = 0;
            Pre_Emphasis_2p0 = 0;
        }
        #endregion
    }
}
