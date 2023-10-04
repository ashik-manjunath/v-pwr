namespace VseriesControllerLibrary.VDM_Message
{
    public class SinkCapability
    {

        #region Private properties 
        private const int PDOCountLimit = 7;
        private List<uint> m_SnkPowerDataObject;
        #endregion

        #region Public properties 
        public List<KeyValuePair<string, string>> Header { get; set; }
        public List<Dictionary<string, string>> PDO { get; set; }
        #endregion

        #region Constructor 
        /// <summary>
        /// Sink Capability Constructor.
        /// </summary>
        public SinkCapability()
        {
            m_SnkPowerDataObject = new List<uint>();
            Header = new List<KeyValuePair<string, string>>();
            PDO = new List<Dictionary<string, string>>();
        }
        #endregion

        #region Public Module

        /// <summary>
        /// To Get Byte Data.
        /// </summary>
        /// <returns> returns the PDO Payload along with buffer. </returns>
        public List<uint> GetByteData()
        {
            return m_SnkPowerDataObject;
        }

        /// <summary>
        /// Clears Object Values
        /// </summary>
        /// <returns> Returns true when cleared. </returns>
        public bool ClearDataObjects()
        {
            m_SnkPowerDataObject.Clear();
            return true;
        }

        /// <summary>
        /// Allows to set Fixed Supply.
        /// </summary>
        /// <param name="Volt_50mV_Unit"> Voltage of 50mV Units. </param>
        /// <param name="OperationalCurrent_10mA_Unit"> Operational Current 10mA units. </param>
        /// <param name="FRSwap_USBTypecCurrent"> Fast Role Swap USB Type-C Current. </param>
        /// <param name="enDualRolePower"> Dual Role Power. </param>
        /// <param name="HigherCapability"> Higher Capability. </param>
        /// <param name="externallyPowered"> Externally Powered. </param>
        /// <param name="USBCommCapable"> USB Communications Capability. </param>
        /// <param name="enDualRoleData"> Dual Role Data. </param>
        /// <returns> Returns true. </returns>
        public bool AddFixedSupply(uint Volt_50mV_Unit, uint OperationalCurrent_10mA_Unit, bool enDualRolePower = false, bool externallyPowered = false, bool USBCommCapable = false, bool enDualRoleData = false)
        {
            if (m_SnkPowerDataObject.Count >= PDOCountLimit)
                return false;
            uint uiData = OperationalCurrent_10mA_Unit;
            uiData |= (Volt_50mV_Unit << 10);

            if (enDualRoleData)
                uiData |= (1 << 25);
            if (USBCommCapable)
                uiData |= (1 << 26);
            if (externallyPowered)
                uiData |= (1 << 27);
            if (enDualRolePower)
                uiData |= (1 << 29);

            m_SnkPowerDataObject.Add(uiData);

            return true;
        }

        /// <summary>
        /// Allows to set Variable Supply.
        /// </summary>
        /// <param name="MaxVolt_50mV_Unit"> Maximum Voltage of 50mV units. </param>
        /// <param name="MinVolt_50mV_Unit"> Minimum Voltage of 50mV units. </param>
        /// <param name="OperationalCurrent_10mA_Unit"> Operational Current of 10mA units. </param>
        /// <returns> Returns True. </returns>
        public bool AddVariableSupply(uint MaxVolt_50mV_Unit, uint MinVolt_50mV_Unit, uint OperationalCurrent_10mA_Unit)
        {
            if (m_SnkPowerDataObject.Count >= PDOCountLimit)
                return false;
            uint uiData = OperationalCurrent_10mA_Unit;
            uiData |= (MinVolt_50mV_Unit << 10);
            uiData |= (MaxVolt_50mV_Unit << 20);
            uiData |= ((uint)0x2 << 30);
            m_SnkPowerDataObject.Add(uiData);

            return true;
        }

        /// <summary>
        /// Allows to set Battery Supply.
        /// </summary>
        /// <param name="MaxVolt_50mV_Unit"> Maximum Voltage of 50mV units. </param>
        /// <param name="MinVolt_50mV_Unit"> Minimum Voltage of 50mV units. </param>
        /// <param name="OperationalPower_250mW_Unit"> Operational Power of 250mW units. </param>
        /// <returns> Returns true. </returns>
        public bool AddBatterySupply(uint MaxVolt_50mV_Unit, uint MinVolt_50mV_Unit, uint OperationalPower_250mW_Unit)
        {
            if (m_SnkPowerDataObject.Count >= PDOCountLimit)
                return false;
            uint uiData = OperationalPower_250mW_Unit;
            uiData |= (MinVolt_50mV_Unit << 10);
            uiData |= (MaxVolt_50mV_Unit << 20);
            uiData |= (1 << 30);
            m_SnkPowerDataObject.Add(uiData);
            return true;
        }

        /// <summary>
        /// Allows to set ppsApdo
        /// </summary>
        /// <param name="MaxVolt_100mV_Unit"> Maximum Voltage of 100mV units. </param>
        /// <param name="MinVolt_100mV_Unit"> Minimum Voltage of 100mV units. </param>
        /// <param name="MaxCurrent_50mA_Unit"> Maximum Current of 50mA units. </param>
        /// <returns> Returns true. </returns>
        public bool AddPpsApdo(uint MaxVolt_100mV_Unit, uint MinVolt_100mV_Unit, uint MaxCurrent_50mA_Unit)
        {
            if (m_SnkPowerDataObject.Count >= PDOCountLimit)
                return false;
            uint uiData = MaxCurrent_50mA_Unit;
            uiData |= (MinVolt_100mV_Unit << 8);
            uiData |= (MaxVolt_100mV_Unit << 17);
            uiData |= ((uint)0x3 << 30);
            m_SnkPowerDataObject.Add(uiData);
            return true;
        }

        /// <summary>
        /// This function will add the raw PDO data 
        /// </summary>
        /// <param name="data"></param>
        public void AddPdoRawData(uint data)
        {
            m_SnkPowerDataObject.Add(data);
        }

        #endregion

    }
}
