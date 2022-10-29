using Newtonsoft.Json;

namespace CalendlyEventWebhook.CalendlyApi.Dtos;

internal class EventDto
{
    [JsonProperty("resource")]
    public ResourceDto Resource { get; set; }

    public class ResourceDto
    {
        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("start_time")]
        public DateTime StartTime { get; set; }

        [JsonProperty("end_time")]
        public DateTime EndTime { get; set; }
    }
}