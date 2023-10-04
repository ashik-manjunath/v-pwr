using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Reflection;
using System.Text;

namespace VseriesControllerLibrary_V1.CommunicationModule.Ethernet
{
    internal class SocketConnection
    {
        #region Private properties 
        private readonly object methodLock = new object();
        private string m_ethIP;
        private string m_ethName;
        private TcpClient tcpClient = null;
        private NetworkStream tcpStream = null;
        private readonly int readTimeOut = 500;
        private readonly int writeTimeOut = 500;
        private int previousIndex = 16;
        private int presentIndex = 17;
        private readonly int bytesReadWriteLength = 1024;
        private DateTime _resettimer = DateTime.Now;

        #endregion

        #region Public Properties
        public bool IsOpen = false;
        public string IpAddress { get; private set; }
        /// <summary>
        /// This holds true when the connection check is in progress dll will be trying to reconnect to the hardware
        /// </summary>
        public bool IsRetry { get; private set; }
        #endregion

        #region Constructor
        public SocketConnection()
        {
        }
        #endregion

        #region Public Module
        public bool Open(string ipAddress = "192.168.0.4")
        {
            try
            {
                int writePort = 5002;
                IsOpen = false;
                EthernetDiscovery(out m_ethIP, out string m_ethDns, out m_ethName);
                ServicePointManager.Expect100Continue = false;
                IpAddress = ipAddress;
                if (!PingHost())
                    return false;

                if ((m_ethIP == IpAddress))
                {
                    string[] addArray = m_ethIP.Split('.');
                    int.TryParse(addArray[addArray.Length - 1], out int Lastnumber);
                    Lastnumber += 1;
                    addArray[addArray.Length - 1] = Lastnumber.ToString();
                    m_ethIP = addArray[addArray.Length - 4] + "." + addArray[addArray.Length - 3] + "." + addArray[addArray.Length - 2] + "." + addArray[addArray.Length - 1];
                    SetIP("/c netsh interface ip set address \"" + m_ethName + "\" static " + m_ethIP);
                }

                tcpClient = new TcpClient(IpAddress, writePort)
                {
                    NoDelay = true,
                    SendTimeout = writeTimeOut,
                    ReceiveTimeout = readTimeOut,
                };
                tcpStream = tcpClient.GetStream();

                IsOpen = tcpClient.Connected;
            }
            catch (Exception ex)
            {
                HelperModule.Debug(MethodBase.GetCurrentMethod().Name, ex);
                IsOpen = false;
            }
            return IsOpen;
        }
        public bool Close()
        {
            return Dispose();
        }
        public bool Write(byte[] buffer, string command)
        {

            bool isProgramingCommand = CheckIsFirmwareCommand(ref buffer, false);
            if (command == "CONTROLLER_RESET")
                isProgramingCommand = false;

            try
            {
                lock (methodLock)
                {
                    byte[] dataBuffer = new byte[bytesReadWriteLength];
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        dataBuffer[i] = buffer[i];
                    }
                    if (tcpClient != null)
                    {
                        if (tcpClient.Connected)
                        {
                            if (tcpStream.CanWrite)
                            {
                                tcpStream.Write(dataBuffer, 0, dataBuffer.Length);
                                HelperModule.Debug(dataBuffer, $"{IpAddress} : Write :" + command);
                            }
                            else
                            {
                                if (CheckConnection())
                                {
                                    return Write_Retry(buffer, "Command retry");
                                }
                                else
                                {
                                    HelperModule.Debug($" {IpAddress} : connection falied ");
                                    return false;
                                }
                            }

                            // Check eco back if it is write command or firmware update commands 
                            if (isProgramingCommand)
                            {
                                dataBuffer = new byte[1024];
                                List<byte> tempValue = new List<byte>();
                                try
                                {
                                    do
                                    {
                                        if (tcpStream.CanRead)
                                            if (tcpStream.CanRead)
                                            {
                                                int numberOfBytesRead = tcpStream.Read(dataBuffer, 0, dataBuffer.Length);
                                                tempValue.AddRange(dataBuffer);
                                            }
                                    }
                                    while (tcpStream.DataAvailable);
                                    HelperModule.Debug(tempValue.ToArray(), $"{IpAddress} : Ecoback");
                                }
                                catch (Exception ex)
                                {
                                    HelperModule.Debug($"{IpAddress} : TCP client is closed trying to reconnect", ex);
                                    if (CheckConnection())
                                    {
                                        return Write_Retry(buffer, "Command retry");
                                    }
                                    else
                                    {
                                        HelperModule.Debug($" {IpAddress} : connection falied ");
                                        return false;
                                    }
                                }
                                if (!ValidateBytes(buffer, tempValue.ToArray()))
                                {
                                    if (CheckConnection())
                                    {
                                        return Write_Retry(buffer, "Command retry");
                                    }
                                    else
                                    {
                                        HelperModule.Debug($" {IpAddress} : connection falied ");
                                        return false;
                                    }
                                }
                                else
                                {
                                    previousIndex = (int)((buffer[0] >> 4) & 0xF);
                                }

                            }
                        }
                        else
                        {
                            HelperModule.Debug($"{IpAddress} : TCP client is closed trying to reconnect");
                            if (CheckConnection())
                            {
                                return Write_Retry(buffer, "Command retry");
                            }
                            else
                            {
                                HelperModule.Debug($" {IpAddress} : connection falied ");
                                return false;
                            }
                        }
                    }
                    else
                    {
                        HelperModule.Debug($"{IpAddress} : TCP client is null please connect again..");
                        if (CheckConnection())
                        {
                            return Write_Retry(buffer, "Command retry");
                        }
                        else
                        {
                            HelperModule.Debug($" {IpAddress} : connection falied ");
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HelperModule.Debug(MethodBase.GetCurrentMethod().Name, ex);
            }

            ResetTimer();
            return true;
        }
        public bool Read(ref byte[] buffer, string command)
        {
            try
            {
                lock (methodLock)
                {
                    byte[] inCommingBuffer = new byte[bytesReadWriteLength];
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        inCommingBuffer[i] = buffer[i];
                    }
                    List<byte> tempValue = new List<byte>();
                    if (tcpClient != null)
                    {
                        if (tcpClient.Connected)
                        {
                            try
                            {
                                if (tcpStream.CanWrite)
                                {
                                    tcpStream.Write(inCommingBuffer, 0, inCommingBuffer.Length);
                                    HelperModule.Debug(inCommingBuffer.ToArray(), $"{IpAddress} : Write :" + command);
                                }
                                else
                                {
                                    HelperModule.Debug(inCommingBuffer.ToArray(), $"{IpAddress} : Write : Failed");
                                    if (CheckConnection())
                                    {
                                        return Read_Retry(ref buffer, "Command retry");
                                    }
                                    else
                                    {
                                        return false;
                                    }

                                }
                                inCommingBuffer = new byte[1024];

                                do
                                {
                                    if (tcpStream.CanRead)
                                    {
                                        int numberOfBytesRead = tcpStream.Read(inCommingBuffer, 0, inCommingBuffer.Length);
                                        tempValue.AddRange(inCommingBuffer);
                                    }
                                }
                                while (tcpStream.DataAvailable);
                                HelperModule.Debug(tempValue.ToArray(), $"{IpAddress} : Read : Length - {tempValue.Count} :" + command);
                            }
                            catch (Exception ex)
                            {
                                HelperModule.Debug($"{IpAddress} : Read runtime command ", ex);
                                if (CheckConnection())
                                {
                                    return Read_Retry(ref buffer, "Command retry");
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            if (inCommingBuffer[0] == 0xAA && inCommingBuffer[1] == 0xAA && inCommingBuffer[2] == 0xAA && inCommingBuffer[3] == 0xAA)
                            {
                                if (CheckConnection())
                                {
                                    if (RS485Reset())
                                    {
                                        return Read_Retry(ref buffer, "Command retry");
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                                else
                                {
                                    return false;
                                }

                            }
                        }
                        else
                        {
                            HelperModule.Debug($"{IpAddress} :TCP client is closed ");
                            if (CheckConnection())
                            {
                                return Read_Retry(ref buffer, "Command retry");
                            }
                            else
                            {
                                return false;
                            }

                        }
                    }
                    else
                    {
                        HelperModule.Debug($"{IpAddress} :TCP client is closed ");
                        if (CheckConnection())
                        {
                            return Read_Retry(ref buffer, "Command retry");
                        }
                        else
                        {
                            return false;
                        }

                    }
                    buffer = new byte[tempValue.Count];
                    buffer = tempValue.ToArray();
                }
            }
            catch (Exception ex)
            {
                HelperModule.Debug($"{IpAddress} : {MethodBase.GetCurrentMethod().Name}", ex);
            }
            ResetTimer();
            return true;
        }
        public bool Dispose()
        {
            try
            {
                if (IsOpen)
                {
                    if (tcpClient != null)
                        tcpClient.Close();
                    if (tcpStream != null)
                        tcpStream.Close();
                }
            }
            catch (Exception ex)
            {
                HelperModule.Debug($"{IpAddress} : {MethodBase.GetCurrentMethod().Name}", ex);
                return false;
            }
            IsOpen = false;
            return true;
        }
        public DateTime GetLastCommadTime()
        {
            return _resettimer;
        }

        #endregion

        #region Private Module 

        /// <summary>
        /// To get current Ethernet config
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="dns"></param>
        /// <param name="nic"></param>
        private void EthernetDiscovery(out string ip, out string dns, out string nic)
        {
            ip = "";
            dns = "";
            nic = "";
            try
            {
                string[] NwDesc = { "TAP", "VMware", "Windows", "Virtual" };  // Adapter types (Description) to be ommited
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet && !NwDesc.Any(ni.Description.Contains))  // check for adapter type and its description
                    {

                        foreach (IPAddress dnsAdress in ni.GetIPProperties().DnsAddresses)
                        {
                            if (dnsAdress.AddressFamily == AddressFamily.InterNetwork)
                            {
                                dns = dnsAdress.ToString();
                            }
                        }
                        foreach (UnicastIPAddressInformation ips in ni.GetIPProperties().UnicastAddresses)
                        {
                            if (ips.Address.AddressFamily == AddressFamily.InterNetwork && !ips.Address.ToString().StartsWith("169")) //to exclude automatic IPS
                            {
                                ip = ips.Address.ToString();
                                nic = ni.Name;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HelperModule.Debug(MethodBase.GetCurrentMethod().Name, ex);

            }
        }

        /// <summary>
        /// To set IP with elevated command prompt
        /// </summary>
        /// <param name="arg"></param>
        private void SetIP(string arg)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("cmd.exe")
                {
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Normal,
                    Verb = "runas",
                    Arguments = arg
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                HelperModule.Debug(MethodBase.GetCurrentMethod().Name, ex);
            }
        }
        private bool CheckIsFirmwareCommand(ref byte[] value, bool isRetry)
        {
            byte bit = (byte)(value[0] & 0xF);
            if (bit == 0x02)
            {
                if (!isRetry)
                {
                    MofidyFirmwareCommand(ref value);
                }
                return true;
            }
            else if (bit == 0x01)
            {

                return true;
            }

            return false;
        }
        private bool ValidateBytes(byte[] dataBuffer, byte[] tempValue)
        {
            if (dataBuffer.Length <= tempValue.Length)
            {
                for (int i = 0; i < dataBuffer.Length; i++)
                {
                    if (tempValue[i] != dataBuffer[i])
                    {
                        if (ValidateBufferData(dataBuffer, tempValue, ref tempValue))
                        {

                            return ValidateBytes(dataBuffer, tempValue);
                        }
                        else
                        {
                            HelperModule.Debug("Unknow packets present");
                            return false;
                        }
                    }
                }
            }
            else
            {


            }

            return true;
        }
        private void MofidyFirmwareCommand(ref byte[] value)
        {
            do
            {
                presentIndex = new Random().Next(0, 15);
                value[0] = (byte)(value[0] & 0xF);
                value[0] = (byte)((presentIndex << 4) | value[0]);

            } while (presentIndex == previousIndex);
        }
        private bool ValidateBufferData(byte[] original, byte[] read, ref byte[] reCodeData)
        {
            try
            {
                int j = 0;
                for (int i = 0; i < read.Length; i++)
                {
                    if (read[i] == original[j])
                    {
                        i++; j++;

                        if (read[i] == original[j])
                        {
                            i++; j++;

                            if (read[i] == original[j])
                            {
                                i++; j++;

                                if (read[i] == original[j])
                                {
                                    i -= 3;

                                    byte[] tmp = new byte[read.Length];
                                    int l = 0;
                                    for (int K = i; K < read.Length; K++)
                                    {
                                        tmp[l] = read[K];
                                        l++;
                                    }

                                    reCodeData = tmp;
                                    return true;
                                }
                            }
                        }
                    }

                    j = 0;
                }
            }
            catch (Exception ex)
            {
                HelperModule.Debug("ValidateBufferData", ex);
            }
            return false;
        }
        private void Sleep(int waitTimeMilli)
        {
            if (waitTimeMilli <= 0)
                waitTimeMilli = 1;

            int i = 0;
            System.Timers.Timer delayTimer = new System.Timers.Timer(waitTimeMilli)
            {
                AutoReset = false //so that it only calls the method once
            };
            delayTimer.Elapsed += (s, args) => i = 1;
            delayTimer.Start();
            while (i == 0) { };
        }
        private bool PingHost()
        {
            try
            {
                Ping pingSender = new Ping();
                PingOptions options = new PingOptions
                {
                    // Use the default Ttl value which is 128,
                    // but change the fragmentation behavior.
                    DontFragment = true
                };

                // Create a buffer of 32 bytes of data to be transmitted.
                string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
                byte[] buffer = Encoding.ASCII.GetBytes(data);
                int timeout = 180;
                PingReply reply = pingSender.Send(IpAddress, timeout, buffer, options);
                if (reply.Status == IPStatus.Success)
                {
                    return true;
                }
                else if (reply.Status == IPStatus.TimedOut)
                {

                    reply = pingSender.Send(IpAddress, timeout, buffer, options);
                    if (reply.Status == IPStatus.Success)
                    {
                        HelperModule.Debug("Ping successfull");
                        return true;
                    }
                    else
                    {

                        reply = pingSender.Send(IpAddress, timeout, buffer, options);
                        if (reply.Status == IPStatus.Success)
                        {
                            HelperModule.Debug("Ping successfull");
                            return true;
                        }
                        else
                        {
                            HelperModule.Debug("Ping failed");
                            return false;
                        }

                    }
                }
                else
                {
                    HelperModule.Debug("Ping failed");
                    return false;
                }
            }
            catch (Exception ex)
            {
                HelperModule.Debug(" Ping failed ", ex);
                // Discard PingExceptions and return false;
            }

            return true;

        }
        private bool Write_Retry(byte[] buffer, string command)
        {

            bool isProgramingCommand = CheckIsFirmwareCommand(ref buffer, true);
            if (command == "CONTROLLER_RESET")
                isProgramingCommand = false;

            try
            {
                lock (methodLock)
                {
                    byte[] dataBuffer = new byte[bytesReadWriteLength];
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        dataBuffer[i] = buffer[i];
                    }
                    if (tcpClient != null)
                    {
                        if (tcpClient.Connected)
                        {
                            if (tcpStream.CanWrite)
                            {
                                tcpStream.Write(dataBuffer, 0, dataBuffer.Length);
                                HelperModule.Debug(dataBuffer, $"{IpAddress} :  Write retry  :" + command);
                            }
                            else
                            {
                                HelperModule.Debug(dataBuffer, $"{IpAddress} : Write retry : Failed ");
                                return false;
                            }

                            // Check eco back if it is write command or firmware update commands 
                            if (isProgramingCommand)
                            {
                                dataBuffer = new byte[1024];
                                List<byte> tempValue = new List<byte>();
                                try
                                {
                                    do
                                    {
                                        if (tcpStream.CanRead)
                                        {
                                            int numberOfBytesRead = tcpStream.Read(dataBuffer, 0, dataBuffer.Length);
                                            tempValue.AddRange(dataBuffer);
                                        }
                                    }
                                    while (tcpStream.DataAvailable);
                                    HelperModule.Debug(tempValue.ToArray(), $"{IpAddress} : Retry  Ecoback");
                                }
                                catch (Exception ex)
                                {
                                    HelperModule.Debug($" {IpAddress} : Retry write runtime command ", ex);
                                    return false;

                                }
                                if (!ValidateBytes(buffer, tempValue.ToArray()))
                                {
                                    HelperModule.Debug($"{IpAddress} : Retry write failed");
                                    return false;
                                }
                                else
                                {
                                    previousIndex = (int)((buffer[0] >> 4) & 0xF);
                                }
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                HelperModule.Debug($"{IpAddress} : {MethodBase.GetCurrentMethod().Name} ", ex);
            }
            return true;
        }
        private bool Read_Retry(ref byte[] buffer, string command)
        {
            try
            {
                byte[] inCommingBuffer = new byte[bytesReadWriteLength];
                for (int i = 0; i < buffer.Length; i++)
                {
                    inCommingBuffer[i] = buffer[i];
                }
                List<byte> tempValue = new List<byte>();
                if (tcpClient != null)
                {
                    if (tcpClient.Connected)
                    {
                        try
                        {
                            if (tcpStream.CanWrite)
                            {
                                tcpStream.Write(inCommingBuffer, 0, inCommingBuffer.Length);
                                HelperModule.Debug(inCommingBuffer.ToArray(), $"{IpAddress} : Write :" + command);
                            }
                            else
                            {
                                HelperModule.Debug(inCommingBuffer.ToArray(), $"{IpAddress} : Write : Failed");
                                HelperModule.AddStatusUpdate("Connection Failed");
                            }
                            inCommingBuffer = new byte[1024];

                            do
                            {
                                if (tcpStream.CanRead)
                                {
                                    int numberOfBytesRead = tcpStream.Read(inCommingBuffer, 0, inCommingBuffer.Length);
                                    tempValue.AddRange(inCommingBuffer);
                                }
                            }
                            while (tcpStream.DataAvailable);
                            HelperModule.Debug(tempValue.ToArray(), $"{IpAddress} : Read : Length - {tempValue.Count} :" + command);
                        }
                        catch (Exception ex)
                        {
                            HelperModule.Debug($"{IpAddress} : Read runtime command ", ex);
                            return false;
                        }
                        if (inCommingBuffer[0] == 0xAA && inCommingBuffer[1] == 0xAA && inCommingBuffer[2] == 0xAA && inCommingBuffer[3] == 0xAA)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    HelperModule.Debug($"{IpAddress} :TCP client is closed ");

                }

                buffer = new byte[tempValue.Count];
                buffer = tempValue.ToArray();
            }
            catch (Exception ex)
            {
                HelperModule.Debug($"{IpAddress} : {MethodBase.GetCurrentMethod().Name}", ex);
            }
            return true;
        }
        private bool CheckConnection()
        {
            return PingHost();
        }
        private void ResetTimer()
        {
            _resettimer = DateTime.Now;
        }
        private bool RS485Reset()
        {
            try
            {
                byte[] dataBuffer = new byte[] { 0x11, 0x02, 0x01, 0x06 };
                dataBuffer[1] = (byte)(dataBuffer.Length - 2);
                bool retVal = Write(dataBuffer, MethodBase.GetCurrentMethod().Name);
                if (retVal)
                {
                    HelperModule.Debug("RS485Reset successfull");
                    return true;
                }
            }
            catch (Exception ex)
            {
                HelperModule.Debug("RS485Reset : ", ex);
            }
            return false;
        }
        #endregion

    }
}
