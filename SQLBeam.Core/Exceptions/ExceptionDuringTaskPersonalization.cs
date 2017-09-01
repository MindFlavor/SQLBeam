using SQLBeam.Core.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLBeam.Core.Exceptions
{
    public class ExceptionDuringTaskPersonalization: BeamExeception
    {
        public TaskBase TaskBase { get; protected set; }
        public ExceptionDuringTaskPersonalization(TaskBase taskBase, Exception InnerException)
            : base("Exception during task personalization", InnerException)
        {
            this.TaskBase = taskBase;
        }
    }
}
