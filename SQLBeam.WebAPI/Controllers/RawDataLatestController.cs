using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using SQLBeam.LoggingExtensions;

namespace SQLBeam.WebAPI.Controllers
{
    public class RawDataLatestController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(RawDataLatestController));

        private const string SLASH_PLACEHOLDER = "-_-";

        // GET /<table>/<server>
        // In server the back slash should be written as -_- (dash - underscore - dash)
        // so to not conflict with path separator. The code will replace -_- with
        // the back slash \
        public System.Data.DataTable Get(string id, string id2, string id3)
        {
            log.TraceFormat($"Called Get({id:S}, {id2:S}, {id3:S})");
            return Get(id, string.Format($"{id2:S}\\{id3:S}"));
        }

        // GET /<table>/<server>
        // In server the back slash should be written as -_- (dash - underscore - dash)
        // so to not conflict with path separator. The code will replace -_- with
        // the back slash \
        public System.Data.DataTable Get(string id, string id2)
        {
            log.TraceFormat($"Called Get({id:S}, {id2:S})");
            List<String> lDbs = new List<string>();

            string servernameUnmangled = id2.Replace(SLASH_PLACEHOLDER, "\\");
            log.DebugFormat($"servernameUnmangled {servernameUnmangled:S}");

            var dwh = new Core.Database.DWHDatabase(Startup.CoreConfiguration);

            #region Checking table name existance
            if (!dwh.TableExists(id))
            {
                throw new SQLBeam.Core.Exceptions.TableNotFoundException(id);
            }
            #endregion

            return dwh.GetDataRawLatest(id, servernameUnmangled);
        }
    }
}
