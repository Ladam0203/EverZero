using Microsoft.Extensions.Configuration;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Enrichers.Span;
using System;

namespace Monitoring
{
    public class Monitoring
    {
        private readonly IConfiguration _config;

        // Inject IConfiguration via constructor
        public Monitoring(IConfiguration config)
        {
            _config = config;

            // Serilog Configuration
            string? seqEndpoint = _config["Monitoring:Seq"];
            
            if (string.IsNullOrEmpty(seqEndpoint))
            {
                Serilog.Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo.Console()
                    .Enrich.WithSpan()
                    .CreateLogger();
                Log.Warning("Seq sink is not configured for Serilog.");
            } else {
                Serilog.Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo.Console()
                    .WriteTo.Seq(seqEndpoint)
                    .Enrich.WithSpan()
                    .CreateLogger();
            }
        }

        public OpenTelemetryBuilder Setup(OpenTelemetryBuilder builder, string serviceName, string serviceVersion)
        {
            string zipkinEndpoint = _config["Monitoring:Zipkin"];

            return builder.WithTracing(tcb =>
            {
                tcb.AddSource(serviceName)
                    .SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService(serviceName: serviceName, serviceVersion: serviceVersion))
                    .AddConsoleExporter();
                
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
}
