using log4net;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
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

        public EmailProcessor()
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
                    List<string> emails = SelectEmailsToProcess();
                    ProcessEmailsQueue(emails);
                }
                catch(Exception ex)
                {
                    log.Error(ex.Message, ex);
                }
            }
            while(this.ThreadManager.NextCycle());
        }
        private List<string> SelectEmailsToProcess()
        {
            string dirPath = Config.EmailsDirectoryPath;
            if(Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            Console.WriteLine($"DirPath {dirPath}");

            return Directory.GetFiles(dirPath, "*.eml").ToList();
        }

        readonly List<string> _emailInProcessing = new List<string>();
        readonly object _emailInProcessingLocker = new object();

        private void ProcessEmailsQueue(List<string> emails)
        {
            foreach(string email in emails)
            {
                Console.WriteLine($"Email {email}");

                ThreadManager.StartNewJob(() =>
                {
                    try 
                    {
                        lock(_emailInProcessingLocker)
                        {
                            if(_emailInProcessing.Contains(email))
                            {
                                log.Error($"Email {email} processing starting is failed, it is already in processing.");
                                return;
                            }
                            _emailInProcessing.Add(email);
                        }

                        ProcessEmail(email);
                    }
                    catch(Exception ex)
                    {
                        log.Error(ex);
                    }
                    finally
                    {
                        lock(_emailInProcessingLocker)
                        {
                            _emailInProcessing.Remove(email);
                            DeleteEmail(email);
                        }
                    }
                });
            }
        }

        private void ProcessEmail(string email)
        {
            using(EmailMessageLoader emailLoader = new EmailMessageLoader(Path.Combine(Config.EmailsDirectoryPath, email)))
            {
                Console.WriteLine( $"Sender {emailLoader.EmailMessage.Sender.DisplayName} Subject {emailLoader.EmailMessage.Subject}");
            }
        }

        private void DeleteEmail(string email)
        {
            string emailPath = Path.Combine(Config.EmailsDirectoryPath, email);
            if(File.Exists(emailPath))
            {
                File.Delete(emailPath);
            }
        }

        public void Dispose()
        {
            
        }
    }
}
