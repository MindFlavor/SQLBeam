using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLBeam.WebAPI.Classes
{
    public class ExecutableTaskCreateRequest
    {
        public string TaskName;
        public string Destination;
        public SQLBeam.Core.Tasks.Executable.Backup.Personalizations Personalizations;
    }
}
