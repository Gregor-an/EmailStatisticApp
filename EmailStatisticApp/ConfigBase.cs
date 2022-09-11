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
    public abstract class ConfigBase
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected static T GetConfig<T>(string key, T defaultValue, Configuration config = null)
        {
            try
            {
                var value = config == null ? ConfigurationManager.AppSettings[key] : config.AppSettings.Settings[key]?.Value;
                if(value == null)
                {
                    return defaultValue;
                }
                else
                {
                    if(typeof(T).IsEnum)
                    {
                        return (T)Enum.Parse(typeof(T), value);
                    }
                    else
                    {
                        return (T)Convert.ChangeType(value, typeof(T));
                    }
                }
            }
            catch(Exception ex)
            {
                log.Error(string.Format("GetConfig error for key {0}", key ?? ""), ex);
                return defaultValue;
            }
        }
    }
}
