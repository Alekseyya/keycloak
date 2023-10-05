using Microsoft.AspNetCore.Authorization;

namespace Keycloak
{
    internal class AuthorizeAccessRequirementHandler : AuthorizationHandler<AuthorizeAccessRequirement>
    {
        private readonly IConfiguration configuration;

        public AuthorizeAccessRequirementHandler(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AuthorizeAccessRequirement requirement)
        {
            var enableAuthorize = bool.Parse(configuration["Authentication:EnableAuthorization"]);
            if(!enableAuthorize || context.User.Identities.Any(x => x.IsAuthenticated))
                context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
    sealed class AuthorizeAccessRequirement : IAuthorizationRequirement
    {
    }
}
