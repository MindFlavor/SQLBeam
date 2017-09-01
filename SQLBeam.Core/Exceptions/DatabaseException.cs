using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLBeam.Core.Exceptions
{
    public class DatabaseException:BeamExeception
    {
        public DatabaseException() : base() { }

        public DatabaseException(string message) : base(message) { }
    }
}
