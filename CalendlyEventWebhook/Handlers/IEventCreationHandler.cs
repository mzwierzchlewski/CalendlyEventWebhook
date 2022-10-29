using CalendlyEventWebhook.Models;

namespace CalendlyEventWebhook.Handlers;

public interface IEventCreationHandler
{
    Task<bool> Handle(CalendlyEventDetails eventDetails);
}