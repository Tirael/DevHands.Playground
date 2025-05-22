namespace DevHands.Playground.Application.Extensions;

internal static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseObservability(this IApplicationBuilder app) =>
        app
            .UseOpenTelemetryPrometheusScrapingEndpoint(context => context.Request.Path == "/metrics");
}
