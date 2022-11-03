namespace CalendlyEventWebhook.Models;

public record CalendlyEventDetails(CalendlyResourceIdentifier Id, DateTime StartTime, DateTime EndTime);