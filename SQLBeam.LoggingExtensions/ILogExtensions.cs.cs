using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace SQLBeam.LoggingExtensions
{
    public static class ILogExtentions
    {
        public static void Trace(this ILog log, string message, Exception exception)
        {
            log.Logger.Log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType,
                log4net.Core.Level.Trace, message, exception);
        }

        public static void Trace(this ILog log, string message)
        {
            log.Trace(message, null);
        }
        public static void TraceFormat(this ILog log, string message, params object[] list)
        {
            log.Trace(string.Format(message, list), null);
        }

        public static void Verbose(this ILog log, string message, Exception exception)
        {
            log.Logger.Log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType,
                log4net.Core.Level.Verbose, message, exception);
        }

        public static void Verbose(this ILog log, string message)
        {
            log.Verbose(message, null);
        }

        public static void VerboseFormat(this ILog log, string message, params object[] list)
        {
            log.Verbose(string.Format(message, list), null);
        }
    }
}
