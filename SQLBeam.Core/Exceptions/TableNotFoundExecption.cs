using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLBeam.Core.Exceptions
{
    public class TableNotFoundException : DatabaseException
    {
        public TableNotFoundException(string table) 
            : base(string.Format($"Table {table:S} not found"))
        { }
    }
}
