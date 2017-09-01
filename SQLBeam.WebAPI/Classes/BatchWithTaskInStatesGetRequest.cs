using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLBeam.WebAPI.Classes
{
    public class BatchWithTaskInStatesGetRequest
    {
        public Guid[] BatchGUIDs { get; set; }
    }
}
