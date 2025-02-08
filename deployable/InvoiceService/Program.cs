using InvoiceService;
using InvoiceService.Middleware;
using InvoiceService.Repository;
using InvoiceService.Repository.Interfaces;
using InvoiceService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<AppDbContext>(db => {
    db.UseNpgsql(builder.Configuration.GetConnectionString("NpsqlConnection"));
});

//Repositories
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();

// Services
builder.Services.AddScoped<IInvoiceService, InvoiceService.Services.InvoiceService>();

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

app.Run();