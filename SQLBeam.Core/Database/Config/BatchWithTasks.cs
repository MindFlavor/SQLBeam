using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLBeam.Core.Database.Config
{
    public class BatchWithTasks : Batch
    {
        public List<Guid> Task_GUIDs { get; private set; }

        public BatchWithTasks()
        {
            this.Task_GUIDs = new List<Guid>();
        }
    }
}
