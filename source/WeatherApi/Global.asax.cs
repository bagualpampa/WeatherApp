using System.Web;
using System.Web.Http;

namespace WeatherApi
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(AppConfig.Register);
            GlobalConfiguration.Configure(AppConfig.RegisterServices);
        }
    }
}
