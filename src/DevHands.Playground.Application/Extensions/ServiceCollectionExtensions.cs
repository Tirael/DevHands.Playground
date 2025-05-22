namespace DevHands.Playground.Application.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddObservability(this IServiceCollection services)
    {
        var openTelemetryResourceBuilder = ResourceBuilder
            .CreateDefault()
            .AddService(
                serviceName: "DevHands.Playground.Application",
                serviceVersion: Assembly.GetEntryAssembly()?.GetName().Version?.ToString(),
                serviceInstanceId: Environment.MachineName)
            .AddTelemetrySdk();

        return services
            .AddOpenTelemetry()
            .WithMetrics(meterProviderBuilder =>
            {
                meterProviderBuilder
                    .AddPrometheusExporter()
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .SetResourceBuilder(openTelemetryResourceBuilder)
                    .AddMeter(
                        "Microsoft.AspNetCore.Hosting",
                        "Microsoft.AspNetCore.Server.Kestrel"
                    );
            })
            .Services;
    }
}
