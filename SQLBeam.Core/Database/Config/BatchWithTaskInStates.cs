using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLBeam.Core.Database.Config
{
    public class BatchWithTaskInStates : Batch
    {
        public List<TaskInState> TaskInStates { get; private set; }

        public BatchWithTaskInStates()
        {
            this.TaskInStates = new List<TaskInState>();
        }

        public BatchWithTaskInStates(Guid g, DateTime creationTime) 
            : base(g, creationTime) 
        {
            TaskInStates = new List<TaskInState>(); 
        }
    }
}
