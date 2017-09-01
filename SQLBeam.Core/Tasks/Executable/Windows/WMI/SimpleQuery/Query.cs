using System;
using System.Collections.Generic;
using System.Text;
using SQLBeam.LoggingExtensions;
using System.Data.SqlClient;
using System.Data;
using SQLBeam.Core;
using System.Management;

namespace SQLBeam.Core.Tasks.Windows.WMI.SimpleQuery
{
    public class Query : Core.Tasks.Executable.BulkCopyBase<Initializations, Personalizations>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Query));

        protected override void PopolateData(Destination dest, DataTable dt)
        {
            ManagementScope scope = new ManagementScope("\\\\VSQL16A\\root\\cimv2");
            scope.Connect();
            ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);

            foreach (var i in searcher.Get())
            {
                Console.WriteLine(i);
            }

        }
    }

}
