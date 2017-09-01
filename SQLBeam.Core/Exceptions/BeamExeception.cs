using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLBeam.Core.Exceptions
{
    public class BeamExeception : Exception
    {
        public BeamExeception() : base() { }

        public BeamExeception(string s) : base(s) { }

        public BeamExeception(string s, Exception innerException) : base(s, innerException) { }
    }
}
