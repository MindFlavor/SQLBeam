using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLBeam.LoggingExtensions;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;

namespace SQLBeam.Core.Tasks.Executable.TSQLETL
{
    public class TSQLETL : BulkCopyBase<Initializations, Personalizations>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(TSQLETL));

        protected override void PopolateData(Destination dest, DataTable dt)
        {
            log.Trace($"PopolateData(dest === ${dest.ToString():S}, dt == ${dt.ToString()})");
           
            using (SqlConnection conn = new SqlConnection(dest.ConnectionString))
            {
                conn.Open();

                log.TraceFormat($"Executing query {Initializations.TSQL:S}");
                using (SqlCommand cmd = new SqlCommand(Initializations.TSQL, conn))
                {
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandTimeout = 1000 * 60 * 60 * 24;

                    SqlDataAdapter ada = new SqlDataAdapter(cmd);

                    log.TraceFormat($"Filling DataAdapter");
                    ada.Fill(dt);
                    log.TraceFormat($"DataAdapter filled");
                }
            }
        }
    }
}
