namespace VseriesControllerLibrary_V1
{
    public class VendorDefinedMessage
    {
        private const int PDOCountLimit = 7;
        private readonly List<uint> vdmPowerDataObject; //changed the access modifier to private from public

        //created List to update Header and PDO details
        public List<KeyValuePair<string, string>> Header = new List<KeyValuePair<string, string>>();
        public List<Dictionary<string, string>> PDO = new List<Dictionary<string, string>>();

        /// <summary>
        /// Get Byte data.
        /// </summary>
        /// <returns>  returns the PDO Payload along with buffer. </returns>
        public List<uint> GetByteData()
        {
            return vdmPowerDataObject;
        }

        /// <summary>
        /// Clears Data Object.
        /// </summary>
        /// <returns> returns true. </returns>
        public bool ClearDataObjects()
        {
            vdmPowerDataObject.Clear();
            return true;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public VendorDefinedMessage()
        {
            vdmPowerDataObject = new List<uint>();
        }

        /// <summary>
        /// Allows to set Unstructured VDM.
        /// </summary>
        /// <param name="Vendor_ID"> Vendor ID. </param>
        /// <param name="Vendor_Use"> Vendor Use. </param>
        /// <param name="VDM_Type"> VDM Type. </param>
        /// <returns> On success returns true. </returns>
        public bool AddUnstructuredVDM(uint Vendor_ID, uint Vendor_Use, bool VDM_Type = false)
        {
            if (vdmPowerDataObject.Count >= PDOCountLimit)
                return false;
            uint uiData = Vendor_Use & 0x7FFF;
            uiData |= ((Vendor_ID & 0xFFFF) << 16);

            if (VDM_Type)
                uiData |= (1 << 15);

            vdmPowerDataObject.Add(uiData);
            return true;
        }

        /// <summary>
        /// Allows to set "Message Header"
        /// </summary>
        /// <param name="messageType">configure message type</param>
        /// <param name="specificRevision">Specfic rivision</param>
        /// <param name="cablePlug">Cable plug</param>
        /// <param name="messageID">Message ID</param>
        /// <param name="numberOfDataObj">Number of data object</param>
        /// <returns></returns>
        public bool AddMessageHeader(uint messageType, uint specificRevision, uint cablePlug, uint messageID, uint numberOfDataObj)
        {
            if (vdmPowerDataObject.Count >= PDOCountLimit)
                return false;

            uint uiData = messageType;
            uiData |= ((specificRevision & 0x3) << 6);
            uiData |= ((cablePlug & 0x1) << 8);
            uiData |= ((messageID & 0x7) << 9);
            uiData |= ((numberOfDataObj & 0x7) << 12);
            vdmPowerDataObject.Add(uiData);
            return true;
        }

        /// <summary>
        /// Allows to set Structed VDM (Data Object 1).
        /// </summary>
        /// <param name="Command"> Commands being used.
        /// 0 = Reserved, Shall NotShall Not be used
        /// 1 = Discover Identity
        /// 2 = Discover SVIDs
        /// 3 = Discover Modes
        /// 4 = Enter Mode
        /// 5 = Exit Mode
        /// 6 = Attention
        /// 16…31 = SVID Specific Commands
        /// </param>
        /// <param name="Command_Type"> Type of Command.
        /// 00b = REQ (Request from Initiator Port)
        /// 01b = ACK(Acknowledge Response from Responder Port)
        /// 10b = NAK(Negative Acknowledge Response fromResponder Port)
        /// 11b = BUSY(Busy Response from Responder Port)
        /// </param>
        /// <param name="Object_Position"> For the Enter Mode, Exit Mode and Attention Commands.</param>
        /// <param name="Struc_VDM_Ver"> Structured VDM Version.
        /// Version Number of the Structured VDM .
        /// Version 1.0 = 00b
        /// Version 2.0 = 01b
        /// Reserved = 10b and 11b
        /// </param>
        /// <param name="SVID"> Unique 16-bit unsigned integer (0xFF01 for Displayport alternate mode) </param>
        /// <param name="VDM_Type">
        /// 1 = Structured VDM.
        /// 0 = UnStructured VDM.
        /// </param>
        /// <returns> On Success Returns True. </returns>
        public bool AddStructuredVDMHeader(uint Command, uint Command_Type, uint Object_Position, uint VDM_Version, uint SVID, uint VDM_Type = 1)
        {
            if (vdmPowerDataObject.Count >= PDOCountLimit)
                return false;
            uint uiData = Command;
            uiData |= ((Command_Type & 0x3) << 6);
            uiData |= ((Object_Position & 0x7) << 8);
            uiData |= ((VDM_Version & 0x3) << 13);
            uiData |= ((VDM_Type & 0x1) << 15);
            uiData |= ((SVID & 0xFFFF) << 16);

            vdmPowerDataObject.Add(uiData);
            return true;
        }

        /// <summary>
        /// Allows to set ID Header VDO (Data Object 2).
        /// </summary>
        /// <param name="USB_VID">  USB Vendor ID. </param>
        /// <param name="ProductType_DPF"> Product Type DPF.
        /// 0 – Undefined
        /// 1 – PDUSB Hub
        /// 2 – PDUSB Host
        /// 3 – Power Brick
        /// 4 - Alternate Mode Controller(AMC)
        /// </param>
        /// <param name="ProductType_UPF"> Product Type UPF.
        /// 0 – Undefined
        /// 1 – PDUSB Hub
        /// 2 – PDUSB Peripheral
        /// 3… – PSD
        /// 5 – Alternate Mode Adapter(AMA)
        ///
        /// Product Type (Cable Plug):
        /// 0 – Undefined
        /// 1…2 – Reserved, Shall NotShall Not be used.
        /// 3 – Passive Cable
        /// 4 – Active Cable
        /// </param>
        /// <param name="ModalOprSup"> Modal Operation Support.
        /// To be set 1 if it supports Modal Operation Support else zero.</param>
        /// <param name="USBCommCap_USBDev"> USB Communication Capable USB Device.
        /// To be set one if product can enumerate as device else zero. </param>
        /// <param name="USBCommCap_USBHost"> USB Communication Capable USB Host.
        /// To be set one if product can enumerate as Host else zero.</param>
        /// <returns> On Success Returns true. </returns>
        public bool AddID_HeaderVDO(uint USB_VID, uint ProductType_DPF, uint ProductType_UPF, bool ModalOprSup = false, bool USBCommCap_USBDev = false, bool USBCommCap_USBHost = false)
        {
            if (vdmPowerDataObject.Count >= PDOCountLimit)
                return false;
            uint uiData = USB_VID;
            uiData |= (ProductType_DPF << 23);

            if (ModalOprSup)
                uiData |= (1 << 26);

            uiData |= (ProductType_UPF << 27);

            if (USBCommCap_USBDev)
                uiData |= (1 << 30);

            if (USBCommCap_USBHost)
                uiData |= ((uint)1 << 31);

            vdmPowerDataObject.Add(uiData);
            return true;
        }

        /// <summary>
        /// Allows to set Cert Stt VDO (Data Object 3.
        /// </summary>
        /// <param name="XID"> XID assigned by USBIF. </param>
        /// <returns> onsuccess returns true. </returns>
        public bool AddCertStatVDO(uint XID)
        {
            if (vdmPowerDataObject.Count >= PDOCountLimit)
                return false;
            uint uiData = XID;

            vdmPowerDataObject.Add(uiData);
            return true;
        }

        /// <summary>
        /// Allows to Product VDO. (Data Object 4)
        /// </summary>
        /// <param name="bcdDevice"> Device Version Number relevent to the release. </param>
        /// <param name="USB_ProductID"> USB Product ID. </param>
        /// <returns> on success returns true. </returns>
        public bool AddProductVDO(uint bcdDevice, uint USB_ProductID)
        {
            if (vdmPowerDataObject.Count >= PDOCountLimit)
                return false;
            uint uiData = bcdDevice;
            uiData |= (USB_ProductID << 16);

            vdmPowerDataObject.Add(uiData);
            return true;
        }

        /// <summary>
        /// Allows to Product VDO2 (Data Object 5)
        /// </summary>
        /// <param name="USB_SS_SIGNALING"></param>
        /// <param name="VBUS_CURRENT_CAP"> 
        /// 00b = Reserved
        /// 01b = 3A
        /// 10b = 5A
        /// 11b = Reserved
        /// </param>
        /// <param name="MAX_VBUS_VOLT"></param>
        /// <param name="Cable_Term_Type"></param>
        /// <param name="Cable_Latency"></param>
        /// <param name="USBTypeCPlug_to_USBTypeCCaptive"></param>
        /// <param name="VDO_Version"></param>
        /// <param name="Firmware_Version"></param>
        /// <param name="HW_Version"></param>
        /// <returns></returns>
        public bool Passive_Cable_VDO(uint USB_SS_SIGNALING, uint VBUS_CURRENT_CAP, uint MAX_VBUS_VOLT, uint Cable_Term_Type, uint Cable_Latency, uint USBTypeCPlug_to_USBTypeCCaptive, uint VDO_Version, uint Firmware_Version, uint HW_Version)
        {
            if (vdmPowerDataObject.Count >= PDOCountLimit)
                return false;
            uint uiData = USB_SS_SIGNALING;
            uiData |= (VBUS_CURRENT_CAP << 5);
            uiData |= (MAX_VBUS_VOLT << 9);
            uiData |= (Cable_Term_Type << 11);
            uiData |= (Cable_Latency << 13);
            uiData |= (USBTypeCPlug_to_USBTypeCCaptive << 18);
            uiData |= (VDO_Version << 21);
            uiData |= (Firmware_Version << 24);
            uiData |= (HW_Version << 28);

            vdmPowerDataObject.Add(uiData);
            return true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="USB_SS_SIGNALING"></param>
        /// <param name="VBUS_CURRENT_CAP"></param>
        /// <param name="MAX_VBUS_VOLT"></param>
        /// <param name="Cable_Term_Type"></param>
        /// <param name="Cable_Latency"></param>
        /// <param name="Connector_Type"></param>
        /// <param name="VDO_Version"></param>
        /// <param name="Firmware_Version"></param>
        /// <param name="HW_Version"></param>
        /// <param name="SOP_Controller_Present"></param>
        /// <param name="VBUS_Through_Cable"></param>
        /// <param name="SBU_Type"></param>
        /// <param name="SBU_Supported"></param>
        /// <returns></returns>
        public bool Active_Cable_VDO1(uint USB_SS_SIGNALING, uint VBUS_CURRENT_CAP, uint MAX_VBUS_VOLT, uint Cable_Term_Type, uint Cable_Latency, uint Connector_Type, uint VDO_Version, uint Firmware_Version, uint HW_Version, bool SOP_Controller_Present = false, bool VBUS_Through_Cable = false, bool SBU_Type = false, bool SBU_Supported = false)
        {
            if (vdmPowerDataObject.Count >= PDOCountLimit)
                return false;
            uint uiData = USB_SS_SIGNALING;
            if (SOP_Controller_Present)
                uiData |= (1 << 3);

            if (VBUS_Through_Cable)
                uiData |= (1 << 4);

            uiData |= (VBUS_CURRENT_CAP << 5);

            if (SBU_Type)
                uiData |= (1 << 7);

            if (SBU_Supported)
                uiData |= (1 << 8);

            uiData |= (MAX_VBUS_VOLT << 9);
            uiData |= (Cable_Term_Type << 11);
            uiData |= (Cable_Latency << 13);
            uiData |= (Connector_Type << 18);
            uiData |= (VDO_Version << 21);
            uiData |= (Firmware_Version << 24);
            uiData |= (HW_Version << 28);

            vdmPowerDataObject.Add(uiData);
            return true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="SS_SIGNALING"></param>
        /// <param name="USB2_0_HubHopsConsumed"></param>
        /// <param name="U3_Power"></param>
        /// <param name="Shutdown_Temp"></param>
        /// <param name="Max_Operating_Temp"></param>
        /// <param name="SS_Lanes_Supported"></param>
        /// <param name="SS_Supported"></param>
        /// <param name="USB_2_0_Supported"></param>
        /// <param name="U3_to_U0_Transition_Mode"></param>
        /// <returns></returns>
        public bool Active_Cable_VDO2(uint SS_SIGNALING, uint USB2_0_HubHopsConsumed, uint U3_Power, uint Shutdown_Temp, uint Max_Operating_Temp, bool SS_Lanes_Supported = false, bool SS_Supported = false, bool USB_2_0_Supported = false, bool U3_to_U0_Transition_Mode = false)
        {
            if (vdmPowerDataObject.Count >= PDOCountLimit)
                return false;
            uint uiData = SS_SIGNALING;
            if (SS_Lanes_Supported)
                uiData |= (1 << 3);

            if (SS_Supported)
                uiData |= (1 << 4);

            if (USB_2_0_Supported)
                uiData |= (1 << 5);

            uiData |= (USB2_0_HubHopsConsumed << 6);

            if (U3_to_U0_Transition_Mode)
                uiData |= (1 << 11);

            uiData |= (U3_Power << 12);
            uiData |= (Shutdown_Temp << 16);
            uiData |= (Max_Operating_Temp << 24);

            vdmPowerDataObject.Add(uiData);
            return true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="USB_SS_SIGNALING"></param>
        /// <param name="VCONN_Power"></param>
        /// <param name="VDO_Version"></param>
        /// <param name="Firmware_Version"></param>
        /// <param name="HW_Version"></param>
        /// <param name="VBUS_Required"></param>
        /// <param name="VCONN_Required"></param>
        /// <returns></returns>
        public bool AMA_VDO(uint USB_SS_SIGNALING, uint VCONN_Power, uint VDO_Version, uint Firmware_Version, uint HW_Version, bool VBUS_Required = false, bool VCONN_Required = false)
        {
            if (vdmPowerDataObject.Count >= PDOCountLimit)
                return false;
            uint uiData = USB_SS_SIGNALING;
            if (VBUS_Required)
                uiData |= (1 << 3);

            if (VCONN_Required)
                uiData |= (1 << 4);

            uiData |= (VCONN_Power << 5);
            uiData |= (VDO_Version << 21);
            uiData |= (Firmware_Version << 24);
            uiData |= (HW_Version << 28);

            vdmPowerDataObject.Add(uiData);
            return true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="Gnd_Impedance"></param>
        /// <param name="VBUS_Impedance"></param>
        /// <param name="Max_VBUS_Voltage"></param>
        /// <param name="VDO_Version"></param>
        /// <param name="Firmware_Version"></param>
        /// <param name="HW_Version"></param>
        /// <param name="Chagre_Through_Support"></param>
        /// <returns></returns>
        public bool VPD_VDO(uint Gnd_Impedance, uint VBUS_Impedance, uint Max_VBUS_Voltage, uint VDO_Version, uint Firmware_Version, uint HW_Version, bool Chagre_Through_Support = false)
        {
            if (vdmPowerDataObject.Count >= PDOCountLimit)
                return false;

            uint uiData = 0x0;

            if (Chagre_Through_Support)
                uiData |= (1 << 0);

            uiData |= (Gnd_Impedance << 1);
            uiData |= (VBUS_Impedance << 7);
            uiData |= (Max_VBUS_Voltage << 15);
            uiData |= (VDO_Version << 21);
            uiData |= (Firmware_Version << 24);
            uiData |= (HW_Version << 28);

            vdmPowerDataObject.Add(uiData);
            return true;
        }

        /// <summary>
        /// Add required SVID values into Discover SVID response message configuration
        /// </summary>
        /// <param name="SVID_n_plus_1"></param>
        /// <param name="SVID_n"></param>
        /// <returns></returns>
        public bool Discover_SVID_Responder_VDO(uint SVID_n_plus_1, uint SVID_n)
        {
            if (vdmPowerDataObject.Count >= PDOCountLimit)
                return false;
            uint uiData = SVID_n_plus_1;
            uiData |= (SVID_n << 16);
            vdmPowerDataObject.Add(uiData);
            return true;
        }

        /// <summary>
        /// Add required SVID values into Discover SVID response message configuration
        /// </summary>
        /// <param name="SVID_n_plus_1"></param>
        /// <param name="SVID_n"></param>
        /// <returns></returns>
        public bool Discover_Mode_Responder_VDO(uint ModeValue)
        {
            if (vdmPowerDataObject.Count >= PDOCountLimit)
                return false;

            vdmPowerDataObject.Add(ModeValue);
            return true;
        }

        /// <summary>
        /// This API allows user to add custom VDO data into payload
        /// </summary>
        /// <param name="data"></param>
        public void AddPdoRawData(uint data)
        {
            vdmPowerDataObject.Add(data);
        }


    }

}
