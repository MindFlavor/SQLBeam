using System;
using System.Collections.Generic;
using System.Text;
using SQLBeam.LoggingExtensions;
using System.Data.SqlClient;
using Newtonsoft.Json;


namespace SQLBeam.Core.Tasks.Executable
{
    public abstract class BulkCopyBase<INIT, PERS> : PersonalizableTaskBase<INIT,PERS> where INIT : InitializationBase where PERS : PersonalizationBase
    {
        const string PARAM_TSQL = "TSQL";
        const string PARAM_DESTINATION_TABLE = "DestinationTable";

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(BulkCopyBase<INIT, PERS>));

        protected abstract void PopolateData(Destination dest, System.Data.DataTable dt);

        public override void Execute(Destination dest)
        {
            base.Execute(dest);
            log.TraceFormat($"Starting ETL to destination {dest.ToString()}");

            System.Data.DataTable dt = new System.Data.DataTable();

            PopolateData(dest, dt);

            #region Add calculated fields
            if (Initializations.CalculatedFields != null)
            {
                DateTime insertTime = DateTime.Now;
                string serverName = dest.Name;

                foreach (var calcField in Initializations.CalculatedFields)
                {
                    switch (calcField.Value)
                    {
                        case InitializationBase.TAG_INSERT_TIME:
                            dt.Columns.Add(new System.Data.DataColumn(calcField.Key, insertTime.GetType()));
                            foreach (System.Data.DataRow row in dt.Rows)
                            {
                                row[calcField.Key] = insertTime;
                            }
                            break;
                        case InitializationBase.TAG_SERVER_NAME:
                            dt.Columns.Add(new System.Data.DataColumn(calcField.Key, serverName.GetType()));
                            foreach (System.Data.DataRow row in dt.Rows)
                            {
                                row[calcField.Key] = serverName;
                            }
                            break;
                    }
                }
            }
            #endregion

            #region Add personalizations
            if (Personalizations != null)
            {
                #region Add fixed columns
                if (Personalizations.ConstantFields != null)
                {
                    foreach (var constField in Personalizations.ConstantFields)
                    {
                        dt.Columns.Add(new System.Data.DataColumn(constField.Key, constField.Value.GetType()));

                        foreach (System.Data.DataRow row in dt.Rows)
                        {
                            row[constField.Key] = constField.Value;
                        }
                    }
                }
                #endregion
            }
            #endregion

            log.TraceFormat($"Opening connection to DWH");
            using (SqlConnection conn = new SqlConnection(Configuration.DWHConnectionString))
            {
                conn.Open();

                log.TraceFormat("Starting bulk insert");

                using (SqlBulkCopy sbc = new SqlBulkCopy(conn))
                {
                    sbc.DestinationTableName = Initializations.DestinationTable;

                    // This mapping ensures proper name mataching between source and 
                    // destination regardless of their order. This is needed because
                    // we are adding Server and InsertTime columns at the and of
                    // the DataTable but those columns can be somewhere else
                    // in the destination table (tipically at the start)
                    foreach (System.Data.DataColumn column in dt.Columns)
                    {
                        sbc.ColumnMappings.Add(new SqlBulkCopyColumnMapping(column.ColumnName, column.ColumnName));
                    }

                    sbc.WriteToServer(dt);
                }
            }
        }
    }
}
