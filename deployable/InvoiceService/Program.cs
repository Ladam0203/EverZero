using Context;
using InvoiceService.Mappings;
using InvoiceService.Repository;
using InvoiceService.Repository.Interfaces;
using InvoiceService.Services;
using InvoiceService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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