using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLBeam.Core;
using SQLBeam.LoggingExtensions;
using System.Data.SqlClient;

namespace SQLBeam.TaskLoader
{
    class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Program));

        static int Main(string[] args)
        {
            if (args.Length < 4)
            {
                Console.WriteLine("Syntax error: missing parameter(s).\nSyntax: SQLBeam.TaskLoader <name> <script_file> <destination_table> <connection_string> [ConstantFieldName:ConstantFieldValue]*");
                return -100;
            }

            string name = args[0];
            string scriptFile = args[1];
            string destinationTable = args[2];
            string connectionString = args[3];
            
            string strContent;
            using (System.IO.StreamReader sr = new System.IO.StreamReader(new System.IO.FileStream(scriptFile, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read)))
            {
                strContent = sr.ReadToEnd();
            }

            SQLBeam.Core.Tasks.Executable.TSQLETL.Initializations init = new Core.Tasks.Executable.TSQLETL.Initializations()
            {
                  TSQL = strContent,
                  DestinationTable = destinationTable,
                  CalculatedFields = new List<KeyValuePair<string, string>>()
            };

            for(int i=4;i<args.Length;i++ )
            {
                log.Info($"Parsing {args[i]:S} as KeyValue pair separated by colon");
                var tokens = args[i].Split(new char[] { ':' });

                var kvp = new KeyValuePair<string, string>(tokens[0], tokens[1]);
                log.Info($"Parsed as {kvp.ToString():S}");
                init.CalculatedFields.Add(kvp);
            }

            string yaml = init.Serialize();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"
                        INSERT INTO	[core].[Task]([Name], [Class], [Parameters])
                        VALUES(@name, @class, @parameters)
                    ", conn))
                {
                    SqlParameter param = new SqlParameter("@name", System.Data.SqlDbType.NVarChar, 255);
                    param.Value = name;
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@class", System.Data.SqlDbType.NVarChar, -1);
                    param.Value = typeof(SQLBeam.Core.Tasks.Executable.TSQLETL.TSQLETL).FullName;
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@parameters", System.Data.SqlDbType.NVarChar, -1);
                    param.Value = yaml;
                    cmd.Parameters.Add(param);

                    return cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
