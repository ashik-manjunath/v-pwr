namespace VseriesControllerLibrary_V1.VDM_Message
{
    /// <summary>
    /// this class creates the VDM Data Object 2
    /// </summary>
    internal class VDM_Data_Object_2
    {
        #region Public Properties

        /// <summary>
        /// holds the USB Vendor ID
        /// </summary>
        public int USBVendorID { get; private set; }

        /// <summary>
        /// holds the reserved byte value
        /// </summary>
        public byte Reseerved { get; private set; }

        /// <summary>
        /// holds the Product Type(DFP)
        /// </summary>
        public byte ProductType_DFP { get; private set; }

        /// <summary>
        /// holds the Modal operation supported
        /// </summary>
        public byte Modal_Operation_Supported { get; private set; }

        /// <summary>
        /// hodls the Product Type(UFP)
        /// </summary>
        public byte ProductType_UFP { get; private set; }

        /// <summary>
        /// holds the USB Commnunication capables as a USB Device
        /// </summary>
        public byte USB_Comm_USB_Device { get; private set; }

        /// <summary>
        /// holds the USB Commnunication capables as a USB Host
        /// </summary>
        public byte USB_Comm_USB_Host { get; private set; }

        #endregion

        #region Constructor 

        /// <summary>
        /// Default Constructor
        /// </summary>
        public VDM_Data_Object_2()
        {
            USBVendorID = 0;
            Reseerved = 0;
            ProductType_DFP = 0;
            Modal_Operation_Supported = 0;
            ProductType_UFP = 0;
            USB_Comm_USB_Device = 0;
            USB_Comm_USB_Host = 0;
        }
        #endregion

        #region Public Members 

        public static byte[] GetByteValue(int standardORVerdorID, byte vdmType, byte structureVDMVersion, byte reserved_Byte_11_12,
            byte objectPosition, byte commandType, byte reserved_Byte_5, byte command)
        {

            int value = (standardORVerdorID << 16) |
            (vdmType << 15) |
            (structureVDMVersion << 13) |
            (reserved_Byte_11_12 << 11) |
            (objectPosition << 8) |
            (commandType << 6) |
            (reserved_Byte_5 << 5) |
            command;

            return BitConverter.GetBytes(value);
        }

        public static VDM_Data_Object_2 DecodeByteValue(byte[] byteValue)
        {
            int dataObjectValue = HelperModule.GetIntFromByteArray(byteValue);
            return new VDM_Data_Object_2()
            {
                USBVendorID = (dataObjectValue & 0xFFFF),
                Reseerved = (byte)((dataObjectValue >> 16) & 0x7F),
                ProductType_DFP = (byte)((dataObjectValue >> 23) & 0x7),
                Modal_Operation_Supported = (byte)((dataObjectValue >> 26) & 0x1),
                ProductType_UFP = (byte)((dataObjectValue >> 27) & 0x7),
                USB_Comm_USB_Device = (byte)((dataObjectValue >> 30) & 0x1),
                USB_Comm_USB_Host = (byte)((dataObjectValue >> 31) & 0x1),
            };
        }

        #endregion

    }
}
