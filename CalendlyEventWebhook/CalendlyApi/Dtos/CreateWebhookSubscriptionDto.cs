using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CalendlyEventWebhook.CalendlyApi.Dtos;

#nullable disable

internal class CreateWebhookSubscriptionDto
{
    [JsonProperty("url")]
    public string CallbackUrl { get; private init; }

    [JsonProperty("scope")]
    [JsonConverter(typeof(StringEnumConverter))]
    public WebhookScope Scope { get; private init; }

    [JsonProperty("events", ItemConverterType = typeof(StringEnumConverter))]
    public IReadOnlyCollection<WebhookEvent> Events { get; private init; }

    [JsonProperty("user")]
    public string User { get; private init; }

    [JsonProperty("organization")]
    public string Organisation { get; private init; }

    [JsonProperty("signing_key")]
    public string SigningKey { get; private set; }

    public static CreateWebhookSubscriptionDto OrganisationDto(string callbackUrl, string organisationUri, string signingKey, IReadOnlyCollection<WebhookEvent> events)
        => new()
        {
            CallbackUrl = callbackUrl,
            Scope = WebhookScope.Organisation,
            Organisation = organisationUri,
            Events = events,
            SigningKey = signingKey,
        };

    public static CreateWebhookSubscriptionDto UserDto(string callbackUrl, string userUri, string signingKey, IReadOnlyCollection<WebhookEvent> events)
        => new()
        {
            CallbackUrl = callbackUrl,
            Scope = WebhookScope.User,
            User = userUri,
            Events = events,
            SigningKey = signingKey,
        };
}

#nullable restore