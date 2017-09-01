using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLBeam.Core.Tasks.Executable
{
    public abstract class InitializationBase
    {
        public const string TAG_SERVER_NAME = "SERVER_NAME";
        public const string TAG_INSERT_TIME = "INSERT_TIME";

        public string DestinationTable { get; set; }
        public List<KeyValuePair<string,string>> CalculatedFields { get; set; }

        public string Serialize()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }
}
