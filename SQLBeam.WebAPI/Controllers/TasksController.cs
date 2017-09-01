using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using SQLBeam.LoggingExtensions;
using System.Net.Http;
using SQLBeam.Core.Tasks;

namespace SQLBeam.WebAPI.Controllers
{
    public class TasksController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(TasksController));

        public List<ITask> Get()
        {
            log.TraceFormat($"Called Get()");

            DateTime dtStart = DateTime.Now;
            var c = new SQLBeam.Core.Database.ConfigDatabase(Startup.CoreConfiguration).GetTasks();
            log.Trace($"Get took {(DateTime.Now - dtStart).TotalMilliseconds:N0} ms");
            return c;
        }

        // POST in body pass the parameter as JSON.
        [System.Web.Http.Description.ResponseType(typeof(ITask))]
        [HttpPut]
        [ActionName("Complex")]
        public HttpResponseMessage Put(Classes.BatchWithTaskInStatesGetRequest parameters)
        {

            throw new NotImplementedException();


        }
 

    }
}
