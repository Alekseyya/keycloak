using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Test.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IHttpContextAccessor httpContextAccessor;

    public WeatherForecastController(ILogger<WeatherForecastController> logger , IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        this.httpContextAccessor = httpContextAccessor;
    }
    //[EnableCors("MyPolicy")]
    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<IEnumerable<WeatherForecast>> GetAsync()
    {
        var accessToken = await httpContextAccessor!.HttpContext!.GetTokenAsync("access_token");
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)],
            Token = accessToken!
        })
        .ToArray();
    }
}
