using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpYaml.Serialization;

namespace SQLBeam.Core
{
    public class Configuration
    {
        public string ConfigConnectionString;
        public string DWHConnectionString;
        public int MaximumThreads;
        public int MainPollIntervalMilliseconds;
        public int JoinTimeReactorMilliseconds;
        public int JoinTimeExecutorMilliseconds;
        public bool RESTEnabled;
        public int RESTPort;

        public void Serialize(string fileName)
        {
            string s = new Serializer().Serialize(this);
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(System.IO.File.Create(fileName)))
            {
                sw.Write(s);
            }
        }

        public static Configuration Deserialize(System.IO.FileInfo file)
        {
            var serde = new Serializer();
            using (System.IO.StreamReader sr = new System.IO.StreamReader(new System.IO.FileStream(file.FullName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read)))
            {
                string s = sr.ReadToEnd();
                return serde.Deserialize<Configuration>(s);
            }
        }
    }
}
