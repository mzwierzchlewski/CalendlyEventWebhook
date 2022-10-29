﻿using CalendlyEventWebhook.Webhook.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CalendlyEventWebhook.Webhook;

internal class RequestContentAccessor : IRequestContentAccessor
{
    private string? _requestBody;
    
    private static readonly SemaphoreSlim RequestBodyLock = new(1);
    
    private WebhookDto? _requestDto;

    private static readonly SemaphoreSlim RequestDtoLock = new(1);

    private readonly IHttpContextAccessor _httpContextAccessor;
    
    private readonly ILogger<RequestContentAccessor> _logger;

    public RequestContentAccessor(IHttpContextAccessor httpContextAccessor, ILogger<RequestContentAccessor> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<string?> GetString()
    {
        if (string.IsNullOrEmpty(_requestBody))
        {
            try
            {
                await RequestBodyLock.WaitAsync();
                if (!string.IsNullOrEmpty(_requestBody))
                {
                    return _requestBody;
                }

                var request = _httpContextAccessor.HttpContext?.Request;
                if (request == null)
                {
                    _logger.LogWarning("Failed to read Calendly webhook request - HttpContext is null");
                    return null;
                }

                if (!request.Body.CanSeek)
                {
                    request.EnableBuffering();
                }

                request.Body.Seek(0, SeekOrigin.Begin);
                using (var sr = new StreamReader(request.Body, leaveOpen: true))
                {
                    _requestBody = await sr.ReadToEndAsync();
                }

                request.Body.Seek(0, SeekOrigin.Begin);
                RequestBodyLock.Release();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read Calendly webhook request");
                return null;
            }
        }

        return _requestBody;
    }

    public async Task<WebhookDto?> GetDto()
    {
        if (_requestDto == null)
        {
            try
            {
                await RequestDtoLock.WaitAsync();
                if (_requestDto != null)
                {
                    return _requestDto;
                }

                var serializedBody = await GetString();
                if (string.IsNullOrEmpty(serializedBody))
                {
                    return null;
                }

                _requestDto = JsonConvert.DeserializeObject<WebhookDto>(serializedBody);
                RequestDtoLock.Release();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get Calendly webhook dto");
                return null;
            }
        }

        return _requestDto;
    }
}