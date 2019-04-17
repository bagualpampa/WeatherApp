using System.Threading.Tasks;
using System.Web.Http;
using WeatherApi.Services;
using WeatherApi.Utils;

namespace WeatherApi.Controllers
{
    /// <summary>
    /// Weather Api Controller, Get 5 days forecast
    /// </summary>
    [RoutePrefix("api/v1"), AllowAnonymous]
    public class WeatherController : ApiController
    {
        private readonly IOpenWeatherLightClient _openWeatherClient;

        /// <summary>
        /// Weather Main Controller
        /// </summary>
        public WeatherController(IOpenWeatherLightClient openWeatherClient)
        {
            _openWeatherClient = openWeatherClient;
        }

        /// <summary>
        /// Get Forecast: calls openweather api and returns 5 days forecast
        /// </summary>
        /// <param name="lat"> Lattitude </param>
        /// <param name="lon"> Longitude </param>
        /// <param name="metric"> Units: metric = true, imperial = false</param>
        /// <returns> 5 days Forecast for a given location </returns>
        [HttpGet, Route("{lat}/{lon}/{metric?}"), Cache(TimeDurationInMinutes = 10)]
        public async Task<IHttpActionResult> GetForecast(double lat, double lon, bool metric = false)
        {
            var forecast = await _openWeatherClient.GetForecast(lat, lon, metric);

            if (forecast == null)
            {
                return NotFound();
            }
            return Ok(forecast);
        }
    }
}
