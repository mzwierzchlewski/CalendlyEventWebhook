using Newtonsoft.Json;

namespace CalendlyEventWebhook.CalendlyApi.Dtos;

internal class WebhookListDto
{
    [JsonProperty("collection")]
    public IReadOnlyCollection<WebhookDto> Collection { get; set; }

    [JsonProperty("pagination")]
    public PaginationDto Pagination { get; set; }
}