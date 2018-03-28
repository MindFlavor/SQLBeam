using System;
using System.Collections.Generic;
using System.Text;
using SQLBeam.LoggingExtensions;
using System.Data.SqlClient;
using Newtonsoft.Json;


namespace SQLBeam.Core.Tasks.Executable
{
    public abstract class PersonalizableTaskBase<INIT, PERS> : TaskBase where INIT : InitializationBase where PERS : PersonalizationBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(BulkCopyBase<INIT, PERS>));

        public INIT Initializations = default(INIT);
        public PERS Personalizations = default(PERS);

        public override void Initialize()
        {
            base.Initialize();

            if (!string.IsNullOrEmpty(TaskParameters))
                Initializations = JsonConvert.DeserializeObject<INIT>(TaskParameters);
        }

        public override void Personalize(string parameters)
        {
            log.Trace($"{this.GetType().FullName:S}::Personalize(${parameters:S}) called");
            base.Personalize(parameters);

            if (!string.IsNullOrEmpty(parameters))
            {
                try
                {
                    Personalizations = Newtonsoft.Json.JsonConvert.DeserializeObject<PERS>(parameters);
                }
                catch (System.Exception exce)
                {
                    log.Warn($"Cannot personalize task: ${exce.Message}");
                    throw new Exceptions.ExceptionDuringTaskPersonalization(this, exce);
                }
            }
        }

        public override string ToString()
        {
            return base.ToString() + $"PersonalizableTaskBase[Initializations={Initializations}, Personalizations={Personalizations}]";
        }

    }
}
