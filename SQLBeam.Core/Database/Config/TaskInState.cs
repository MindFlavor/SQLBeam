using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLBeam.Core.Database.Config
{
    public enum TaskState
    {
        Undetermined,
        Waiting,
        Scheduled,
        Running,
        Completed,
        Error
    }

    public class TaskInState
    {
        public TaskState TaskState { get; set; }

        public Guid Guid { get; set; }
        public int TaskID { get; set; }
        public string Parameters { get; set; }
        public DateTime WaitStartTime { get; set; }
        public int DestinationID { get; set; }

        public Guid Server { get; set; }

        public DateTime? ScheduledTime { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? CompleteTime { get; set; }

        public DateTime? ErrorTime { get; set; }
        public string ErrorText { get; set; }
    }
}
