namespace CalendlyEventWebhook.Webhook;

internal interface ISignatureCalculator
{
    string Calculate(string requestTimestamp, string requestBody, string signingKey);
}