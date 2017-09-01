using Owin;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SQLBeam.WebAPI
{
    public class Startup
    {
        public static SQLBeam.Core.Configuration CoreConfiguration;

        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public void Configuration(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();

            var cors = new System.Web.Http.Cors.EnableCorsAttribute("*", "*", "*");
            config.EnableCors(cors);

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "{controller}/{id}/{id2}/{id3}",
                defaults: new { id = RouteParameter.Optional, id2 = RouteParameter.Optional, id3 = RouteParameter.Optional });

            var appXmlType = config.Formatters.XmlFormatter.SupportedMediaTypes.FirstOrDefault(t => t.MediaType == "application/xml");
            config.Formatters.XmlFormatter.SupportedMediaTypes.Remove(appXmlType);

            // this will screw up curl but PowerShell will still work. JavaScript is to be tested.
            //var listener = (System.Net.HttpListener)appBuilder.Properties["System.Net.HttpListener"];
            //listener.AuthenticationSchemes = System.Net.AuthenticationSchemes.IntegratedWindowsAuthentication;

            appBuilder.UseWebApi(config);
        }
    }
}
