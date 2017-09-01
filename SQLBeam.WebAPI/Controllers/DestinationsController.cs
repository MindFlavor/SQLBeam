using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using SQLBeam.LoggingExtensions;

namespace SQLBeam.WebAPI.Controllers
{
    public class DestinationsController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(DestinationsController));

        // GET api/destinations
        public IEnumerable<object> Get()
        {
            log.Trace($"Called Get()");
            List<object> lDestinationsWithoutConnectionString = new List<object>();

            var config = new Core.Database.ConfigDatabase(Startup.CoreConfiguration);

            foreach (var dest in config.GetDestinations())
            {
                lDestinationsWithoutConnectionString.Add(new { ID = dest.ID, Name = dest.Name });
            }

            return lDestinationsWithoutConnectionString;
        }
    }
}
