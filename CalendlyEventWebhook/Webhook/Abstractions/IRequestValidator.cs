namespace CalendlyEventWebhook.Webhook;

internal interface IRequestValidator
{
    Task<bool> Validate();
}