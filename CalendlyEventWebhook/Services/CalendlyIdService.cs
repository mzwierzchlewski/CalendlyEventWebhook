using CalendlyEventWebhook.Models;

namespace CalendlyEventWebhook.Services;

internal class CalendlyIdService
{
    public CalendlyResourceIdentifier? GetIdFromEventUri(string eventUri) => GetIdFromLastUriSegment(eventUri);

    public CalendlyResourceIdentifier? GetIdFromWebhookUri(string webhookUri) => GetIdFromLastUriSegment(webhookUri);

    public CalendlyResourceIdentifier? GetIdFromUserUri(string userUri) => GetIdFromLastUriSegment(userUri);

    public CalendlyResourceIdentifier? GetIdFromOrganisationUri(string organisationUri) => GetIdFromLastUriSegment(organisationUri);

    private CalendlyResourceIdentifier? GetIdFromLastUriSegment(string uri)
    {
        var id = uri.Split('/').LastOrDefault();
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        if (!Guid.TryParse(id, out var guid))
        {
            return null;
        }

        return new CalendlyResourceIdentifier(guid, uri);
    }
}