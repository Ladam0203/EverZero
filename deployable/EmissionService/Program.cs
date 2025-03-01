using Context;
using EmissionService.Infrastructure;
using EmissionService.Mappings;
using EmissionService.Repositories;
using EmissionService.Repositories.Interfaces;
using EmissionService.Services;
using EmissionService.Services.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MongoDB
BsonSerializer.RegisterSerializer(typeof(Guid), new GuidSerializer(GuidRepresentation.Standard));
var mongoConnectionString = builder.Configuration.GetConnectionString("mongo");
var mongoClient = new MongoClient(mongoConnectionString);
var database = mongoClient.GetDatabase("emission");

builder.Services.AddSingleton(database);

// DbInitializer
builder.Services.AddScoped<DbInitializer>();

// Repositories
builder.Services.AddScoped<IEmissionFactorRepository, EmissionFactorRepository>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Services
builder.Services.AddScoped<IEmissionFactorService, EmissionService.Services.EmissionService>();

// Request context
builder.Services.AddScoped<RequestContext>();

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