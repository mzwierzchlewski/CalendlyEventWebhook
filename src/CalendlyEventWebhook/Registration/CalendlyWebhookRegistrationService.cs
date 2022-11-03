using CalendlyEventWebhook.Configuration;
using CalendlyEventWebhook.Models;
using CalendlyEventWebhook.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CalendlyEventWebhook.Registration;

internal class CalendlyWebhookRegistrationService : BackgroundService
{
    private readonly CalendlyConfiguration _configuration;

    private readonly ILogger<CalendlyWebhookRegistrationService> _logger;

    private readonly IServiceScopeFactory _serviceScopeFactory;

    public CalendlyWebhookRegistrationService(CalendlyConfiguration configuration, IServiceScopeFactory serviceScopeFactory, ILogger<CalendlyWebhookRegistrationService> logger)
    {
        _configuration = configuration;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrEmpty(_configuration.Webhook.CallbackUrl))
        {
            return;
        }

        using var scope = _serviceScopeFactory.CreateScope();
        var calendlyService = scope.ServiceProvider.GetRequiredService<ICalendlyService>();

        await HandleExistingSubscriptions(calendlyService, stoppingToken);
        await HandleNewSubscription(calendlyService);
    }

    private async Task HandleExistingSubscriptions(ICalendlyService calendlyService, CancellationToken stoppingToken)
    {
        async Task RemoveWebhookSubscription(CalendlyWebhook webhook)
        {
            var removalStatus = await calendlyService.DeleteWebhookSubscription(webhook.Id);
            if (!removalStatus)
            {
                _logger.LogWarning("Failed to remove existing webhook with uri: {Uri}", webhook.Id.Uri);
            }
        }

        var webhookEvents = GetWebhookEvents();
        await foreach (var webhook in calendlyService.ListWebhookSubscriptions(stoppingToken))
        {
            if (_configuration.Webhook.CleanupAllExistingWebhooks)
            {
                await RemoveWebhookSubscription(webhook);
                continue;
            }

            if (!IsMatchingWebhook(webhook))
            {
                continue;
            }

            if (ShouldBeRemoved(webhook, webhookEvents))
            {
                await RemoveWebhookSubscription(webhook);
            }
            else
            {
                return;
            }
        }
    }

    private async Task HandleNewSubscription(ICalendlyService calendlyService)
    {
        if (_configuration.Webhook.SkipWebhookCreation)
        {
            return;
        }

        var creationStatus = await calendlyService.CreateWebhookSubscription(_configuration.Webhook.CallbackUrl, _configuration.Webhook.SigningKey, GetWebhookEvents());
        if (!creationStatus)
        {
            _logger.LogError("Failed to create webhook for callback url: {CallbackUrl}", _configuration.Webhook.CallbackUrl);
        }
    }

    private IReadOnlyCollection<CalendlyWebhookEvent> GetWebhookEvents() => _configuration.Webhook.EventCreation
        ? new[]
        {
            CalendlyWebhookEvent.EventCreated, CalendlyWebhookEvent.EventCancelled,
        }
        : new[]
        {
            CalendlyWebhookEvent.EventCancelled,
        };

    private bool IsMatchingWebhook(CalendlyWebhook webhook)
        => webhook.CallbackUrl == _configuration.Webhook.CallbackUrl;

    private static bool ShouldBeRemoved(CalendlyWebhook webhook, IReadOnlyCollection<CalendlyWebhookEvent> events) 
        => webhook.State == CalendlyWebhookState.Disabled 
           || webhook.Events.Count == events.Count && !webhook.Events.All(events.Contains);
}