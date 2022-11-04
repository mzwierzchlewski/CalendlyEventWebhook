using CalendlyEventWebhook.Configuration;
using CalendlyEventWebhook.Webhook.Dtos;
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
        if (ShouldValidateSignature() && !await ValidateSignature())
        {
            _logger.LogWarning("Failed to verify signature of Calendly webhook message");
            return false;
        }

        if (!await ValidateBody())
        {
            _logger.LogWarning("Failed to validate body of Calendly webhook message");
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

        if (string.IsNullOrEmpty(dto.Payload?.EventUri) ||
            (dto.Event == Event.EventCancelled && dto.Payload.Rescheduled && string.IsNullOrEmpty(dto.Payload.NewInviteeUri)))
        {
            _logger.LogWarning("Invalid values of Calendly webhook request dto");
            return false;
        }

        return true;
    }

    private bool ShouldValidateSignature() => !string.IsNullOrEmpty(_signingKey);

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

        return string.Compare(calculatedSignature, requestSignature.Value.Signature, StringComparison.InvariantCultureIgnoreCase) == 0;
    }
}