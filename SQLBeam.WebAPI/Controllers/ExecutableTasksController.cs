using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using SQLBeam.LoggingExtensions;

namespace SQLBeam.WebAPI.Controllers
{
    public class ExecutableTasksController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(ExecutableTasksController));

        // POST /api/ExecutableTasks<url>/Task/Destination
        // both destination and task must be passed by name
        // Destination can be two parts (ie Server\Instance)
        public object Post(string id, string id2, string id3)
        {
            log.Trace($"Called Post({id}, {id2}, {id3})");
            return Post(id, string.Format($"{id2}\\{id3}"));
        }

        public object Post(string id, string id2)
        {
            log.Trace($"Called Post({id}, {id2})");
            var conn = new SQLBeam.Core.Database.ConfigDatabase(Startup.CoreConfiguration);
            var destination = conn.GetDestinationByName(id2);
            var task = conn.GetTaskByName(id);

            log.Trace($"Adding task: {task.ToString():S} to destination: {destination.ToString():S}");
            return new { Guid = conn.AddTaskToWait(destination, task) };
        }
    }

}
