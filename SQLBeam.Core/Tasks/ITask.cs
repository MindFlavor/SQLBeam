using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLBeam.Core.Tasks
{
    public interface ITask
    {
        int ID { get; set; } 
        string Name { get; set; }
        string Class { get; set; } 
        string TaskParameters { get; set; }

        string Personalization { get; set; }

        bool IsDebug { get; set; }

        Configuration Configuration { get; set; }

        void Initialize();

        void Personalize(string parameters);

        void Execute(Destination dest);
     }
}