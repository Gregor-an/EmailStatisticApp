using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EmailStatisticApp.EmailProcessing
{
    class EmailProcessor : IDisposable
    {
        public ThreadManager ThreadManager { get; set; }
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        EmailProcessor()
        {
            this.ThreadManager = new ThreadManager(maxQueueCount: Config.QueueMaxCount, minThreadCount: Config.WorkerThreads);
        }

        public void ProcessEmails()
        {
            try
            {
                ThreadManager.Start();
            }
            catch(Exception ex)
            {
                log.Error(ex.Message, ex);
            }
            do
            {
                try
                {
                    //List<EmailObject> l = dataSource.RulesEngineEmailSelect();
                    //ProcessEmailsQueue(l);
                }
                catch(Exception ex)
                {
                    log.Error(ex.Message, ex);
                }
            }
            while(this.ThreadManager.NextCycle());
        }

        public void Dispose()
        {
            
        }
    }
}
