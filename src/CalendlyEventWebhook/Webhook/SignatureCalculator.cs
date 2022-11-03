using System.Security.Cryptography;
using System.Text;
using SmartFormat;

namespace CalendlyEventWebhook.Webhook;

internal class SignatureCalculator : ISignatureCalculator
{
    public record SignatureDataParameters(string Timestamp, string Body);

    public string Calculate(string requestTimestamp, string requestBody, string signingKey)
    {
        var signatureData = Smart.Format(Constants.WebhookSignature.DataFormat, new SignatureDataParameters(requestTimestamp, requestBody));
        var signatureDataBytes = Encoding.UTF8.GetBytes(signatureData);
        var signingKeyBytes = Encoding.ASCII.GetBytes(signingKey);
        using var hmac = new HMACSHA256(signingKeyBytes);
        return Convert.ToHexString(hmac.ComputeHash(signatureDataBytes));
    }
}