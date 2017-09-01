using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLBeam.Core.Tasks.Executable.Wait
{
    public class Wait : TaskBase
    {
        public int SleepTimeSeconds { get; set; }

        public override void Initialize()
        {
            base.Initialize();
            this.SleepTimeSeconds = int.Parse(TaskParameters);
        }

        public override void Execute(Destination dest)
        {
            System.Threading.Thread.Sleep(SleepTimeSeconds * 1000);
        }
    }
}
