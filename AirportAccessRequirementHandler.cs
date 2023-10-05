using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

internal sealed class AirportAccessRequirementHandler : AuthorizationHandler<AirportAccessRequirement>
{
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly IConfiguration configuration;

    public AirportAccessRequirementHandler(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        this.httpContextAccessor = httpContextAccessor;
        this.configuration = configuration;
    }
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AirportAccessRequirement requirement)
    {
        var accessToken = await httpContextAccessor!.HttpContext!.GetTokenAsync("access_token");
        var clientId = configuration["Authentication:KeycloakAuthentication:ClientId"];
        var clientSecret = configuration["Authentication:KeycloakAuthentication:ClientSecret"];
        var client = new HttpClient();
        var intospect = await client.IntrospectTokenAsync(new TokenIntrospectionRequest
        {
            Address = "http://localhost:8080/realms/test/protocol/openid-connect/token/introspect",
            ClientId = clientId,
            ClientSecret = clientSecret,

            Token = accessToken
        });
        if (intospect.IsActive)
        {
            var userInfo = await client.GetUserInfoAsync(new UserInfoRequest
            {
                Address = "http://localhost:8080/realms/test/protocol/openid-connect/userinfo",
                Token = accessToken
            });
            if (!userInfo.IsError)
            {
                if(userInfo.Claims.Where(x => x.Type.Contains("airports")).Select(x => x.Value).Any(x => x == requirement.Airport))
                    context.Succeed(requirement);
            }
        }
        
        //var success = false;
        //if (context.User.HasClaim(x => x.Issuer == "airports"))
        //    context.Succeed(requirement);
        //context.Succeed(requirement);
        //if (context.User.Claims.TryGetResourceCollection(out var resourcesAccess) &&
        //    resourcesAccess.TryGetValue(clientId, out var resourceAccess))
        //{
        //    success = resourceAccess.Roles.Intersect(requirement.Roles).Any();

        //    if (success)
        //    {
        //        context.Succeed(requirement);
        //    }
        //}

        //this.ResourceAuthorizationResult(
        //    requirement.ToString(), success, context.User.Identity?.Name);

        //return Task.CompletedTask;
    }
}


sealed class AirportAccessRequirement : IAuthorizationRequirement
{
    public AirportAccessRequirement(string airport)
    {
        Airport = airport;
    }
    public string? Airport { get; set; }
}