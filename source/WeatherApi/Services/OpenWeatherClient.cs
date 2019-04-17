using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog;
using WeatherApi.Models;

namespace WeatherApi.Services
{
    public class OpenWeatherClient : IOpenWeatherLightClient
    {
        private readonly HttpClient _client;

        private const string CacheKey = "forecast/{0}/{1}/{2}";
        private const string ApiFcUrl = "forecasts/v1/daily/5day/{0}?apikey={1}&details=true&metric={2}";
        private const string ApiLocUrl = "locations/v1/cities/geoposition/search?apikey={0}&q={1}%2c{2}&language=en-us&detailes=false&toplevel=true";
        private readonly string _apiKey;
        private readonly ILogger _logger;

        public OpenWeatherClient(string baseUrl, string apiKey, ILogger logger)
        {
            _apiKey = apiKey;
            _logger = logger;
            _client = new HttpClient { BaseAddress = new Uri(baseUrl) };
        }

        public async Task<Forecast> GetForecast(double lat, double lon, bool metric )
        {
            // Initialize cache
            var cache = MemoryCache.Default;
            // Cache Policy: 60 seconds
            var cachePolicy = new CacheItemPolicy { AbsoluteExpiration = DateTime.Now.AddSeconds(60) };
            // Number format to avoid locales change

            string cachedName = String.Format(new CultureInfo("en-US"),CacheKey, lat, lon, metric);
            // Search cache by cachedName
            Forecast cachedResponse = (Forecast) cache.Get(cachedName);
            if (cachedResponse == null)
            {
                try
                {
                    // Call accuweather api seeking city by coords
                    var citySearch = String.Format(new CultureInfo("en-US"), ApiLocUrl, _apiKey, lat , lon);
                    var citylocation = await _client.GetAsync(citySearch);
                    var vs = await citylocation.Content.ReadAsStringAsync();
                    var jsonCity = JsonConvert.DeserializeObject<dynamic>(vs);
                    CityData cityData = (CityData)GetCityData(jsonCity);

                    if (cityData.Key == null)
                    {
                        _logger.Error(String.Format("City not found, coords:{0},{1}", lat, lon));
                        return null;
                    }

                    var requestUrl = String.Format(ApiFcUrl, cityData.Key, _apiKey,  metric);
                    var response = await _client.GetAsync(requestUrl);
                    var content = await response.Content.ReadAsStringAsync();
                    var json = JsonConvert.DeserializeObject<dynamic>(content);

                    // Parse json object as Forecast
                    cachedResponse = ParseJson(json);
                    // Complete city data
                    cachedResponse.ID = cityData.Key;
                    cachedResponse.Country = cityData.Country;
                    cachedResponse.Name = cityData.EnglishName;
                    cachedResponse.Coord = new Coord
                    {
                        Lat = lat,
                        Lon = lon
                    };

                    if (cachedResponse != null)
                    {
                        // Store forecast
                        cache.Add(requestUrl, cachedResponse, cachePolicy);
                        _logger.Debug("Cache miss: " + requestUrl);
                    }
                    else
                    {
                        _logger.Error("No answer: " + requestUrl);
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("Error contacing API:" + cachedName, ex);
                    throw new ApplicationException("Error contacting API.", ex);
                }
            } else
            {
                _logger.Debug("Cache hit: " + cachedName);
            }
            return cachedResponse;
        }

        static CityData GetCityData(dynamic jsonCity)
        {
            var cityData = new CityData();
            if (jsonCity != null)
            {
                cityData.Key = jsonCity.Key;
                cityData.EnglishName = jsonCity.EnglishName;
                cityData.Country = jsonCity.Country.EnglishName;
            }
            return cityData;
        }

        static Forecast ParseJson(dynamic json)
        {
            var error = json.error;
            if (error != null)
            {
                if (error.httpStatusCode == 400)
                {
                    return null;
                }

                throw new ApplicationException((string)error.errorMessage);
            }
            var forecast = new Forecast();

            if (!GetParsedForecast(ref forecast, json))
            {
                return null;
            }

            return forecast;
        }

        public static bool GetParsedForecast(ref Forecast forecast, dynamic json)
        {

            // Initialize results list and current forecast object
            var results = new List<Prediction>();
            for (var i = 0; i < json.DailyForecasts.Count; i++)
            {
                results.Add(CreatePrediction(json.DailyForecasts[i]));
            }
            forecast.Predictions = results;
            forecast.Current = results[0];
            return true;
        }

        public static Prediction CreatePrediction(dynamic current)
        {
             Prediction prediction = new Prediction
            {
                Date = UnixTimeStampToDateTime((double)current.EpochDate).ToString("MMMM dd"),
                DayName = UnixTimeStampToDateTime((double)current.EpochDate).ToString("dddd"),
                TempMax = current.Temperature.Maximum.Value,
                TempMin = current.Temperature.Minimum.Value,
                Wind = current.Day.Wind.Speed.Value,
                Humidity = current.Day.TotalLiquid.Value,
                Main = current.Day.IconPhrase,
                Description = current.Day.LongPhrase,
                Icon = current.Day.Icon.ToString("D2")
            };
            return prediction;
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }
}