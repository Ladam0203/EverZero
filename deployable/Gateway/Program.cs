using Gateway.Authentication;
using Gateway.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Monitoring;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using OpenTelemetry.Trace;
using ILogger = Serilog.ILogger;

var builder = WebApplication.CreateBuilder(args);

// Configure Tracing
var serviceName = "Gateway";
var serviceVersion = "1.0.0";
var zipkinEndpoint = builder.Configuration["Zipkin:Endpoint"];
builder.Services.AddOpenTelemetry().Setup(serviceName, serviceVersion, zipkinEndpoint);
builder.Services.AddSingleton(TracerProvider.Default.GetTracer(serviceName));

// Configure Logging
var seqEndpoint = builder.Configuration["Seq:Endpoint"];
Monitoring.Monitoring.ConfigureLogging(seqEndpoint);
builder.Services.AddSingleton(Monitoring.Monitoring.Log);

// Configure Ocelot endpoints
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

app.UseMiddleware<RequestLoggingMiddleware>();

app.UseOcelot().Wait();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();