using CalendlyEventWebhook.Models;

namespace CalendlyEventWebhook.Services;

internal interface ICalendlyIdService
{
    CalendlyResourceIdentifier? GetIdFromEventUri(string eventUri);

    CalendlyResourceIdentifier? GetIdFromWebhookUri(string webhookUri);

    CalendlyResourceIdentifier? GetIdFromUserUri(string userUri);

    CalendlyResourceIdentifier? GetIdFromOrganisationUri(string organisationUri);

    CalendlyResourceIdentifier? GetEventIdFromInviteeUri(string inviteeUri);
}