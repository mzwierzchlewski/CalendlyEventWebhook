using Newtonsoft.Json;

namespace CalendlyEventWebhook.Webhook.Dtos;

#nullable disable

internal class PayloadDto
{
    [JsonProperty("event")]
    public string EventUri { get; set; }

    [JsonProperty("new_invitee")]
    public string NewInviteeUri { get; set; }
    
    [JsonProperty("old_invitee")]
    public string OldInviteeUri { get; set; }

    [JsonProperty("rescheduled")]
    public bool Rescheduled { get; set; }
}

#nullable restore