using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EmailStatisticApp
{
    class Config : ConfigBase
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        static readonly Configuration _fileDetection;

        static Config()
        {
            try
            {
                //_fileDetection = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap() { ExeConfigFilename = "EmailSta.config" }, ConfigurationUserLevel.None);
            }
            catch(Exception ex)
            {
                log.Error("FileDetection.config setup was failed", ex);
            }
        }
        public static string _emailsDBConnectionString
        {
            get
            {

                string s = ConfigurationManager.ConnectionStrings["EmailsDBConnectionString"].ConnectionString;
                return s;
            }
        }
        public static int WorkerThreads
        {
            get
            {
                return GetConfig<int>(nameof(WorkerThreads), 10);
            }
        }

        public static int QueueMaxCount
        {
            get
            {
                return GetConfig<int>(nameof(QueueMaxCount), 10);
            }
        }

        public static string EmailsDirectoryPath
        {
            get
            {
                return GetConfig<string>("EmailsDirectoryPath", string.Empty);
            }
        }
    }
}
