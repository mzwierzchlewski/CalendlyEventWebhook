using System.Runtime.Serialization;

namespace CalendlyEventWebhook.Webhook.Dtos;

internal enum Event
{
    [EnumMember(Value = "invitee.created")]
    EventCreated,

    [EnumMember(Value = "invitee.canceled")]
    EventCancelled,

    [EnumMember(Value = "routing_form_submission.created")]
    FormSubmissionCreated,
}