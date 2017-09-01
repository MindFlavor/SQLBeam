using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLBeam.LoggingExtensions;
using System.Web.Http;
using SQLBeam.Core.Database.Config;
using System.Net.Http;

namespace SQLBeam.WebAPI.Controllers
{
    public class TaskInStateController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(TaskInStateController));

        public Core.Database.Config.TaskInState Get(string id)
        {
            log.Trace($"Called Get({id})");

            Guid guid = Guid.Parse(id);

            var conn = new SQLBeam.Core.Database.ConfigDatabase(Startup.CoreConfiguration);
            return conn.GetTaskInState(guid);
        }

        public List<Core.Database.Config.TaskInState> Get()
        {
            log.Trace($"Called Get()");

            var conn = new SQLBeam.Core.Database.ConfigDatabase(Startup.CoreConfiguration);
            return conn.GetTasksInStates(30);
        }

        // Delete/state cleans the relative state
        // it does work only on completed and error
        public HttpResponseMessage Delete(string id)
        {
            TaskState ts = (TaskState)Enum.Parse(typeof(TaskState), id);

            if (ts != TaskState.Completed && ts != TaskState.Error)
                return new HttpResponseMessage(System.Net.HttpStatusCode.NotImplemented);

            var conn = new SQLBeam.Core.Database.ConfigDatabase(Startup.CoreConfiguration);
            conn.CleanTaskInState(ts);

            return new HttpResponseMessage(System.Net.HttpStatusCode.NoContent);
        }
    }
}
