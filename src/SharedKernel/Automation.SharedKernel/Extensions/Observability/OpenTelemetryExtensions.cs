using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Automation.SharedKernel.Extensions.Observability;

public static class OpenTelemetryExtensions
{
    public static IHostApplicationBuilder AddOpenTelemetryServices(this IHostApplicationBuilder builder)
    {
        // Thu thập logging
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        // Metric - chỉ số sức khỏe của server
        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                // Metrics của aspnetcore
                metrics.AddAspNetCoreInstrumentation()
                       // Metrics của http client
                       .AddHttpClientInstrumentation()
                       // Metrics của runtime
                       .AddRuntimeInstrumentation();
            })
            // Trace - truy vết luồng xử lý
            .WithTracing(tracing =>
            {
                // Trace của aspnetcore
                tracing.AddAspNetCoreInstrumentation()
                       // Trace của http client
                       .AddHttpClientInstrumentation()
                       // Trace của ef core
                       .AddEntityFrameworkCoreInstrumentation();
            });

        // Export Metrics và Trace đến Collector
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        return builder;
    }
}


