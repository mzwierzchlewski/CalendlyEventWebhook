using CalendlyEventWebhook.Webhook.Dtos;

namespace CalendlyEventWebhook.Webhook;

internal interface IRequestContentAccessor
{
    Task<string?> GetString();

    Task<WebhookDto?> GetDto();
}