namespace VseriesControllerLibrary_V1.SysData
{
    public class DataPowerRole
    {
        #region Public Properties 

        /// <summary>
        /// this contains PD Contract Negotiation happens or not 
        /// </summary>
        public PDContractNegotation PDC_Negotation { get; set; }

        /// <summary>
        /// Power role type sink , source , Dual  
        /// </summary>
        public PowerRoleType PowerRole { get; set; }

        /// <summary>
        /// Data role type UFP , DFP, DRP
        /// </summary>
        public DataRoleType DataRole { get; set; }


        /// <summary>
        /// This holds the power role set expcetly 
        /// </summary>
        public IntenalPowerRoleType IntenalPowerRole { get; set; }
        /// <summary>
        /// This holds the execution command was succesfull or not
        /// </summary>
        public bool ReturnValue { get; set; }

        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor
        /// </summary>
        public DataPowerRole()
        {
            PDC_Negotation = PDContractNegotation.Failed;
            PowerRole = PowerRoleType.Sink;
            DataRole = DataRoleType.UFP;
            IntenalPowerRole = IntenalPowerRoleType.Dual;
            ReturnValue = false;
        }
        #endregion


        #region Public Module 

        public override string ToString()
        {
            return $"PDC Negotation : {PDC_Negotation}" +
                $" | Power Role : {PowerRole}" +
                $" | Data Role : {DataRole}" +
                $" | Set Power Role : {IntenalPowerRole}";

        }

        #endregion

    }
}
