using System.Runtime.Serialization;

namespace CalendlyEventWebhook.CalendlyApi.Dtos;

internal enum WebhookScope
{
    [EnumMember(Value = "user")]
    User,

    [EnumMember(Value = "organization")]
    Organisation,
}