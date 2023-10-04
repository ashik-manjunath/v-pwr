using System;
using System.IO;

namespace VseriesControllerLibrary_V1 {
    internal enum DebugType { DEBUG, STATUS };

    internal class DebugLogger {
        #region Private Members
        private bool m_LogWritingInProgress = false;
        private StreamWriter m_DebugLogWriter;
        private string m_DebugFileNameDefault;
        private static DebugLogger instance = null;
        private static readonly object padlock = new object();
        private static readonly object padlockWrite = new object();
        private const string MAIN_DIRECTORY = @"\GRL\";
        private const string PROJECT_DIRECTORY = @"GRL-VDPWR\";
        private const string PROJECT_DEBUGLOG_DIRECTORY = @"AppData\";
        private const string DEBUGLOG_FILE_NAME = @"DebugLogger.log";
        private const string DEBUGLOG_FILE_NAME_COPY = @"CopyDebugLogger.log";
        private const string HOMEDRIVE = "HOMEDRIVE";

        #endregion

        #region Public Memebrs
        public string DebugLogFile { get; set; } = "";
        public DebugType DebugMode { get; set; }
        #endregion

        private DebugLogger() {

        }
        public static DebugLogger Instance {
            get {
                if (instance == null) {
                    lock (padlock) {
                        if (instance == null) {
                            instance = new DebugLogger();
                        }
                    }
                }
                return instance;
            }
        }


        public void Create() {
            string SystemDrive = Environment.GetEnvironmentVariable(HOMEDRIVE);
            if (SystemDrive.Length != 2) {
                SystemDrive = "C:";
            }
            string strTempFilePath = SystemDrive + MAIN_DIRECTORY;
            try {
                if (Directory.Exists(strTempFilePath) == false) {
                    Directory.CreateDirectory(strTempFilePath);
                }
                if (Directory.Exists(strTempFilePath + PROJECT_DIRECTORY) == false) {
                    Directory.CreateDirectory(strTempFilePath + PROJECT_DIRECTORY);
                }
                strTempFilePath += PROJECT_DIRECTORY;
                if (Directory.Exists(strTempFilePath + PROJECT_DEBUGLOG_DIRECTORY) == false) {
                    Directory.CreateDirectory(strTempFilePath + PROJECT_DEBUGLOG_DIRECTORY);
                }
                strTempFilePath += PROJECT_DEBUGLOG_DIRECTORY;
                if (File.Exists(strTempFilePath + DEBUGLOG_FILE_NAME) == true) {
                    var file = File.Open(strTempFilePath + DEBUGLOG_FILE_NAME, FileMode.Open, FileAccess.Read);
                    var fileSize = file.Length;
                    file.Close();
                    if (fileSize < 1000000) {
                        if (File.Exists(strTempFilePath + DEBUGLOG_FILE_NAME_COPY))
                            File.Delete(strTempFilePath + DEBUGLOG_FILE_NAME_COPY);
                        File.Copy(strTempFilePath + DEBUGLOG_FILE_NAME, strTempFilePath + DEBUGLOG_FILE_NAME_COPY);
                    }

                    File.Delete(strTempFilePath + DEBUGLOG_FILE_NAME);

                }
            }
            catch (Exception ex) {
                ex.ToString();
            }
            m_DebugFileNameDefault = strTempFilePath + DEBUGLOG_FILE_NAME;
        }
        public void DebugLogFilePath(string filepath) {
            try {
                if (filepath == "") {
                    filepath = m_DebugFileNameDefault;
                }
                else {
                    string strDir = Path.GetDirectoryName(filepath);
                    string strfilenameOnly = Path.GetFileName(filepath);
                    if (Directory.Exists(strDir)) {
                        if (Path.GetFileName(filepath) == "") {
                            filepath = filepath + DEBUGLOG_FILE_NAME;
                        }
                    }
                    else {
                        Directory.CreateDirectory(strDir);
                    }
                }
                DebugLogFile = filepath;
                WriteToDebugLogger(DebugType.STATUS, "USBPD_TesterLib instance started at" + DateTime.Now.ToString());
            }
            catch (Exception ex) {
                Console.WriteLine(ex.ToString());
                //WriteToDebugLogger(DebugType.DEBUG, "DebugLogger - DebugLogFilePath() :", ex);
            }
        }
        public void WriteToDebugLogger(DebugType objMode, string str, Exception exp = null) {
            lock (padlockWrite) {
                try {
                    if (DebugLogFile == "") {
                        DebugLogFile = m_DebugFileNameDefault;
                    }
                    else {
                        if (File.Exists(DebugLogFile) == false) {
                            DebugLogFile = m_DebugFileNameDefault;
                        }
                    }

                    if (m_LogWritingInProgress == true) {
                        int timer = 0;
                        do {
                            //Thread.Sleep(2);
                            timer++;
                            if (timer > 100) {
                                return;
                            }

                        } while (m_LogWritingInProgress == true);
                    }
                    m_LogWritingInProgress = true;
                    try {
                        bool writeLog = false;
                        if (DebugMode == DebugType.DEBUG) {
                            writeLog = true;
                        }
                        else if (objMode == DebugType.STATUS) {
                            writeLog = true;
                        }

                        if (writeLog == true) {
                            string excep = "";
                            if (exp != null) {
                                excep = exp.Message + " \n" + exp.StackTrace + " \n" + exp.Source + " \n" +
                                               exp.TargetSite.ToString() + "\n" + exp.ToString();
                            }
                            m_DebugLogWriter = new StreamWriter(DebugLogFile, true);
                            m_DebugLogWriter.WriteLine($"{DateTime.Now:HH:mm:ss.fff} : {str} \n {excep}");
                            m_DebugLogWriter.Close();
                        }
                    }
                    catch (Exception ex) {
                        m_LogWritingInProgress = false;
                        ex.ToString();
                    }
                    m_LogWritingInProgress = false;

                }
                catch (Exception ex) {
                    Console.Write(ex.ToString());
                }
            }
        }

    }

}
