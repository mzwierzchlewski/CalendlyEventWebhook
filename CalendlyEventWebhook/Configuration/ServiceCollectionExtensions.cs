using CalendlyEventWebhook.CalendlyApi;
using CalendlyEventWebhook.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace CalendlyEventWebhook.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCalendlyEventWebhook(this IServiceCollection services, IConfiguration configuration)
    {
        var calendlyConfiguration = configuration.GetSection(CalendlyConfiguration.SettingsKey).Get<CalendlyConfiguration>();
        if (calendlyConfiguration == null)
        {
            return services;
        }

        services
            .AddHttpClient<CalendlyClient>(
                client => CalendlyClient.ConfigureCalendlyHttpClient(client, calendlyConfiguration.AccessToken))
            .AddTransientHttpErrorPolicy(
                policyBuilder => policyBuilder.WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(0.25)));

        services.AddSingleton(calendlyConfiguration);
        services.AddSingleton<CalendlyIdService>();
        services.AddScoped<CalendlyService>();

        return services;
    }
}