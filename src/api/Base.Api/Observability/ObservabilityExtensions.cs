using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

namespace Base.Api.Observability;

public static class ObservabilityExtensions
{
    public static IServiceCollection AddAppObservability(this IServiceCollection services, IConfiguration configuration)
    {
        var serviceName = configuration["Observability:ServiceName"] ?? "Base.Api";

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddProcessInstrumentation()
                .AddMeter(Metrics.MeterName)
                .AddPrometheusExporter());

        return services;
    }

    public static WebApplication MapAppObservability(this WebApplication app)
    {
        app.MapPrometheusScrapingEndpoint();
        return app;
    }
}
