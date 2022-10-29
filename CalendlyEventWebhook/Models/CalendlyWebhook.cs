namespace CalendlyEventWebhook.Models;

internal record CalendlyWebhook(CalendlyResourceIdentifier Id, string CallbackUrl, CalendlyWebhookState State, IReadOnlyCollection<CalendlyWebhookEvent> Events);