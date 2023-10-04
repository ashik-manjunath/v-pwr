namespace VseriesControllerLibrary_V1.UnStructure_VDM_Message
{
    /// <summary>
    /// this class holds the Unstructure VDM Data Object 1
    /// </summary>
    internal class Unstructure_VDM_Data_Object_1
    {

        #region public properties 

        public int AvaliableVendorUse { get; private set; }

        public byte VDM_Type { get; private set; }

        public int VendorID { get; private set; }

        public int RawValue { get; private set; }

        #endregion

        #region Constructor 
        public Unstructure_VDM_Data_Object_1()
        {
            AvaliableVendorUse = 0;
            VDM_Type = 0;
            VendorID = 0;
        }
        #endregion

        #region Public Members 

        public static byte[] GetByteValues(int avaliable_Vendor_Use, int vendorID, int vdmType)
        {
            int value = (vendorID << 16) |
               (vdmType << 15) |
               avaliable_Vendor_Use;

            return BitConverter.GetBytes(value);
        }

        public static Unstructure_VDM_Data_Object_1  DecodeByteValue(byte[] byteValue)
        {
            int dataObjectValue = HelperModule.GetIntFromByteArray(byteValue);         
            return new Unstructure_VDM_Data_Object_1()
            {
                AvaliableVendorUse = dataObjectValue & 0x7FFF,
                VDM_Type = (byte)((dataObjectValue >> 15) & 0x1),
                VendorID = (dataObjectValue >> 16) & 0xFFFF,
            };
        }

       
        
        #endregion

    }
}
