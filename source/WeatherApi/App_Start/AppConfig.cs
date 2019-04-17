using Autofac;
using Autofac.Integration.WebApi;
using Serilog;
using System.Configuration;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Web.Http;
using WeatherApi.Services;

namespace WeatherApi
{
    public class AppConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();
            config.Formatters.Clear();

            config.Formatters.Add(new JsonMediaTypeFormatter());
            config.EnsureInitialized();
        }

        public static void RegisterServices(HttpConfiguration config)
        {
            string baseUrl = ConfigurationManager.AppSettings["BaseURL"];
            string apiKey = ConfigurationManager.AppSettings["APIKEY"];


            var builder = new ContainerBuilder();

            builder.Register(ctx =>
            {
                Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("logs\\{Hour}.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();
                return Log.Logger;
            }).As<ILogger>().SingleInstance();

            builder.Register(ctx => new OpenWeatherClient(baseUrl, apiKey, ctx.Resolve<ILogger>()))
                .AsImplementedInterfaces()
                .InstancePerRequest();

            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            builder.RegisterWebApiFilterProvider(config);

            var container = builder.Build();
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }
    }
}