using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLBeam.LoggingExtensions;

namespace SQLBeam.Core.Tasks
{
    public class TaskBase : ITask
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(TaskBase));

        public int ID { get; set; }
        public string Name { get; set; }
        public string Class { get; set; }
        public string TaskParameters { get; set; }
        public string Personalization { get; set; }
        public bool IsDebug { get; set; }
        public Configuration Configuration { get; set; }

        public virtual void Execute(Destination dest)
        {
            string s = dest.ToString();
            string p = this.ToString();
            log.Trace(string.Format("TaskBase::Execute({0:S}) called on {1:S}", s, p));
        }

        public virtual void Initialize()
        {
            log.TraceFormat("TaskBase::Initialize called on {0:S}", this.ToString());
        }

        public virtual void Personalize(string parameters)
        {
            this.Personalization = parameters;
            log.Trace($"{this.GetType().FullName:S}::Personalize(${parameters}) called");
        }

        public override string ToString()
        {
            return base.ToString() + $"[TaskBase(ID={ID:N0}, Class=\"{Class:S}\", TaskParameters=\"{TaskParameters:S}\", IsDebug={IsDebug.ToString():S})]";
        }
    }
}
