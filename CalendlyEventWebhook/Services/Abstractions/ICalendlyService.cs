using CalendlyEventWebhook.Models;

namespace CalendlyEventWebhook.Services;

internal interface ICalendlyService
{
    Task<CalendlyEventDetails> GetEventDetails(Guid eventId);

    Task<bool> CreateWebhookSubscription(string callbackUrl, string signingKey, IReadOnlyCollection<CalendlyWebhookEvent> events);

    IAsyncEnumerable<CalendlyWebhook> ListWebhookSubscriptions(CancellationToken cancellationToken);

    Task<bool> DeleteWebhookSubscription(CalendlyResourceIdentifier webhookId);
}