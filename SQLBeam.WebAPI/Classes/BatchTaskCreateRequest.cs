using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLBeam.WebAPI.Classes
{
    public class BatchTaskCreateRequest
    {
        public string Task { get; set; }
        public int[] DestinationIDs { get; set; }
    }
}
