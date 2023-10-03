using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddLogging();
var openIdConnectUrl = "http://localhost:8080/realms/test/.well-known/openid-configuration";

builder.Services.AddSwaggerGen(c =>
{
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Auth",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.OpenIdConnect,
        OpenIdConnectUrl = new Uri(openIdConnectUrl),
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = "Bearer",
            Type = ReferenceType.SecurityScheme
        }
    };
    c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {securityScheme, Array.Empty<string>()}
    });
});
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.Authority = builder.Configuration["Authentication:KeycloakAuthentication:ServerAddress"] + "/auth/realms/" + builder.Configuration["Authentication:KeycloakAuthentication:Realm"];
    o.Audience = builder.Configuration["Authentication:KeycloakAuthentication:ClientId"];
    o.RequireHttpsMetadata = false;
    o.BackchannelHttpHandler = new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (message, certificate, chain, sslPolicyErrors) =>
        {
            return true;
        }
    };
    o.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuer = true,
        NameClaimType = "preferred_username",
        RoleClaimType = "role",
        ClockSkew = TimeSpan.Zero
    };
    o.SaveToken = true;
});
IdentityModelEventSource.ShowPII = true;
builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.All;
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;

});

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
//app.UseCors("MyPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.UseHttpLogging();

app.MapControllers();

app.Run();
