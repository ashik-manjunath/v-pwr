namespace VseriesControllerLibrary_V1.VDM_Message
{
    /// <summary>
    /// this class creates the VDM Data Object 2
    /// </summary>
    internal class VDM_Data_Object_1
    {
        #region Public Properties 

        /// <summary>
        /// this holds the command value 0..4
        /// </summary>
        public byte Command { get; private set; }

        /// <summary>
        /// this holds the Reserved value at byte 5
        /// </summary>
        public byte Reserved_Byte_5 { get; private set; }

        /// <summary>
        /// this holds the Command type value 6..7
        /// </summary>
        public byte CommandType { get; private set; }

        /// <summary>
        /// this holds the Object Position 8..10
        /// </summary>
        public byte ObjectPosition { get; private set; }

        /// <summary>
        /// this holds the Reserved value at bytes 11..12
        /// </summary>
        public byte Reserved_Byte_11_12 { get; private set; }

        /// <summary>
        /// this holds the Structure VDM Version 13..14
        /// </summary>
        public byte StructureVDMVersion { get; private set; }

        /// <summary>
        /// this holds the VDM Type 15
        /// </summary>
        public byte VDMType { get; private set; }

        /// <summary>
        /// this holds the Standard or VerdorID 16..31
        /// </summary>
        public int StandardORVerdorID { get; private set; }
        #endregion

        #region Constructor 
        /// <summary>
        /// default constructor
        /// </summary>
        public VDM_Data_Object_1()
        {
            Command = 0;
            Reserved_Byte_5 = 0;
            CommandType = 0;
            ObjectPosition = 0;
            Reserved_Byte_11_12 = 0;
            StructureVDMVersion = 0;
            VDMType = 0;
            StandardORVerdorID = 0;

        }

        /// <summary>
        /// this decode the property values to byte array 
        /// </summary>
        /// <param name="standardORVerdorID">Standard OR VerdorID </param>
        /// <param name="vdmType">VDM type</param>
        /// <param name="structureVDMVersion">Structure VDM version</param>
        /// <param name="reserved_Byte_11_12">Reserved byte value</param>
        /// <param name="objectPosition">Object position</param>
        /// <param name="commandType">Command type</param>
        /// <param name="reserved_Byte_5">Reserved byte</param>
        /// <param name="command">Command</param>
        /// <returns></returns>
        public static byte[] GetByteValue(int standardORVerdorID, byte vdmType, byte structureVDMVersion, byte reserved_Byte_11_12, byte objectPosition, byte commandType,
            byte reserved_Byte_5, byte command)
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

        /// <summary>
        /// this will decode the byte values and ssigen to respective properties 
        /// </summary>
        /// <param name="byteValue"></param>
        /// <returns></returns>
        public static VDM_Data_Object_1 DecodeByteValue(byte[] byteValue)
        {
            int dataObjectValue = HelperModule.GetIntFromByteArray(byteValue);
            return new VDM_Data_Object_1()
            {
                Command = (byte)(dataObjectValue & 0x1F),
                Reserved_Byte_5 = (byte)((dataObjectValue >> 5) & 0x1),
                CommandType = (byte)((dataObjectValue >> 6) & 0x3),
                ObjectPosition = (byte)((dataObjectValue >> 8) & 0x7),
                Reserved_Byte_11_12 = (byte)((dataObjectValue >> 11) & 0x3),
                StructureVDMVersion = (byte)((dataObjectValue >> 13) & 0x3),
                VDMType = (byte)((dataObjectValue >> 15) & 0x1),
                StandardORVerdorID = ((dataObjectValue >> 16) & 0xFFFF),

            };
        }

        #endregion

    }
}
