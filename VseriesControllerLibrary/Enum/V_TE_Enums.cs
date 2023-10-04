namespace VseriesControllerLibrary_V1
{
    internal enum TI_Program_Config
    {
        BootMode = 0x01,
        ProgramMode = 0x02,
        Write = 0x09,
        CM_Reset = 0x04,
        Erase = 0x0A,a
    }

    internal enum TI_Firmware_Config
    {
        ConnectivityManager_CMCore = 0x00,
        ControlCard_CPU1 = 0x01,
        PPS_CPU2 = 0x02,
    }

}
