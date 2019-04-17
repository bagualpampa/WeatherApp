using System.Threading.Tasks;
using WeatherApi.Models;

namespace WeatherApi.Services
{
    public interface IOpenWeatherLightClient
    {
        Task<Forecast> GetForecast(double lat, double lon, bool metric = false);
    }
}
