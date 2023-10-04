namespace VseriesControllerLibrary_V1.SysData
{
    internal class CableIRDrop
    {

        #region Public Properties
        /// <summary>
        /// This property holds weather IR drop value present or not 
        /// </summary>
        public int IDDropExistes { get; set; }

        /// <summary>
        /// This property holds VBUS IR Drop value 
        /// </summary>
        public int IRDropVBUS { get; set; }

        /// <summary>
        /// This property holds CC2 IR Drop value 
        /// </summary>
        public int IRDropCC2 { get; set; }

        /// <summary>
        /// This property holds CC1 IR Drop value 
        /// </summary>
        public int IRDropCC1 { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor
        /// </summary>
        public CableIRDrop()
        {
        }
        #endregion
    }

}