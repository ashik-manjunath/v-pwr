namespace VseriesControllerLibrary_V1
{
    public class ConfigureSinkCapability
    {

        #region Private properties 
        private const int PDOCountLimit = 7;
        private readonly List<uint> m_SnkPowerDataObject;
        #endregion

        #region Public properties 
        public List<KeyValuePair<string, string>> Header { get; set; }
        public List<Dictionary<string, string>> PDO { get; set; }
        #endregion

        #region Constructor 
        /// <summary>
        /// Sink Capability Constructor.
        /// </summary>
        public ConfigureSinkCapability()
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
        /// <param name="volt_mV_Unit"> Voltage in mV Units. </param>
        /// <param name="operationalCurrent_mA_Unit"> Operational Current in mA units. </param>
        /// <param name="enDualRolePower"> Dual Role Power. </param>
        /// <param name="externallyPowered"> Externally Powered. </param>
        /// <param name="USBCommCapable"> USB Communications Capability. </param>
        /// <param name="enDualRoleData"> Dual Role Data. </param>
        /// <returns> Returns true. </returns>
        public bool AddFixedSupply(uint volt_mV_Unit, uint operationalCurrent_mA_Unit, bool enDualRolePower = false, bool externallyPowered = false, bool USBCommCapable = false, bool enDualRoleData = false)
        {
            uint volt_50mV_Unit = volt_mV_Unit / 50;
            uint operationalCurrent_10mA_Unit = operationalCurrent_mA_Unit / 10;

            if (m_SnkPowerDataObject.Count >= PDOCountLimit)
                return false;
            uint uiData = operationalCurrent_10mA_Unit;
            uiData |= (volt_50mV_Unit << 10);

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
        /// <param name="maxVolt_mV_Unit"> Maximum Voltage of in mV units. </param>
        /// <param name="minVolt_mV_Unit"> Minimum Voltage of in mV units. </param>
        /// <param name="operationalCurrent_mA_Unit"> Operational Current in mA units. </param>
        /// <returns> Returns True. </returns>
        public bool AddVariableSupply(uint maxVolt_mV_Unit, uint minVolt_mV_Unit, uint operationalCurrent_mA_Unit)
        {
            uint maxVolt_50mV_Unit = maxVolt_mV_Unit / 50;
            uint minVolt_50mV_Unit = minVolt_mV_Unit / 50;
            uint operationalCurrent_10mA_Unit = operationalCurrent_mA_Unit / 10;

            if (m_SnkPowerDataObject.Count >= PDOCountLimit)
                return false;
            uint uiData = operationalCurrent_10mA_Unit;
            uiData |= (minVolt_50mV_Unit << 10);
            uiData |= (maxVolt_50mV_Unit << 20);
            uiData |= ((uint)0x2 << 30);
            m_SnkPowerDataObject.Add(uiData);

            return true;
        }

        /// <summary>
        /// Allows to set Battery Supply.
        /// </summary>
        /// <param name="maxVolt_mV_Unit"> Maximum Voltage of 50mV units. </param>
        /// <param name="minVolt_mV_Unit"> Minimum Voltage of 50mV units. </param>
        /// <param name="operationalPower_mW_Unit "> Operational Power of mW units. </param>
        /// <returns> Returns true. </returns>
        public bool AddBatterySupply(uint maxVolt_mV_Unit, uint minVolt_mV_Unit, uint operationalPower_mW_Unit)
        {

            uint maxVolt_50mV_Unit = maxVolt_mV_Unit / 50;
            uint minVolt_50mV_Unit = minVolt_mV_Unit / 50;
            uint operationalPower_250mW_Unit = operationalPower_mW_Unit / 250;

            if (m_SnkPowerDataObject.Count >= PDOCountLimit)
                return false;
            uint uiData = operationalPower_250mW_Unit;
            uiData |= (minVolt_50mV_Unit << 10);
            uiData |= (maxVolt_50mV_Unit << 20);
            uiData |= (1 << 30);
            m_SnkPowerDataObject.Add(uiData);
            return true;
        }

        /// <summary>
        /// Allows to set ppsApdo
        /// </summary>
        /// <param name="maxVolt_mV_Unit"> Maximum Voltage in mV units. </param>
        /// <param name="minVolt_mV_Unit"> Minimum Voltage in mV units. </param>
        /// <param name="maxCurrent_mA_Unit"> Maximum Current in mA units. </param>
        /// <returns> Returns true. </returns>
        public bool AddPpsApdo(uint maxVolt_mV_Unit, uint minVolt_mV_Unit, uint maxCurrent_mA_Unit)
        {
            uint maxVolt_100mV_Unit = maxVolt_mV_Unit / 100;
            uint minVolt_100mV_Unit = minVolt_mV_Unit / 100;
            uint maxCurrent_50mA_Unit = maxCurrent_mA_Unit / 50;
            if (m_SnkPowerDataObject.Count >= PDOCountLimit)
                return false;
            uint uiData = maxCurrent_50mA_Unit;
            uiData |= (minVolt_100mV_Unit << 8);
            uiData |= (maxVolt_100mV_Unit << 17);
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
