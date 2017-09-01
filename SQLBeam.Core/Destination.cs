using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLBeam.Core
{
    public class Destination
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string ConnectionString { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is Destination))
                throw new ArgumentException("Must compare two Destination objects");
            var o = (Destination)obj;
            return this.ID.Equals(o.ID);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString() + $"[Destination(ID={ID:N0}, Name=\"{Name:S}\", ConnectionString=\"{ConnectionString:S}\")]";
        }
    }
}
