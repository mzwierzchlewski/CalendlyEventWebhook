using CalendlyEventWebhook.Models;
using Microsoft.Extensions.Logging;

namespace CalendlyEventWebhook.Handlers.Defaults;

internal class EventCancellationHandler : IEventCancellationHandler
{
    private readonly ILogger<EventCancellationHandler> _logger;

    public EventCancellationHandler(ILogger<EventCancellationHandler> logger)
    {
        _logger = logger;
    }

    public Task<bool> Handle(CalendlyResourceIdentifier id)
    {
        _logger.LogInformation("Event {EventId} cancelled", id.Id);
        return Task.FromResult(true);
    }
}