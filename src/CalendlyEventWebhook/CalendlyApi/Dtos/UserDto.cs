using Newtonsoft.Json;

namespace CalendlyEventWebhook.CalendlyApi.Dtos;

#nullable disable

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

#nullable restore