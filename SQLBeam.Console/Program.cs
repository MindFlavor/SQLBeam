using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLBeam.Core;
using SQLBeam.LoggingExtensions;

namespace SQLBeam.Console
{
    class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Program));

        private static Reactor reactor;

        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                System.Console.WriteLine("Syntax error:\nSyntax: SQLBeam.Console.exe <log configuration file> <configuration file>");
                return;
            }

            string logConfigurationFile = args[0];

            #region Setup logging
            // Setup logging
            FileInfo fi = new FileInfo(logConfigurationFile);
            if (!fi.Exists)
                throw new ArgumentException("Cannot find log configuration file: " + fi.FullName);

            try
            {
                using (System.IO.FileStream fs = new FileStream(fi.FullName,
                    FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    fs.ReadByte();
                }

            }
            catch (Exception exce)
            {
                throw new ArgumentException("Error opening log configuration file", exce);
            }

            log4net.Config.XmlConfigurator.Configure(new FileInfo(logConfigurationFile));
            log.InfoFormat("{0:S} v{1:S} started.", 
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Name,
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
            #endregion            

            System.IO.FileInfo fiConfig = new FileInfo(args[1]);
            log.DebugFormat("Loading configuration from file {0:S}", fiConfig.FullName);
            Configuration config = Configuration.Deserialize(fiConfig);
            log.InfoFormat("Configuration read from file {0:S}", fiConfig);

            #region Startup Owin WebAPI
            // Start OWIN host 
            if (config.RESTEnabled)
            {
                SQLBeam.WebAPI.Startup.CoreConfiguration = config;
                var uri = $"http://+:{config.RESTPort:0}/api";
                log.Info($"Opening REST port at {uri}");
                var owin = Microsoft.Owin.Hosting.WebApp.Start<SQLBeam.WebAPI.Startup>(url: uri);
            }
            else
            {
                log.Info("REST is disabled");
            }
            #endregion

            reactor = new Reactor(config);
            reactor.Start();

            log.InfoFormat("Press <ENTER> to terminate the program");
            System.Console.ReadLine();

            reactor.Stop();
            log.InfoFormat("Reactor shutted down");
        }
    }
}
