using CalendlyEventWebhook.Models;

namespace CalendlyEventWebhook.Services;

internal class CalendlyIdService
{
    public CalendlyResourceIdentifier? GetIdFromOrganisationUri(string organisationUri) => GetIdFromLatUriSegment(organisationUri);

    private CalendlyResourceIdentifier? GetIdFromLatUriSegment(string uri)
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