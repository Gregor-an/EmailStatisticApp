using System;
using System.Linq;
using System.Text;
using System.ServiceProcess;
using log4net;
using System.Reflection;
using System.IO;
using System.Configuration.Install;
using EmailStatisticApp.EmailProcessing;
using System.Threading;
using System.Data.SqlClient;
using System.Data;

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
                bool showMenu = true;
                while(showMenu)
                {
                    showMenu = MainMenu();
                }
                service.OnStop();
            }
            else
            {
                ServiceBase.Run(new Program());
            }
        }

        private static bool MainMenu()
        {
            Console.Clear();
            Console.WriteLine("Choose an option:");
            Console.WriteLine("1) Print the last 100 records");
            Console.WriteLine("2) Exit");
            Console.Write("\r\nSelect an option: ");

            switch(Console.ReadLine())
            {
                case "1":
                    try
                    {
                        PrintTheLastRecords();
                        Console.ReadLine();
                    }
                    catch(Exception ex) 
                    {
                        log.Error(ex.Message, ex);
                    }
                    return true;
                case "2":
                    return false;
                default:
                    return true;
            }
        }

        private static void PrintTheLastRecords()
        {
            using(SqlConnection con = new SqlConnection(Config.EmailsDBConnectionString))
            {
                using(SqlCommand cmd = new SqlCommand("ReadEmailInfo", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    con.Open();
                    DataTable Table = new DataTable("Records");
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(Table);
                    DumpDataTable(Table);
                }
            }
        }

        private static void DumpDataTable(DataTable dt)
        {
            using(DataTableReader dtReader = dt.CreateDataReader())
            {
                if((dt == null) || !(dtReader.HasRows))
                {
                    Console.Error.WriteLine("There are no rows");
                }
                else
                {
                    while(dtReader.Read())
                    {
                        Console.WriteLine(new string('-', 50));
                        for(int i = 0; i < dtReader.FieldCount; i++)
                        {
                            string value = dtReader.GetValue(i).ToString().Trim();
                            Console.WriteLine("{0} = {1}",
                                dtReader.GetName(i).Trim(),
                                string.IsNullOrEmpty(value) ? "NULL" : value);
                        }
                        Console.WriteLine();
                    }
                }
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
