using CalendlyEventWebhook.CalendlyApi;
using CalendlyEventWebhook.Configuration;
using CalendlyEventWebhook.Handlers;
using CalendlyEventWebhook.Handlers.Defaults;
using CalendlyEventWebhook.Registration;
using CalendlyEventWebhook.Services;
using CalendlyEventWebhook.Webhook;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Polly;

namespace CalendlyEventWebhook;

public static class Installer
{
    public static IServiceCollection AddCalendlyEventWebhook(this IServiceCollection services, IConfiguration configuration)
    {
        var userCalendlyConfiguration = configuration.GetSection(UserCalendlyConfiguration.SettingsKey).Get<UserCalendlyConfiguration>();
        if (userCalendlyConfiguration == null)
        {
            return services;
        }

        var calendlyConfiguration = new CalendlyConfiguration(userCalendlyConfiguration);
        services
            .AddHttpClient<CalendlyClient>(
                client => CalendlyClient.ConfigureCalendlyHttpClient(client, calendlyConfiguration.AccessToken))
            .AddTransientHttpErrorPolicy(
                policyBuilder => policyBuilder.WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(i * 0.25)));

        services.AddSingleton(calendlyConfiguration);
        services.AddSingleton<ICalendlyIdService, CalendlyIdService>();
        services.AddScoped<ICalendlyService, CalendlyService>();
        
        services.TryAddScoped<IEventCreationHandler, EventCreationHandler>();
        services.TryAddScoped<IEventReschedulingHandler, EventReschedulingHandler>();
        services.TryAddScoped<IEventCancellationHandler, EventCancellationHandler>();

        services.AddHttpContextAccessor();

        services.AddScoped<IRequestContentAccessor, RequestContentAccessor>();
        services.AddScoped<IRequestProcessor, RequestProcessor>();
        services.AddScoped<IRequestSignatureAccessor, RequestSignatureAccessor>();
        services.AddScoped<IRequestValidator, RequestValidator>();
        services.AddScoped<ISignatureCalculator, SignatureCalculator>();

        if (!calendlyConfiguration.Webhook.SkipWebhookCreation || calendlyConfiguration.Webhook.CleanupAllExistingWebhooks)
        {
            services.AddHostedService<CalendlyWebhookRegistrationService>();
        }

        return services;
    }
    
    public static IEndpointRouteBuilder MapCalendlyWebhook(this IEndpointRouteBuilder endpoints)
    {
        var app = endpoints.CreateApplicationBuilder();
        var calendlyConfiguration = app.ApplicationServices.GetRequiredService<CalendlyConfiguration>();
        if (string.IsNullOrEmpty(calendlyConfiguration.Webhook.CallbackUrl))
        {
            return endpoints;
        }
            
        var webhookUrl = new Uri(calendlyConfiguration.Webhook.CallbackUrl);
        var webhookDelegate = app.UseMiddleware<RequestHandlerMiddleware>().Build();
        endpoints.MapPost(webhookUrl.AbsolutePath, webhookDelegate);

        return endpoints;
    }
}