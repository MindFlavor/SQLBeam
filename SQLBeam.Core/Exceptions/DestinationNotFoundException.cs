using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLBeam.Core.Exceptions
{
    public class DestinationNotFoundException : DatabaseException
    {
        public string DestinationName { get; private set; }
        public DestinationNotFoundException(string destinationName)
            : base(string.Format($"Destination with name {destinationName} not found"))
        {
            this.DestinationName = DestinationName;
        }
    }
}
