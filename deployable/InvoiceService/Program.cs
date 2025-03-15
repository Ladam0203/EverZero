using Context;
using InvoiceService.Mappings;
using InvoiceService.Repositories;
using InvoiceService.Repositories.Interfaces;
using InvoiceService.Repository;
using InvoiceService.Services;
using InvoiceService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Monitoring;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Configure Tracing
var serviceName = "InvoiceService";
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
    db.UseNpgsql(builder.Configuration.GetConnectionString("NpsqlConnection"));
});

// DbInitializer
builder.Services.AddScoped<DbInitializer>();

//Repositories
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Services
builder.Services.AddScoped<IInvoiceService, InvoiceService.Services.InvoiceService>();
builder.Services.AddScoped<ISuggestionService, SuggestionService>();

// Middleware
builder.Services.AddScoped<RequestContext>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

var app = builder.Build();

app.UseMiddleware<RequestContextMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

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