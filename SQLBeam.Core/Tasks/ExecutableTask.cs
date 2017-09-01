using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLBeam.Core.Tasks
{
    public class ExecutableTask
    {
        public Guid Guid { get; set; }
        public ITask Task { get; set; }
        public Destination Destination { get; set; }

        public override string ToString()
        {
            return base.ToString() + $"[ExecutableTask(Guid={Guid.ToString():S}, Task={Task.ToString():S}, Destination={Destination.ToString():S})]";
        }
    }
}
