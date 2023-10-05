using IdentityModel.Client;
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
    private readonly IConfiguration configuration;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        _logger = logger;
        this.httpContextAccessor = httpContextAccessor;
        this.configuration = configuration;
    }
    [Authorize(Policy = "airport")]
    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<IActionResult> GetAsync()
    {
        var clientId = configuration["Authentication:KeycloakAuthentication:ClientId"];
        var clientSecret = configuration["Authentication:KeycloakAuthentication:ClientSecret"];
        var accessToken = await httpContextAccessor!.HttpContext!.GetTokenAsync("access_token");
        var client = new HttpClient();
        var intospect = await client.IntrospectTokenAsync(new TokenIntrospectionRequest
        {
            Address = "http://localhost:8080/realms/test/protocol/openid-connect/token/introspect",
            ClientId = clientId,
            ClientSecret = clientSecret,

            Token = accessToken
        });
        if (!intospect.IsActive)
            return BadRequest("Token not valid!");
        var userInfo = await client.GetUserInfoAsync(new UserInfoRequest
        {
            Address = "http://localhost:8080/realms/test/protocol/openid-connect/userinfo",
            Token = accessToken
        });
        if (userInfo.IsError)
            return BadRequest("User info request error!");
        return Ok(userInfo.Claims.Where(x => x.Type.Contains("airports")).Select(x=> x.Value));
    }
    [HttpPost]
    public IActionResult HelloAsync()
    {
        return Ok("Hello");
    }
}
