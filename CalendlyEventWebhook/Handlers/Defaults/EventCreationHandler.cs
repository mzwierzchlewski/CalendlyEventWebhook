using CalendlyEventWebhook.Models;
using Microsoft.Extensions.Logging;

namespace CalendlyEventWebhook.Handlers.Defaults;

public class EventCreationHandler : IEventCreationHandler
{
    private readonly ILogger<EventCreationHandler> _logger;

    public EventCreationHandler(ILogger<EventCreationHandler> logger)
    {
        _logger = logger;
    }

    public Task<bool> Handle(CalendlyEventDetails eventDetails)
    {
        _logger.LogInformation("Event {EventId} from {StartTime} to {EndTime} created", eventDetails.Id.Id, eventDetails.StartTime, eventDetails.EndTime);
        return Task.FromResult(true);
    }
}