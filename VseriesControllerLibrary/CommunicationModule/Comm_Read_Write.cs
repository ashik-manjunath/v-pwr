
using VseriesControllerLibrary_V1.CommunicationModule.Ethernet;

namespace VseriesControllerLibrary_V1.CommunicationModule
{
    internal class Comm_Read_Write
    {

        #region Private properties 
        private bool isRuntimeCommand = false;
        private bool isPollingCommand = false;
        #endregion

        #region Public Properties 

        public SocketConnection SocketConnection { get; set; }
        //public UART_Link_Comm UartConnection { get; set; }
        //public USBLinkComm USBLinkComm { get; set; }

        #endregion

        #region Constructor 
        public Comm_Read_Write()
        {
            SocketConnection = new SocketConnection();
            //USBLinkComm = new USBLinkComm();
            //UartConnection = new UART_Link_Comm();
        }
        #endregion

        #region Public Module 
        public bool Write(byte[] buffer, string command, byte reqCode = 0x01, bool isRead = false, Communication_Phy communication_Phy = Communication_Phy.Eth)
        {
            bool retValue = false;
            if (HelperModule.AppType == ApplicationType.V_TE)
            {
                do { } while (isPollingCommand);
                isRuntimeCommand = true;

                if (communication_Phy == Communication_Phy.Eth)
                {
                    retValue = SocketConnection.Write(buffer, command);
                }
                else if (communication_Phy == Communication_Phy.UART)
                {
                    //retValue = UartConnection.Write(buffer, 0, buffer.Length, command);
                }

                isRuntimeCommand = false;
            }
            else if (HelperModule.AppType == ApplicationType.V_UP)
            {
                //USBLinkComm.WriteFWCommand(buffer, command, reqCode, isRead);
            }

            return retValue;
        }
        public bool Write(ref byte[] buffer, ref int buffCnt, uint timeOut = 10)
        {
            return false;
            //return USBLinkComm.WriteFWCommand(ref buffer, ref buffCnt, timeOut);
        }
        public bool Read(ref byte[] buffer, string command, Communication_Phy communication_Phy = Communication_Phy.Eth)
        {
            bool retValue = false;
            if (HelperModule.AppType == ApplicationType.V_TE)
            {
                do { } while (isPollingCommand);
                isRuntimeCommand = true;

                if (communication_Phy == Communication_Phy.Eth)
                {
                    retValue = SocketConnection.Read(ref buffer, command);
                }
                else if (communication_Phy == Communication_Phy.UART)
                {
                    //retValue = UartConnection.Write(buffer, 0, buffer.Length, command);
                }


                isRuntimeCommand = false;
            }
            else if (HelperModule.AppType == ApplicationType.V_UP)
            {
                //USBLinkComm.ReadFWCommand(ref buffer, command);
            }

            return retValue;
        }
        public bool Read(ref byte[] buffer, ref int buffCnt, uint timeOut = 10)
        {
            return false;
            //return USBLinkComm.ReadFWCommand(ref buffer, ref buffCnt, timeOut);
        }
        public bool GetPollingData(ref byte[] buffer, out bool runTimeCommand)
        {
            bool retValue = false;
            runTimeCommand = false;
            if (HelperModule.AppType == ApplicationType.V_TE)
            {
                if (!isRuntimeCommand)
                {
                    isPollingCommand = true;
                    retValue = SocketConnection.Read(ref buffer, "Polling");
                    isPollingCommand = false;
                }
                else
                {
                    runTimeCommand = isRuntimeCommand;
                }
            }
            else if (HelperModule.AppType == ApplicationType.V_UP)
            {
                //retValue = USBLinkComm.ReadFWCommand(ref buffer);
            }
            return retValue;
        }

        #endregion

    }
}
