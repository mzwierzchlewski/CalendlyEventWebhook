using Newtonsoft.Json;

namespace CalendlyEventWebhook.CalendlyApi.Dtos;

internal class UserDto
{
    [JsonProperty("resource")]
    public ResourceDto Resource { get; set; }

    public class ResourceDto
    {
        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("current_organization")]
        public string OrganizationUri { get; set; }
    }
}