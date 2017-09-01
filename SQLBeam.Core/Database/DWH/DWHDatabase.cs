using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLBeam.LoggingExtensions;
using System.Data.SqlClient;

namespace SQLBeam.Core.Database
{
    public class DWHDatabase

    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(DWHDatabase));

        private const string CHECK_TABLE_NAME_STREAM = "SQLBeam.Core.Database.DWH.CheckTableName.sql";
        private static string _tsqlCheckTableName;

        private const string GET_INSTANCE_STREAM = "SQLBeam.Core.Database.DWH.GetInstancesByTable.sql";
        private static string _tsqlGetInstance;

        private const string GET_DATA_RAW_STREAM = "SQLBeam.Core.Database.DWH.GetDataRawLatest.sql";
        private const string TABLE_PLACEHOLDER = "%%!TableName!%%";
        private static string _tsqlGetDataRaw;

        private Configuration Configuration;

        public DWHDatabase(Configuration Configuration)
        {
            this.Configuration = Configuration;
        }

        private static string TSQLGetInstance
        {
            get
            {
                if (_tsqlGetInstance == null)
                {
                    log.TraceFormat($"Reading manifest stream {GET_INSTANCE_STREAM:S}");
                    using (System.IO.StreamReader sr = new System.IO.StreamReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(GET_INSTANCE_STREAM)))
                    {
                        _tsqlGetInstance = sr.ReadToEnd();
                        log.TraceFormat($"Stream read: \"{_tsqlGetInstance:S}\"");
                    }
                }

                return _tsqlGetInstance;
            }
        }

        private static string TSQLCheckTableName
        {
            get
            {
                if (_tsqlCheckTableName == null)
                {
                    log.TraceFormat($"Reading manifest stream {CHECK_TABLE_NAME_STREAM:S}");
                    using (System.IO.StreamReader sr = new System.IO.StreamReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(CHECK_TABLE_NAME_STREAM)))
                    {
                        _tsqlCheckTableName = sr.ReadToEnd();
                        log.TraceFormat($"Stream read: \"{_tsqlCheckTableName:S}\"");
                    }
                }

                return _tsqlCheckTableName;
            }
        }
        private static string TSQLGetDataRaw
        {
            get
            {
                if (_tsqlGetDataRaw == null)
                {
                    log.TraceFormat($"Reading manifest stream {GET_DATA_RAW_STREAM:S}");
                    using (System.IO.StreamReader sr = new System.IO.StreamReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(GET_DATA_RAW_STREAM)))
                    {
                        _tsqlGetDataRaw = sr.ReadToEnd();
                        log.TraceFormat($"Stream read: \"{_tsqlGetDataRaw:S}\"");
                    }
                }

                return _tsqlGetDataRaw;
            }
        }


        public bool TableExists(string table)
        {
            using (SqlConnection conn = new SqlConnection(Configuration.DWHConnectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(TSQLCheckTableName, conn))
                {
                    SqlParameter param = new SqlParameter("@name", System.Data.SqlDbType.NVarChar, -1);
                    param.Value = table;
                    cmd.Parameters.Add(param);

                    object ret = cmd.ExecuteScalar();
                    return !((ret == null) || (ret is DBNull));
                }
            }
        }


        public List<string> GetInstancesByTable(string tableName)
        {
            List<string> lDBs = new List<string>();

            using (SqlConnection conn = new SqlConnection(Configuration.DWHConnectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(TSQLGetInstance, conn))
                {
                    SqlParameter param = new SqlParameter("@name", System.Data.SqlDbType.NVarChar, -1);
                    param.Value = tableName;
                    cmd.Parameters.Add(param);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lDBs.Add(reader.GetString(0));
                        }
                    }
                }
            }

            return lDBs;
        }

        public System.Data.DataTable GetDataRawLatest(string table, string server)
        {
            System.Data.DataTable dt = new System.Data.DataTable();

            using (SqlConnection conn = new SqlConnection(Configuration.DWHConnectionString))
            {
                string tsl = TSQLGetDataRaw.Replace(TABLE_PLACEHOLDER, server);
                using (SqlCommand cmd = new SqlCommand(TSQLGetDataRaw, conn))
                {
                    SqlParameter param = new SqlParameter("@name", System.Data.SqlDbType.NVarChar, -1);
                    param.Value = table;
                    cmd.Parameters.Add(param);
                    //log.TraceFormat($"Adding {param.ToString():S}");

                    param = new SqlParameter("@serverName", System.Data.SqlDbType.NVarChar, -1);
                    param.Value = server;
                    cmd.Parameters.Add(param);
                    //log.TraceFormat($"Adding {param.ToString():S}");

                    using (SqlDataAdapter ada = new SqlDataAdapter(cmd))
                    {
                        ada.Fill(dt);
                    }
                }
            }

            return dt;
        }
    }

}
