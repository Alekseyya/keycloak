using Keycloak;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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
builder.Services.AddSwaggerGen();
//builder.Services.AddSwaggerGen(c =>
//{
//    var securityScheme = new OpenApiSecurityScheme
//    {
//        Name = "Auth",
//        In = ParameterLocation.Header,
//        Type = SecuritySchemeType.OpenIdConnect,
//        OpenIdConnectUrl = new Uri(openIdConnectUrl),
//        Scheme = "bearer",
//        BearerFormat = "JWT",
//        Reference = new OpenApiReference
//        {
//            Id = "Bearer",
//            Type = ReferenceType.SecurityScheme
//        }
//    };
//    c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
//    c.AddSecurityRequirement(new OpenApiSecurityRequirement
//    {
//        {securityScheme, Array.Empty<string>()}
//    });
//});
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.Authority = $"{builder.Configuration["Authentication:KeycloakAuthentication:ServerAddress"]}/realms/{builder.Configuration["Authentication:KeycloakAuthentication:Realm"]}";
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
    {//настройка надо ли проверять доп инф о токене
        ValidateAudience = false,    // установка потребителя токена
        ValidateLifetime = true, // будет ли валидироваться время существования
        ValidateIssuer = true,// строка, представляющая издателя
        NameClaimType = "preferred_username",
        RoleClaimType = "role",
        ClockSkew = TimeSpan.Zero
    };
    o.SaveToken = true;
});
builder.Services.AddAuthorization(options => {
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
            .AddRequirements(new AuthorizeAccessRequirement())
            .Build();
    options.AddPolicy("airport", policy => policy.Requirements.Add(new AirportAccessRequirement("SVO1")));
});
builder.Services.AddSingleton<IAuthorizationHandler, AuthorizeAccessRequirementHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, AirportAccessRequirementHandler>();
IdentityModelEventSource.ShowPII = true;

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

app.MapControllers().RequireAuthorization();

app.Run();
