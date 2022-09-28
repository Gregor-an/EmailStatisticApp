using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
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

            return Directory.GetFiles(dirPath, "*.eml").ToList();
        }

        readonly List<string> _emailInProcessing = new List<string>();
        readonly object _emailInProcessingLocker = new object();

        private void ProcessEmailsQueue(List<string> emails)
        {
            foreach(string email in emails)
            {
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
                using(SqlConnection con = new SqlConnection(Config._emailsDBConnectionString))
                {
                    using(SqlCommand cmd = new SqlCommand("InsertEmailInfo", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@MessageId", SqlDbType.VarChar).Value = emailLoader.EmailMessage.MessageId;
                        cmd.Parameters.Add("@Subject", SqlDbType.VarChar).Value = emailLoader.EmailMessage.Subject;
                        cmd.Parameters.Add("@From", SqlDbType.VarChar).Value = emailLoader.EmailMessage.From.DisplayName;
                        cmd.Parameters.Add("@To", SqlDbType.VarChar).Value = String.Join(",", emailLoader.EmailMessage.To);
                        cmd.Parameters.Add("@Sender", SqlDbType.VarChar).Value = emailLoader.EmailMessage.Sender.DisplayName;
                        cmd.Parameters.AddWithValue("@TimeOfExecution", DateTime.Now);

                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
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
