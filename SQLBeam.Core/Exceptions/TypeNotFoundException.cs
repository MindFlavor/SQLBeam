using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLBeam.Core.Exceptions
{
    public class TypeNotFoundException : BeamExeception
    {
        public string RequestedType { get; set; }

        public TypeNotFoundException(string RequestedType)
        {
            this.RequestedType = RequestedType;
        }
    }
}
