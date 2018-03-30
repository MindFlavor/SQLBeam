using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLBeam.Core.Tasks.Executable.Backup
{
    public class Personalizations : PersonalizationBase
    {
        public string DestinationFolder;
        public string DataBase;
        public string DateTimeFormatSuffix;
        public bool EnableCompression;
        public bool Init;
        public BackupType BackupType;

        public override string ToString()
        {
            return base.ToString() + $"{this.GetType().FullName}[" +
                $"DestinationFolder={DestinationFolder}, DataBase={DataBase}, DateTimeFormatSuffix={DateTimeFormatSuffix}, EnableCompression={EnableCompression}" +
                $"Init={Init}, BackupType={BackupType}" +
                "]";
        }
    }
}
