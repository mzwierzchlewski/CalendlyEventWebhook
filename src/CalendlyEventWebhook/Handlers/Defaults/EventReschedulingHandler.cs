using CalendlyEventWebhook.Models;
using Microsoft.Extensions.Logging;

namespace CalendlyEventWebhook.Handlers.Defaults;

public class EventReschedulingHandler : IEventReschedulingHandler
{
    private readonly ILogger<EventReschedulingHandler> _logger;

    public EventReschedulingHandler(ILogger<EventReschedulingHandler> logger)
    {
        _logger = logger;
    }

    public Task<bool> Handle(CalendlyResourceIdentifier oldEventId, CalendlyEventDetails newEventDetails)
    {
        _logger.LogInformation("Event {OldEventId} rescheduled as {NewEventId} from {NewStartTime} to {NewEndTime}", oldEventId.Id, newEventDetails.Id, newEventDetails.StartTime, newEventDetails.EndTime);
        return Task.FromResult(true);
    }
}