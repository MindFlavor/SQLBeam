using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLBeam.Core.Database.Config
{
    public class Batch
    {
        public Guid GUID { get; set; }
        public DateTime CreationTime { get; set; }

        public Batch() { }

        public Batch(Guid guid, DateTime creationTime)
        {
            this.GUID = guid;
            this.CreationTime = creationTime;
        }
   }
}
