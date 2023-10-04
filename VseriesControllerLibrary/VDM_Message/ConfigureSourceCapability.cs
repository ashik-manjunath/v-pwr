namespace VseriesControllerLibrary_V1
{
    /// <summary>
    /// SourceCapability class which contains all fields required for the PDSource Capability function. 
    /// </summary>
    [Serializable()]
    public class ConfigureSourceCapability
    {
        const int PDOCountLimit = 7;

        private List<uint> m_SrcPowerDataObject;
        public List<KeyValuePair<string, string>> Header = new List<KeyValuePair<string, string>>();
        public List<Dictionary<string, string>> PDO = new List<Dictionary<string, string>>();

        /// <summary>
        /// Source Capability Constructor.
        /// </summary>
        public ConfigureSourceCapability()
        {
            m_SrcPowerDataObject = new List<uint>();
        }

        /// <summary>
        /// To Get Byte Data.
        /// </summary>
        /// <returns> returns the PDO Payload along with buffer. </returns>
        public List<uint> GetByteData()
        {
            return m_SrcPowerDataObject; // returns the PDO Payload along with buffer.
        }

        /// <summary>
        /// Clears Object Values
        /// </summary>
        /// <returns> Returns true when cleared. </returns>
        public bool ClearDataObjects()
        {
            m_SrcPowerDataObject.Clear();
            return true;
        }

        /// <summary>
        /// Allows to set Fixed Supply.
        /// </summary>
        /// <param name="volt_mV_Unit"> Voltage in mV units. </param>
        /// <param name="maxCurrent_mA_Unit"> Maximum Current in mA units. </param>
        /// <param name="enDualRolePower"> Dual Role Power. </param>
        /// <param name="supportUSBSuspend"> USB Suspended Support. </param>
        /// <param name="externallyPowered"> Unconstrained Power. </param>
        /// <param name="USBCommCapable"> USB Communications Cable. </param>
        /// <param name="enDualRoleData"> Dual Role Data. </param>
        /// <param name="supUnChunkedExt"> non-chunked Extended Messages Supported. </param>
        /// <param name="peakCurrent"> Peak Current. </param>
        /// <returns> On Success Returns True. </returns>
        public bool AddFixedSupply(uint volt_mV_Unit, uint maxCurrent_mA_Unit, bool enDualRolePower = false,
            bool supportUSBSuspend = false, bool externallyPowered = false, bool USBCommCapable = false, bool enDualRoleData = false,
            bool supUnChunkedExt = false, PeakCurrentCapability peakCurrent = PeakCurrentCapability.Default_Ioc)
        {

            uint volt_50mV_Unit = volt_mV_Unit / 50;
            uint maxCurrent_10mA_Unit = maxCurrent_mA_Unit / 10;

            if (m_SrcPowerDataObject != null)
            {

                if (m_SrcPowerDataObject.Count >= PDOCountLimit)
                    return false;
            }
            else
                m_SrcPowerDataObject = new List<uint>();
            uint uiPkCur = (uint)peakCurrent;

            uint uiData = maxCurrent_10mA_Unit;
            uiData |= (volt_50mV_Unit << 10);
            uiData |= (uiPkCur << 20);

            if (supUnChunkedExt)
                uiData |= (1 << 24);
            if (enDualRoleData)
                uiData |= (1 << 25);
            if (USBCommCapable)
                uiData |= (1 << 26);
            if (externallyPowered)
                uiData |= (1 << 27);
            if (supportUSBSuspend)
                uiData |= (1 << 28);
            if (enDualRolePower)
                uiData |= (1 << 29);

            m_SrcPowerDataObject.Add(uiData);
            return true;
        }

        /// <summary>
        /// Allows to set Variable supply.
        /// </summary>
        /// <param name="maxVolt_mV_Unit"> Maximum Voltage in mV units. </param>
        /// <param name="minVolt_mV_Unit"> Minimum Voltage in mV units. </param>
        /// <param name="maxCurrent_mA_Unit"> Maximum Current in mA units. </param>
        /// <returns> Returns True if Success.</returns>
        public bool AddVariableSupply(uint maxVolt_mV_Unit, uint minVolt_mV_Unit, uint maxCurrent_mA_Unit)
        {

            uint maxVolt_50mV_Unit = maxVolt_mV_Unit / 50;
            uint minVolt_50mV_Unit = minVolt_mV_Unit / 50;
            uint maxCurrent_10mA_Unit = maxCurrent_mA_Unit / 10;

            if (m_SrcPowerDataObject.Count >= PDOCountLimit)
                return false;
            uint uiData = maxCurrent_10mA_Unit;
            uiData |= (minVolt_50mV_Unit << 10);
            uiData |= (maxVolt_50mV_Unit << 20);
            uiData |= ((uint)0x2 << 30);
            m_SrcPowerDataObject.Add(uiData);

            return true;
        }

        /// <summary>
        /// Allows to set Battery Supply.
        /// </summary>
        /// <param name="maxVolt_mV_Unit"> Maximum Voltage in mV units. </param>
        /// <param name="minVolt_mV_Unit"> Minimum Voltage in mV units. </param>
        /// <param name="maxPower_mW_Unit"> Maximum Power in mW units. </param>
        /// <returns> Returns true. </returns>
        public bool AddBatterySupply(uint maxVolt_mV_Unit, uint minVolt_mV_Unit, uint maxPower_mW_Unit)
        {

            uint maxVolt_50mV_Unit = maxVolt_mV_Unit / 50;
            uint minVolt_50mV_Unit = minVolt_mV_Unit / 50;
            uint maxPower_250mW_Unit = maxPower_mW_Unit / 250;


            if (m_SrcPowerDataObject.Count >= PDOCountLimit)
                return false;
            uint uiData = maxPower_250mW_Unit;
            uiData |= (minVolt_50mV_Unit << 10);
            uiData |= (maxVolt_50mV_Unit << 20);
            uiData |= (1 << 30);
            m_SrcPowerDataObject.Add(uiData);
            return true;
        }

        /// <summary>
        /// Allows to set ppsApdo.
        /// </summary>
        /// <param name="maxVolt_mV_Unit"> Maximum Voltage in mV units. </param>
        /// <param name="minVolt_mV_Unit"> Minimum Voltage in mV units. </param>
        /// <param name="maxCurrent_mA_Unit"> Maximum Current in mA units. </param>
        /// <param name="pps_Power_Limited"> PPS power limited </param>
        /// <returns> Returns True. </returns>
        public bool AddPpsApdo(uint maxVolt_mV_Unit, uint minVolt_mV_Unit, uint maxCurrent_mA_Unit, bool pps_Power_Limited)
        {

            uint maxVolt_100mV_Unit = maxVolt_mV_Unit / 100;
            uint minVolt_100mV_Unit = minVolt_mV_Unit / 100;
            uint maxCurrent_50mA_Unit = maxCurrent_mA_Unit / 50;

            if (m_SrcPowerDataObject.Count >= PDOCountLimit)
                return false;
            uint uiData = maxCurrent_50mA_Unit;
            uiData |= ((minVolt_100mV_Unit & 0xFF) << 8);
            uiData |= ((maxVolt_100mV_Unit & 0xFF) << 17);
            if (pps_Power_Limited)
                uiData |= (1 << 27);
            uiData |= ((uint)0x3 << 30);
            m_SrcPowerDataObject.Add(uiData);
            return true;
        }

        public void AddPdoRawData(uint data)
        {
            m_SrcPowerDataObject.Add(data);
        }

        /// <summary>
        /// Get PDO count
        /// </summary>
        /// <returns></returns>
        public double GetPDOCount()
        {
            if (PDO == null)
                return 0;
            else
                return PDO.Count;
        }
    }
}

