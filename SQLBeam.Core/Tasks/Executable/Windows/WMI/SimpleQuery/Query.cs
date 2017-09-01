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
            ManagementScope scope = new ManagementScope($"\\\\{dest.Name}\\root\\cimv2");
            scope.Connect();
            ObjectQuery query = new ObjectQuery(Initializations.Query);
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);

            bool fFirst = true;
            
            foreach (var queryResult in searcher.Get())
            {
                 if(fFirst)
                {
                    // initialize the datatable
                    foreach(var prop in queryResult.Properties)
                    {
                        dt.Columns.Add(new DataColumn(prop.Name, prop.Value.GetType()));
                    }

                    fFirst = false;
                }

                DataRow row = dt.NewRow();
                for (int i=0; i<queryResult.Properties.Count; i++)
                {
                    row[i] = queryResult.Properties[dt.Columns[i].ColumnName].Value;
                }

                dt.Rows.Add(row);
            }
        }
    }

}
