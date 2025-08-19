using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Weather_App.Models;

namespace Weather_App.Controllers
{
    public class WeatherController : Controller
    {
        private readonly string apikey = "728296625bc2ac2fec0931d151a5281c";

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Index(string city, string unit)
        {
            if(string.IsNullOrEmpty(city))
            {
                ViewBag.Error = "Please enter a city name.";
                return View();
            }

            bool isCelsius = (unit == "metric");

            Weather weather = await GetWeather(city, isCelsius);

            if(weather == null)
            {
                ViewBag.Error = "Could not retieve weather data.";
                return View();
            }

            return View(weather);
        }

        private async Task<Weather> GetWeather(string city, bool isCelsius)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    string unit = isCelsius ? "metric" : "imperial";
                    string weatherUrl = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid=728296625bc2ac2fec0931d151a5281c&units={unit}";
                    string forecastUrl = $"https://api.openweathermap.org/data/2.5/forecast?q={city}&appid=728296625bc2ac2fec0931d151a5281c&units={unit}";

                    var weatherResponse = await client.GetAsync(weatherUrl);
                    var forecastResponse = await client.GetAsync(forecastUrl);

                    if (!weatherResponse.IsSuccessStatusCode || !forecastResponse.IsSuccessStatusCode)
                        return null;

                    var weatherJson = await weatherResponse.Content.ReadAsStringAsync();
                    var forecastJson = await forecastResponse.Content.ReadAsStringAsync();

                    dynamic weatherData = JsonConvert.DeserializeObject(weatherJson);
                    dynamic forecastData = JsonConvert.DeserializeObject(forecastJson);

                    var forecastList = new List<ForecastItem>();
                    foreach (var item in forecastData.list)
                    {
                        string time = item.dt_txt;
                        if(time.Contains("12:00:00"))
                        {
                            forecastList.Add(new ForecastItem
                            {
                                Date = DateTime.Parse(time).ToString("ddd, dd MMM"),
                                Temperature = item.main.temp,
                                Description = item.weather[0].description,
                                Icon = item.weather[0].icon,
                            });
                        }
                    }

                    return new Weather
                    {
                        City = weatherData.name,
                        Description = weatherData.weather[0].description,
                        Temperature = weatherData.main.temp,
                        Humidity = weatherData.main.humidity,
                        WindSpeed = weatherData.wind.speed,
                        Icon = weatherData.weather[0].icon,
                        Forecasts = forecastList,
                        IsCelsius = isCelsius,
                        WeatherCondition = weatherData.weather[0].main
                    };
                }
                catch
                {
                    return null;
                }
            }
        }
    }

}
