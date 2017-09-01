using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLBeam.Core.Exceptions
{
    public class IDNotFoundException<T> : DatabaseException
    {
        public T ID { get; protected set; }
        public string Table { get; protected set; }

        public IDNotFoundException(T ID, string Table)
            : base($"ID (type {ID.GetType().FullName}, value {ID.ToString()}) not found in table {Table}")
        {
            this.ID = ID;
            this.Table = Table;
        }
    }
}
