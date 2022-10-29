using System.Runtime.CompilerServices;
using CalendlyEventWebhook.CalendlyApi.Dtos;
using CalendlyEventWebhook.Models;

[assembly: InternalsVisibleTo("CalendlyEventWebhookConsole")]

namespace CalendlyEventWebhook.Services;

internal class CalendlyService
{
    public async Task<CalendlyEventDetails> GetEventDetails(Guid eventId) => new CalendlyEventDetails(new CalendlyResourceIdentifier(Guid.Empty, string.Empty), DateTime.UtcNow, DateTime.UtcNow);
}

internal static class CalendlyExtensions
{
    public static WebhookScope ToWebhookScope(this CalendlyWebhookScope scope) => scope switch
    {
        CalendlyWebhookScope.User         => WebhookScope.User,
        CalendlyWebhookScope.Organisation => WebhookScope.Organisation,
        _                                 => throw new ArgumentOutOfRangeException(nameof(scope), scope, "Invalid Calendly webhook scope"),
    };
}