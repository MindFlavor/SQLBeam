using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

        // POST in body pass the parameter as JSON.
        // example:
        // curl -Uri http://localhost:9000/api/executabletasks  -Method POST -ContentType "application/json" -Body "{Tasks:[{TaskName:""backup"", Destination:""localhost"", Personalizations:{DestinationFolder: ""C:\\temp\\backup"", DataBase: ""Sella"", DateTimeFormatSuffix: ""yyyyMMdd_HHmmss"", EnableCompression:true, Init:true, BackupType: ""Log""}}, {TaskName:""backup"", Destination:""localhost"", Personalizations:{DestinationFolder: ""C:\\temp\\backup"", DataBase: ""Sella"", DateTimeFormatSuffix: ""yyyyMMdd_HHmmss"", EnableCompression:true, Init:true, BackupType: ""Full""}}]}"
        [System.Web.Http.Description.ResponseType(typeof(Guid[]))]
        [HttpPost]
        [ActionName("Complex")]
        public HttpResponseMessage Post(Classes.ExecutableTaskCreateBatchRequest req)
        {
            DateTime dtStart = DateTime.Now;
            log.Trace($"Called Post({req.ToString():S})");
            try
            {
                List<Guid> lGuids = new List<Guid>();
                var config = new Core.Database.ConfigDatabase(Startup.CoreConfiguration);

                foreach (var r in req.Tasks)
                {
                    log.Trace($"Adding task {r.ToString():S}");
                    var taskId = config.GetTaskByName(r.TaskName);
                    var task = config.CreateTaskReflecting(taskId.ID);
                    string persString = Newtonsoft.Json.JsonConvert.SerializeObject(r.Personalizations);
                    task.Personalization = persString;

                    var destination = config.GetDestinationByName(r.Destination);

                    lGuids.Add(config.AddTaskToWait(destination, task));
                }
                log.Trace($"Post took {(DateTime.Now - dtStart).TotalMilliseconds:N0} ms");
                return this.Request.CreateResponse(System.Net.HttpStatusCode.Created, lGuids.ToArray<Guid>());
            }
            catch (Exception exce)
            {
                return this.Request.CreateErrorResponse(System.Net.HttpStatusCode.BadRequest, exce);
            }
        }

    }
}
