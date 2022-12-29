using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;

namespace Utility
{
    public static class LogManager
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public enum LoggerFileName
        {
            UserManagement = 1,
            PlayBookManagement = 2,
            Utility = 3,
            PlayBookImageMigrationWithDb = 4,
        };

        static void LogManagerConfigure(LoggerFileName logFileName)
        {
            string fileName = String.Empty;
            switch (logFileName)
            {
                case LoggerFileName.UserManagement:
                    fileName = "UserManagement";
                    break;
                case LoggerFileName.PlayBookManagement:
                    fileName = "PlayBookManagement";
                    break;
                case LoggerFileName.Utility:
                    fileName = "Utility";
                    break;
                default: fileName = "LogFile";
                    break;
            }

            //string currentDirectory = Directory.GetCurrentDirectory();
            //currentDirectory = currentDirectory + "\\logs\\" + System.DateTime.Now.ToString("MM-dd-yyyy") + "\\";
            log4net.GlobalContext.Properties["LogName"] = DateTime.Now.ToString("yy-MM-dd") + "\\" + fileName;
            log4net.Config.XmlConfigurator.Configure();
        }

        /// <summary>
        /// Below code is for removal of old log files
        /// </summary>
        private static void RemoveOldLogs()
        {
            try
            {
                string[] directories = Directory.GetDirectories("Logs\\");

                foreach (string directory in directories)
                {
                    //Created the Code as per the date if user wants certain customization in terms of date
                    DirectoryInfo directoryInfo = new DirectoryInfo(directory);
                    if (directoryInfo.CreationTime < DateTime.Now.AddDays(Convert.ToDouble(ConfigurationManager.AppSettings["RollingFileDeletionValueinDays"].ToString())))
                    {
                        directoryInfo.Delete(true);
                    }
                }
            }
            catch (Exception excp)
            {
                Log("Error while deleting old logs", excp);
            }
        }

        public static void Log(string error, Exception ex = null)
        {
            if (ex == null)
                Logger.Error(error);
            else
                Logger.Error(error, ex);
        }

        public static void Info(string info, LoggerFileName fileName)
        {
            LogManagerConfigure(fileName);
            Logger.Info(info);
        }

        public static void Debug(string info, LoggerFileName fileName)
        {
            LogManagerConfigure(fileName);
            Logger.Debug(info);
        }

        public static void Warn(string info, LoggerFileName fileName)
        {
            LogManagerConfigure(fileName);
            Logger.Warn(info);
        }

        public static void Fatal(Exception excp, LoggerFileName fileName)
        {
            string exceptionDetails = ExtractExceptionDetails(excp);
            LogManagerConfigure(fileName);
            Logger.Fatal(exceptionDetails);
        }

        private static string ExtractExceptionDetails(Exception excp)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\n[Start Of Error Report]");
            sb.Append("\nTime Stamp\t:" + DateTime.Now);
            sb.Append("\nError Message\t:" + excp.Message);
            sb.Append("\nInner Exception\t:" + excp.InnerException);

            System.Diagnostics.StackFrame sf = new System.Diagnostics.StackFrame(1);
            System.Reflection.MethodBase mb = sf.GetMethod();
            string methodName = sf.GetMethod().DeclaringType.ToString() + "." + mb.Name;
            string stackTrace = excp.StackTrace;

            sb.Append("\nStack Trace\t:" + stackTrace);
            sb.Append("\nMethod Name\t:" + methodName);
            sb.Append("[End Of Error Report]");
            sb.Append("\n\n----------------------------------");
            return sb.ToString();
        }
    }
}
