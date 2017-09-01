using System;
using SQLBeam.Core.Database.Config;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using SQLBeam.LoggingExtensions;
using System.Net.Http;

namespace SQLBeam.WebAPI.Controllers
{
    public class BatchTasksController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(BatchTasksController));

        public List<Core.Database.Config.Batch> Get()
        {
            DateTime dtStart = DateTime.Now;
            log.Trace($"Called Get()");

            var conn = new SQLBeam.Core.Database.ConfigDatabase(Startup.CoreConfiguration);
            var b = conn.GetBatches(30);

            log.Trace($"Get took {(DateTime.Now - dtStart).TotalMilliseconds:N0} ms");
            return b;
        }

        // POST in body pass the parameter as JSON.
        // See the BatchTaskRequest class for its definition.
        // E:\src\c-sharp\SQLBeam [master ≡ +5 ~7 -1 !]> curl -Uri http://localhost:9000/api/batchtasks  -Method PUT -ContentType
        // "application/json" -Body "{BatchGUIDs:[""3F76EF81-4387-E711-8111-00155D00A106"", ""558B4DA2-4387-E711-8111-00155D00A106"
        // "]}"
        [System.Web.Http.Description.ResponseType(typeof(List<BatchWithTaskInStates>))]
        [HttpPut]
        [ActionName("Complex")]
        public HttpResponseMessage Put(Classes.BatchWithTaskInStatesGetRequest parameters)
        {
            DateTime dtStart = DateTime.Now;
            log.Trace($"Called Put({parameters.ToString():S})");
            try
            {
                var config = new Core.Database.ConfigDatabase(Startup.CoreConfiguration);

                var batches = config.GetBatchWithTaskInStatesByGUIDs(parameters.BatchGUIDs);

                log.Trace($"Put took {(DateTime.Now - dtStart).TotalMilliseconds:N0} ms");
                return this.Request.CreateResponse(System.Net.HttpStatusCode.OK, batches);
            }
            catch (Exception exce)
            {
                return this.Request.CreateErrorResponse(System.Net.HttpStatusCode.BadRequest, exce);
            }

        }

        // POST in body pass the parameter as JSON.
        // See the BatchTaskRequest class for its definition.
        [System.Web.Http.Description.ResponseType(typeof(SQLBeam.Core.Database.Config.BatchWithTasks))]
        [HttpPost]
        [ActionName("Complex")]
        public HttpResponseMessage Post(Classes.BatchTaskCreateRequest parameters)
        {
            DateTime dtStart = DateTime.Now;
            log.Trace($"Called Post({parameters.ToString():S})");
            try
            {
                var config = new Core.Database.ConfigDatabase(Startup.CoreConfiguration);

                var task = config.GetTaskByName(parameters.Task);

                var batch = config.AddBatchTasks(task, parameters.DestinationIDs);

                log.Trace($"Post took {(DateTime.Now - dtStart).TotalMilliseconds:N0} ms");
                return this.Request.CreateResponse(System.Net.HttpStatusCode.Created, batch);

            }
            catch (Exception exce)
            {
                return this.Request.CreateErrorResponse(System.Net.HttpStatusCode.BadRequest, exce);
            }
        }
    }
}
