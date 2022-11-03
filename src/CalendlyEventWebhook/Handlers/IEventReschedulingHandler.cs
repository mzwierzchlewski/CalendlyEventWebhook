using CalendlyEventWebhook.Models;

namespace CalendlyEventWebhook.Handlers;

public interface IEventReschedulingHandler
{
    Task<bool> Handle(CalendlyResourceIdentifier oldEventId, CalendlyEventDetails newEventDetails);
}