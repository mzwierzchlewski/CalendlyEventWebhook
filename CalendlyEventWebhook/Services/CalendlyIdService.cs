using CalendlyEventWebhook.Models;

namespace CalendlyEventWebhook.Services;

internal class CalendlyIdService : ICalendlyIdService
{
    public CalendlyResourceIdentifier? GetIdFromEventUri(string eventUri) => GetIdFromLastUriSegment(eventUri);

    public CalendlyResourceIdentifier? GetIdFromWebhookUri(string webhookUri) => GetIdFromLastUriSegment(webhookUri);

    public CalendlyResourceIdentifier? GetIdFromUserUri(string userUri) => GetIdFromLastUriSegment(userUri);

    public CalendlyResourceIdentifier? GetIdFromOrganisationUri(string organisationUri) => GetIdFromLastUriSegment(organisationUri);

    public CalendlyResourceIdentifier? GetIdFromInviteeUri(string inviteeUri) => GetIdFromNthUriSegment(inviteeUri, 2);

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
    
    private CalendlyResourceIdentifier? GetIdFromNthUriSegment(string uri, int n)
    {
        var uriObject = new Uri(uri);
        if (n > uriObject.Segments.Length)
        {
            return null;
        }

        var id = uriObject.Segments[n].TrimEnd('/');
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