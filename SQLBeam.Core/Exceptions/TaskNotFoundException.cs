using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLBeam.Core.Exceptions
{
    public class TaskNotFoundException : DatabaseException
    {
        public string TaskName { get; private set; }
        public TaskNotFoundException(string taskName)
            : base(string.Format($"Task with name {taskName} not found"))
        {
            this.TaskName = TaskName;
        }
    }
}
