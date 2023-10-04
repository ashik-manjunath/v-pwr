namespace VseriesControllerLibrary_V1
{
    /// <summary>
    /// Controller Manufacturing location 
    /// </summary>
    internal enum ManufacturingLocation
    {
        OTHER,
        INDIA,
        TAIWAN,
        USA
    }

    /// <summary>
    /// FRAM ID's
    /// </summary>
    internal enum ID
    {
        #region FRAM Start
        ID,
        CHECK,
        LENGTH,
        BLOCK_START,
        BLOCK_ID,
        DILIMTER,
        RESERVED,
        NONE,
        #endregion

        #region Block 0 - System Details
        FRAM_REV,
        SYSTEM_SERIAL_NUMBER,
        CC_SERIAL_NUMBER,
        CC_BOARD_REV,
        BP_SERIAL_NUMBER,
        BP_BOARD_REV,
        IDN,
        LICENSE,
        Usage,
        #endregion

        #region Block 1 - Manufacturing Details
        Manufacturing_month,
        Manufacturing_year,
        Location_of_Manufacturing,
        #endregion

        #region Block 2 - Calibration Details
        Calibration_Date,
        Calibration_Month,
        Calibration_Year,
        Next_Calibration_Date,
        Next_Calibration_Month,
        Next_Calibration_Year,
        NAME,
        #endregion

        #region Block 3 - Temporary_License
        TEMP_LICENSE,
        License_Start_Date,
        License_Start_Month,
        License_Start_Year,
        License_End_Date,
        License_End_Month,
        License_End_Year,
        License_LastRun_Date,
        License_LastRun_Month,
        License_LastRun_Year,
        #endregion

        #region Block 4 Eload Rework information 
        FC_Board_Rework1,
        FC_Board_Rework2,
        FC_Board_Rework3,
        FC_Board_Rework4,
        #endregion

        #region Block 5 Cable IR Drop cablibration
        IR_DROP_En_Dis,
        VBUS_IR_Drop,
        CC1_IR_Drop,
        CC2_IR_Drop,
        #endregion

        #region Block ADC Bank 1 Calibration
        ADC_BANK,
        CHANNEL_NO,
        ADC_UNIT,
        VOLTAGE1,
        ADC_COUNT,
        VOLTAGE2,
        VOLTAGE3,
        VOLTAGE4,
        VOLTAGE5,
        VOLTAGE6,
        VOLTAGE7,
        VOLTAGE8,
        VOLTAGE9,
        VOLTAGE10,
        CURRENT1,
        CURRENT2,
        CURRENT3,
        CURRENT4,
        CURRENT5,
        CURRENT6,
        CURRENT7,
        CURRENT8,
        CURRENT9,
        CURRENT10,
        #endregion

    }

    /// <summary>
    /// FRAM Blocks
    /// </summary>
    public enum Blocks
    {
        NONE = -1,
        Block_0 = 0,
        Block_1 = 1,
        Block_2 = 2,
        Block_3 = 3,
        Block_4 = 4,
        Block_5 = 5,
        Block_6 = 6,
        Block_7 = 7,
        Block_8 = 8,
        Block_9 = 9,
        Block_10 = 10,
        Block_11 = 11,
        Block_12 = 12,
        Block_13 = 13,
        Block_14 = 14,
        Block_15 = 15,
        Block_16 = 16,
        Block_17 = 17,
        Block_18 = 18,
    }

   

    public enum ControlCardSheetRev
    {
        NONE = 0,
        
        // COntrol card revision 1 length is 109
        Rev1 =109,
    }

    public enum TesterCardSheetRev
    {
        NONE = 0,

        // COntrol card revision 1 length is 109
        Rev1 = 109,
    }



    public enum Byte
    {
        One = 1,
        Two = 2,
    }

    public enum InputData
    {
        File,
        String,
    }
}