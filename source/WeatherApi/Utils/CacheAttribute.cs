using System;
using System.Net.Http.Headers;
using System.Web.Http.Filters;

namespace WeatherApi.Utils
{
    public class CacheAttribute : ActionFilterAttribute
    {
        public int TimeDurationInMinutes { get; set; }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Response != null)
            {
                actionExecutedContext.Response.Headers.CacheControl = new CacheControlHeaderValue
                {
                    MaxAge = TimeSpan.FromMinutes(TimeDurationInMinutes),
                    MustRevalidate = true,
                    Public = true
                };
            }
        }
    }
}


