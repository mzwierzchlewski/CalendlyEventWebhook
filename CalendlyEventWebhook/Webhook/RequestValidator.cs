using CalendlyEventWebhook.Configuration;
using Microsoft.Extensions.Logging;

namespace CalendlyEventWebhook.Webhook;

internal class RequestValidator : IRequestValidator
{
    private readonly string _signingKey;

    private readonly IRequestContentAccessor _webhookRequestContentAccessor;

    private readonly IRequestSignatureAccessor _webhookRequestSignatureAccessor;

    private readonly ISignatureCalculator _webhookSignatureCalculator;
    
    private readonly ILogger<RequestValidator> _logger;

    public RequestValidator(
        CalendlyConfiguration configuration, 
        IRequestSignatureAccessor webhookRequestSignatureAccessor, 
        IRequestContentAccessor webhookRequestContentAccessor, 
        ISignatureCalculator webhookSignatureCalculator, 
        ILogger<RequestValidator> logger)
    {
        _signingKey = configuration.Webhook.SigningKey;
        _webhookRequestContentAccessor = webhookRequestContentAccessor;
        _webhookRequestSignatureAccessor = webhookRequestSignatureAccessor;
        _webhookSignatureCalculator = webhookSignatureCalculator;
        _logger = logger;
    }

    public async Task<bool> Validate()
    {
        if (!await ValidateSignature())
        {
            _logger.LogWarning("Failed to verify signature of Calendly webhook message");
            return false;
        }

        if (!await ValidateBody())
        {
            _logger.LogWarning("Failed to read body of Calendly webhook message");
            return false;
        }

        return true;
    }

    private async Task<bool> ValidateBody()
    {
        var dto = await _webhookRequestContentAccessor.GetDto();
        if (dto == null)
        {
            _logger.LogWarning("Failed to get Calendly webhook request dto");
            return false;
        }

        if (string.IsNullOrEmpty(dto.Payload?.EventUri) || string.IsNullOrEmpty(dto.Payload?.NewInviteeUri))
        {
            _logger.LogWarning("Invalid values of Calendly webhook request dto");
            return false;
        }

        return true;
    }

    private async Task<bool> ValidateSignature()
    {
        var requestSignature = _webhookRequestSignatureAccessor.GetTimestampAndSignature();
        if (!requestSignature.HasValue)
        {
            _logger.LogWarning("Failed to get Calendly webhook request signature");
            return false;
        }

        var requestTimestamp = requestSignature.Value.Timestamp;
        var requestContent = await _webhookRequestContentAccessor.GetString();
        if (string.IsNullOrEmpty(requestContent))
        {
            _logger.LogWarning("Failed to get Calendly webhook request content");
            return false;
        }

        var calculatedSignature = _webhookSignatureCalculator.Calculate(requestTimestamp, requestContent, _signingKey);

        return calculatedSignature == requestSignature.Value.Signature;
    }
}