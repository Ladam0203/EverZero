using Gateway.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddJsonFile("appsettings.json", true, true);
builder.Configuration.AddJsonFile("appsettings.Development.json", true, true);

builder.Configuration.AddJsonFile(builder.Environment.IsDevelopment() ? "ocelot.local.json" : "ocelot.json", true, true);

// Add services to the container.
builder.Services
    .AddAuthentication(
        options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }
    )
    .AddScheme<RemoteAuthenticationOptions, RemoteAuthenticationHandler>("RemoteAuthentication", null);

// Inject HttpClient, for RemoteAuthenticationHandler
builder.Services.AddScoped<HttpClient>();

builder.Services.AddOcelot(builder.Configuration);
builder.Services.AddControllers();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure Swagger UI
if (app.Environment.IsDevelopment() || args.Contains("swagger") || args.Contains("--swagger")) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseOcelot().Wait();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();