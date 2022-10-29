using System.Runtime.Serialization;

namespace CalendlyEventWebhook.CalendlyApi.Dtos;

internal enum WebhookState
{
    [EnumMember(Value = "active")]
    Active,

    [EnumMember(Value = "disabled")]
    Disabled,
}