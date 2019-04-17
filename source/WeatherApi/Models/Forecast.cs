using System.Collections.Generic;

namespace WeatherApi.Models
{
    /// <summary>
    /// Main Forecast class
    /// </summary>
    public class Forecast
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public Coord Coord { get; set; }
        public string Country { get; set; }
        public List<Prediction> Predictions { get; set; }
        public Prediction Current { get; set; }
    }
}