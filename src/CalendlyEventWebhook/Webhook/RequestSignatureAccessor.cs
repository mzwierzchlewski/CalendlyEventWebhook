using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CalendlyEventWebhook.Webhook;

internal class RequestSignatureAccessor : IRequestSignatureAccessor
{
    private static readonly Regex SignatureRegex = new(Constants.WebhookSignature.Header.Regex.Pattern, RegexOptions.Compiled);

    private readonly IHttpContextAccessor _httpContextAccessor;

    private readonly ILogger<RequestSignatureAccessor> _logger;

    public RequestSignatureAccessor(IHttpContextAccessor httpContextAccessor, ILogger<RequestSignatureAccessor> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public (string Timestamp, string Signature)? GetTimestampAndSignature()
    {
        var headerValue = _httpContextAccessor.HttpContext?.Request.Headers[Constants.WebhookSignature.Header.Key];
        if (string.IsNullOrEmpty(headerValue))
        {
            _logger.LogWarning("Missing Calendly webhook signature header");
            return null;
        }

        var headerMatches = SignatureRegex.Matches(headerValue);
        if (headerMatches.Count != 1)
        {
            _logger.LogWarning("Invalid Calendly webhook signature header format");
            return null;
        }

        var headerMatch = headerMatches.First();
        var timestampMatch = headerMatch.Groups[Constants.WebhookSignature.Header.Regex.TimestampGroupKey];
        var signatureMatch = headerMatch.Groups[Constants.WebhookSignature.Header.Regex.SignatureGroupKey];
        if (!timestampMatch.Success || !signatureMatch.Success)
        {
            _logger.LogWarning("Invalid Calendly webhook signature header format");
            return null;
        }

        return (timestampMatch.Value, signatureMatch.Value);
    }
}