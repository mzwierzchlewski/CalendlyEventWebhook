using Newtonsoft.Json;

namespace CalendlyEventWebhook.Webhook.Dtos;

#nullable disable

internal class WebhookDto
{
    [JsonProperty("event")]
    public Event Event { get; set; }

    [JsonProperty("payload")]
    public PayloadDto Payload { get; set; }
}

#nullable restore