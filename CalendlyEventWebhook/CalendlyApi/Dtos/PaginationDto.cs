using Newtonsoft.Json;

namespace CalendlyEventWebhook.CalendlyApi.Dtos;

internal class PaginationDto
{
    [JsonProperty("next_page")]
    public string NextPageUrl { get; set; }
}