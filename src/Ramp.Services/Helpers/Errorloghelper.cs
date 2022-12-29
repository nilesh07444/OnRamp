using Common.Data;
using Domain.Models;
using System;
using System.IO;
using System.Web.Hosting;

namespace Ramp.Services.Helpers
{
    public class Errorloghelper
    {
        private IRepository<ErrorLogs> _logRepository;

        public Errorloghelper(IRepository<ErrorLogs> logRepository)
        {
            this._logRepository = logRepository;
        }

        public string getErrorexception(Exception error)
        {
            string date = DateTime.UtcNow.Date.ToString();
            string[] datesplit = date.Split(' ');
            string today = datesplit[0].Replace('/', '_');
            string FileName = "RampPro_ErrorLog_" + today;
            string path = HostingEnvironment.MapPath(@"~/ErrorFiles/") + FileName + ".txt";
            if (!File.Exists(path))
            {
                File.Create(path).Dispose();
                TextWriter tw = new StreamWriter(path);
                tw.WriteLine("Error Message :" + error.Message);
                tw.WriteLine("Error Description :" + error.StackTrace);
                tw.WriteLine("Error DateTime :" + DateTime.UtcNow);
                tw.WriteLine("----------------------------------------------------------------------------------------------------------");
                tw.Close();
            }
            else if (File.Exists(path))
            {
                var fs = new FileStream(path, FileMode.Append, FileAccess.Write);
                TextWriter tw = new StreamWriter(fs);
                tw.WriteLine("Error Message :" + error.Message);
                tw.WriteLine("Error Description :" + error.StackTrace);
                tw.WriteLine("Error DateTime :" + DateTime.UtcNow);
                tw.WriteLine("----------------------------------------------------------------------------------------------------------");
                tw.Close();
            }

            if (error != null)
            {
                var errorlog = new ErrorLogs();
                errorlog.ErrorMessage = error.Message;
                errorlog.ErrorDescription = error.StackTrace;
                errorlog.ErrorDate = DateTime.UtcNow;
                _logRepository.Add(errorlog);
            }
            return null;
        }
    }
}