using System.Runtime.Serialization;

namespace CalendlyEventWebhook.CalendlyApi.Dtos;

internal enum WebhookEvent
{
    [EnumMember(Value = "invitee.created")]
    EventCreated,

    [EnumMember(Value = "invitee.canceled")]
    EventCancelled,

    [EnumMember(Value = "routing_form_submission.created")]
    FormSubmissionCreated,
}