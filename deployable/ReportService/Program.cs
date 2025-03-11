using Context;
using Microsoft.EntityFrameworkCore;
using Monitoring;
using OpenTelemetry.Trace;
using QuestPDF.Infrastructure;
using ReportService.Repositories;
using ReportService.Repositories.Interfaces;
using ReportService.Services;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// Configure Tracing
var serviceName = "ReportService";
var serviceVersion = "1.0.0";
var zipkinEndpoint = builder.Configuration["Zipkin:Endpoint"];
builder.Services.AddOpenTelemetry().Setup(serviceName, serviceVersion, zipkinEndpoint);
builder.Services.AddSingleton(TracerProvider.Default.GetTracer(serviceName));

// Configure Logging
var seqEndpoint = builder.Configuration["Seq:Endpoint"];
Monitoring.Monitoring.ConfigureLogging(seqEndpoint);
builder.Services.AddSingleton(Monitoring.Monitoring.Log);

// DbContext
builder.Services.AddDbContext<AppDbContext>(db => {
    db.UseSqlite(builder.Configuration.GetConnectionString("SQLiteConnection"));
});

// DbInitializer
builder.Services.AddScoped<DbInitializer>();

// Repositories
builder.Services.AddScoped<IReportRepository, ReportRepository>();

// Services
builder.Services.AddScoped<IReportService, ReportService.Services.ReportService>();

// Request context
builder.Services.AddScoped<RequestContext>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

var app = builder.Build();

app.UseMiddleware<RequestContextMiddleware>();

app.MapControllers();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

// Initialize the database
if (args.Contains("db-init") || args.Contains("--db-init"))
{
    using var scope = app.Services.CreateScope();
    var dbInitializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    await dbInitializer.Initialize();
}
// Reinitialize the database (development default)
else if (args.Contains("db-reinit") || args.Contains("--db-reinit") || app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbInitializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    await dbInitializer.Reinitialize();
}

app.Run();