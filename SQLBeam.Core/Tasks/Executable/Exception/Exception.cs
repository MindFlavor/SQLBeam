using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLBeam.LoggingExtensions;

namespace SQLBeam.Core.Tasks.Executable.Exception
{
    public class Exception : TaskBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Exception));

        public string Message { get; set; }

        public override void Execute(Destination dest)
        {
            log.TraceFormat($"Exception::Execute({dest.ToString():S}) called on {this.ToString():S}");

            string s = string.IsNullOrEmpty(Message) ? "Default message" : Message;
            throw new Exceptions.BeamExeception(s);
        }
        public override void Personalize(string parameters)
        {
            log.Trace($"{this.GetType().FullName:S}::Personalize(${parameters}) called");
            base.Personalize(parameters);

            if (!string.IsNullOrEmpty(parameters))
            {
                try
                {
                    var definition = new { Message = "" };
                    Message = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(parameters, definition).Message;

                    log.Trace($"Message set to ${this.Message}");
                }
                catch (System.Exception exce)
                {
                    log.Warn($"Cannot personalize task: ${exce.Message}");
                }
            }
        }

    }
}
