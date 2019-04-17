namespace WeatherApi.Models
{
    /// <summary>
    /// 5 Days prediction data
    /// </summary>
    public class Prediction
    {
        public double TempMin { get; set; }
        public double TempMax { get; set; }
        public string Date { get; set; }
        public double Wind { get; set; }
        public string Icon { get; set; }
        public double Humidity { get; set; }
        public string DayName { get; set; }
        public string Main { get; set; }
        public string Description { get; set; }
        public double Rain { get; set; }
    }
}