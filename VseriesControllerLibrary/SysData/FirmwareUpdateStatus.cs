namespace VseriesControllerLibrary_V1.SysData
{
    internal class FirmwareUpdateStatus
    {
        #region Public Properties 

        public byte CodeWord { get; set; } = 0x00;


        /// <summary>
        /// This holds the value if the write data was success or not ; If this is 0x01 then Write is success else false
        /// </summary>
        public byte WriteStatus { get; set; } = 0x00;


        /// <summary>
        /// this holds the address mismatch error ; If this is 0x01 then the error occured else no.
        /// </summary>
        public byte AddressMissmatchError { get; set; } = 0x00;

        /// <summary>
        /// this holds the flash write verification error ; If this is 0x01 then the error occured else no.
        /// </summary>
        public byte FlashWriteVerificationError { get; set; } = 0x00;

        /// <summary>
        /// This holds if the provided FW file is invalid ; If this is 0x01 then the error occured else no.
        /// </summary>
        public byte KeyError { get; set; } = 0x00;


        /// <summary>
        /// This holds the value of the address miss match value.
        /// </summary>
        public byte AddressMismatchValue { get; set; } = 0x00;

        /// <summary>
        /// This holds the provided the value of iteration where flash write verification error heppened.
        /// </summary>
        public byte FlashWriteVerificationErrorValue  { get; set; } = 0x00;

        /// <summary>
        /// This holds provides the value of iteration where key error heppened
        /// </summary>
        public byte KeyErrorHappenedValue { get; set; } = 0x00;

        #endregion

        #region Default Constructor 

        public FirmwareUpdateStatus()
        {

        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            return $"Code Word  - {CodeWord}" +
        $"\nWrite Status - {WriteStatus}" +
        $"\nAddress Missmatch - {AddressMissmatchError}" +
        $"\nFlash Write Status - {FlashWriteVerificationError}" +
        $"\nFirmware File Check - {KeyError}" +
        $"\nAddress Mismatch Value - {AddressMismatchValue}" +
        $"\nIteration Mismatch Value - {FlashWriteVerificationErrorValue}" +
        $"\nIteration Flash Write Value - {KeyErrorHappenedValue} ";

        }
        #endregion

    }
}
