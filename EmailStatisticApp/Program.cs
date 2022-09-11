using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceProcess;
using System.Threading.Tasks;
using log4net;
using System.Reflection;
using System.IO;
using System.Configuration.Install;
using EmailStatisticApp.EmailProcessing;
using System.Threading;

namespace EmailStatisticApp
{
    class Program : ServiceBase
    {
        public static string InstallServiceName = "Email_Process_App";
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        static Program()
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
        }

        static void Main(string[] args)
        {
            bool debugMode = false;
            if(args.Length > 0)
            {
                for(int ii = 0; ii < args.Length; ii++)
                {
                    switch(args[ii].ToUpper())
                    {
                        case "/NAME":
                            if(args.Length > ii + 1)
                            {
                                InstallServiceName = args[++ii];
                            }
                            break;
                        case "/I":
                            InstallService();
                            return;
                        case "/U":
                            UninstallService();
                            return;
                        case "/D":
                            debugMode = true;
                            break;
                        default:
                            break;
                    }
                }
            }

            if(debugMode)
            {
                Console.WriteLine("Ready to start...");
                Console.WriteLine("<press enter to continue...>");
                Console.ReadLine();
                Program service = new Program();
                service.OnStart(null);
                Console.WriteLine("Service Started...");
                Console.WriteLine("<press enter to exit...>");
                Console.ReadLine();
                service.OnStop();
            }
            else
            {
                ServiceBase.Run(new Program());
            }
        }

        EmailProcessor _emailProcessor;
        protected override void OnStart(string[] args)
        {
            this._emailProcessor = new EmailProcessor();
            Thread thread = new Thread(new ThreadStart(this._emailProcessor.ProcessEmails));
            thread.Start();
        }

        protected override void OnStop()
        {
            if(_emailProcessor != null)
            {
                try
                {
                    _emailProcessor.ThreadManager.Stop();
                }
                catch(Exception ex)
                {
                    log.Error(ex.Message, ex);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _emailProcessor.Dispose();
        }

        private static bool IsServiceInstalled()
        {
            return ServiceController.GetServices().Any(s => s.ServiceName == InstallServiceName);
        }

        private static void InstallService()
        {
            if(IsServiceInstalled())
            {
                UninstallService();
            }

            ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
        }

        private static void UninstallService()
        {
            ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
        }
    }
}
