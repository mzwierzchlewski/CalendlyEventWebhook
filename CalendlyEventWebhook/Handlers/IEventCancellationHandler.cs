using CalendlyEventWebhook.Models;

namespace CalendlyEventWebhook.Handlers;

public interface IEventCancellationHandler
{
    Task<bool> Handle(CalendlyResourceIdentifier id);
}