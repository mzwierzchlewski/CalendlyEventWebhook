using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CalendlyEventWebhook.CalendlyApi.Dtos;

#nullable disable

internal class WebhookDto
{
    [JsonProperty("uri")]
    public string Uri { get; set; }

    [JsonProperty("callback_url")]
    public string CallbackUrl { get; set; }

    [JsonProperty("state")]
    [JsonConverter(typeof(StringEnumConverter))]
    public WebhookState State { get; set; }

    [JsonProperty("events", ItemConverterType = typeof(StringEnumConverter))]
    public IReadOnlyCollection<WebhookEvent> Events { get; set; }
}

#nullable restore