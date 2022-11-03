namespace CalendlyEventWebhook.Webhook;

internal interface IRequestSignatureAccessor
{
    (string Timestamp, string Signature)? GetTimestampAndSignature();
}