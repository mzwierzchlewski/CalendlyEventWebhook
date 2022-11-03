namespace CalendlyEventWebhook.Webhook;

internal interface IRequestProcessor
{
    Task<bool> Process();
}