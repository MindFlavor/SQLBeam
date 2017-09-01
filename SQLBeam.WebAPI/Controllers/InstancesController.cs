using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using SQLBeam.LoggingExtensions;

namespace SQLBeam.WebAPI.Controllers
{
    public class InstancesController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(InstancesController));

        // GET api/instances 
        public IEnumerable<string> Get(string id)
        {
            log.TraceFormat($"Called Get({id:S})");
            List<String> lDbs = new List<string>();

            var dwh = new Core.Database.DWHDatabase(Startup.CoreConfiguration);

            #region Checking table name existance
            if (!dwh.TableExists(id))
            {
                throw new SQLBeam.Core.Exceptions.TableNotFoundException(id);
            }
            #endregion

            return dwh.GetInstancesByTable(id);
        }
    }
}
