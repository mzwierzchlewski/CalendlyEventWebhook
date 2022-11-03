using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CalendlyEventWebhook.Webhook;

internal class RequestHandlerMiddleware
{
    [SuppressMessage("ReSharper", "NotAccessedField.Local", Justification = "Middleware handles the request and returns a response")]
    private readonly RequestDelegate _next;

    private readonly ILogger<RequestHandlerMiddleware> _logger;

    public RequestHandlerMiddleware(RequestDelegate next, ILogger<RequestHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext, IRequestValidator requestValidator, IRequestProcessor requestProcessor)
    {
        var validateResult = await requestValidator.Validate();
        if (!validateResult)
        {
            _logger.LogError("Failed to verify Calendly webhook signature");
            await Forbidden(httpContext);
            return;
        }

        var processResult = await requestProcessor.Process();
        if (!processResult)
        {
            _logger.LogError("Failed to process Calendly webhook request");
            await BadRequest(httpContext);
            return;
        }

        await Ok(httpContext);
    }

    private static async Task Forbidden(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
        await httpContext.Response.CompleteAsync();
    }

    private static async Task BadRequest(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
        await httpContext.Response.CompleteAsync();
    }

    private static async Task Ok(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = StatusCodes.Status200OK;
        await httpContext.Response.CompleteAsync();
    }
}