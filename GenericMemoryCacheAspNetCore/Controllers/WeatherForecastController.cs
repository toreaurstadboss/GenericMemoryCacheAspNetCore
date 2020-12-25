using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SomeAcme.SomeUtilNamespace;

namespace GenericMemoryCacheAspNetCore.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly GenericMemoryCache<WeatherForecast> genericMemoryForecast;
        private readonly GenericMemoryCache<WeatherForecast> _genericMemoryCache;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, GenericMemoryCache<WeatherForecast> genericMemoryForecast)
        {
            _logger = logger;
            _genericMemoryCache = genericMemoryForecast;
            if (_logger != null)
            {

            }
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            _genericMemoryCache.AddItem("OSLO", new WeatherForecast { Summary = "SUNNY" });
            

            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
