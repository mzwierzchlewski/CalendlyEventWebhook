using Newtonsoft.Json;

namespace CalendlyEventWebhook.CalendlyApi.Dtos;

#nullable disable

internal class PaginationDto
{
    [JsonProperty("next_page")]
    public string NextPageUrl { get; set; }
}

#nullable restore