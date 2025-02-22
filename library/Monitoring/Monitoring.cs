using Microsoft.Extensions.Configuration;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Enrichers.Span;
using System;
using Serilog.Events;

namespace Monitoring;

public static class Monitoring
{
    public static ILogger Log => Serilog.Log.Logger;
    
    // Parameterized constructor for dynamic configuration
    public static void ConfigureLogging(string? seqEndpoint = null, LogEventLevel logLevel = LogEventLevel.Verbose)
    {
        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Is(logLevel) // Set the log level dynamically
            .WriteTo.Console() // Always write to the console
            .Enrich.WithSpan(); // Include Span info in logs

        if (!string.IsNullOrEmpty(seqEndpoint))
        {
            loggerConfig = loggerConfig.WriteTo.Seq(seqEndpoint); // Configure Seq endpoint if provided
        }
        else
        {
            Log.Warning("Seq sink is not configured for Serilog.");
        }

        Serilog.Log.Logger = loggerConfig.CreateLogger();
    }

    public static OpenTelemetryBuilder Setup(this OpenTelemetryBuilder builder, string serviceName, string serviceVersion, string? zipkinEndpoint)
    {
        return builder.WithTracing(tcb =>
        {
            tcb.AddSource(serviceName)
                .SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService(serviceName: serviceName, serviceVersion: serviceVersion));
                //.AddConsoleExporter();
            
            if (!string.IsNullOrEmpty(zipkinEndpoint))
            {
                tcb.AddZipkinExporter(c => c.Endpoint = new Uri(zipkinEndpoint));
            }
            else
            {
                Log.Warning("Zipkin exporter is not configured for OpenTelemetry.");
            }

            tcb.AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation();
        });
    }
}
