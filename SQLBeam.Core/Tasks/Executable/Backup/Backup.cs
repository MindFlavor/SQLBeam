using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLBeam.LoggingExtensions;
using System.Data.SqlClient;

namespace SQLBeam.Core.Tasks.Executable.Backup
{
    public class Backup : PersonalizableTaskBase<Initializations, Personalizations>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Backup));

        public override void Execute(Destination dest)
        {
            base.Execute(dest);

            if (Personalizations == null)
                throw new ArgumentException("Personalizations are required for backup");

            string fileName = string.Format("{0:S}_{1:S}.{2:S}",
                Personalizations.DataBase,
                DateTime.Now.ToString(Personalizations.DateTimeFormatSuffix),
                Personalizations.BackupType == BackupType.Log ? "trn" : "bak");

            string tsql = $"BACKUP " +
                        string.Format(Personalizations.BackupType == BackupType.Full || Personalizations.BackupType == BackupType.Differential ? "DATABASE" : "LOG") +
                        $"[{Personalizations.DataBase}] TO DISK = '{Personalizations.DestinationFolder}\\{fileName}'" +
                        string.Format(" WITH {0:S}", Personalizations.EnableCompression ? "COMPRESSION" : "NO_COMPRESSION") +
                        string.Format(", {0:S}", Personalizations.Init ? "INIT" : "NOINIT") +
                        string.Format("{0:S}", Personalizations.BackupType == BackupType.Differential ? " DIFFERENTIAL" : "") +
                        ";";

            log.Trace($"tsql = {tsql}");


            using (SqlConnection conn = new SqlConnection(dest.ConnectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(tsql, conn))
                {
                    cmd.CommandTimeout = 0;
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
