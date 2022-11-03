using CalendlyEventWebhook.Handlers;
using CalendlyEventWebhook.Services;
using CalendlyEventWebhook.Webhook.Dtos;
using Microsoft.Extensions.Logging;

namespace CalendlyEventWebhook.Webhook;

internal class RequestProcessor : IRequestProcessor
{
    private readonly ICalendlyIdService _calendlyIdService;

    private readonly ICalendlyService _calendlyService;

    private readonly IEventCancellationHandler _eventCancellationHandler;

    private readonly IEventCreationHandler _eventCreationHandler;

    private readonly IEventReschedulingHandler _eventReschedulingHandler;

    private readonly ILogger<RequestProcessor> _logger;
    private readonly IRequestContentAccessor _requestContentAccessor;

    public RequestProcessor(IRequestContentAccessor requestContentAccessor, ICalendlyService calendlyService, ICalendlyIdService calendlyIdService, IEventCreationHandler eventCreationHandler, IEventReschedulingHandler eventReschedulingHandler, IEventCancellationHandler eventCancellationHandler, ILogger<RequestProcessor> logger)
    {
        _requestContentAccessor = requestContentAccessor;
        _calendlyIdService = calendlyIdService;
        _eventCreationHandler = eventCreationHandler;
        _eventReschedulingHandler = eventReschedulingHandler;
        _eventCancellationHandler = eventCancellationHandler;
        _logger = logger;
        _calendlyService = calendlyService;
    }

    public async Task<bool> Process()
    {
        var dto = await _requestContentAccessor.GetDto();
        if (dto == null)
        {
            return false;
        }

        return (dto.Event, IsRescheduling(dto)) switch
        {
            (Event.EventCreated, false)   => await ProcessEventCreation(dto),
            (Event.EventCancelled, true)  => await ProcessEventRescheduling(dto),
            (Event.EventCancelled, false) => await ProcessEventCancellation(dto),
            _                             => true,
        };
    }

    private async Task<bool> ProcessEventCreation(WebhookDto dto)
    {
        var id = _calendlyIdService.GetIdFromEventUri(dto.Payload.EventUri);
        if (id == null)
        {
            return false;
        }

        var eventDetails = await _calendlyService.GetEventDetails(id.Id);

        return await _eventCreationHandler.Handle(eventDetails);
    }

    private async Task<bool> ProcessEventRescheduling(WebhookDto dto)
    {
        var oldId = _calendlyIdService.GetIdFromEventUri(dto.Payload.EventUri);
        if (oldId == null)
        {
            return false;
        }

        var newId = _calendlyIdService.GetIdFromInviteeUri(dto.Payload.NewInviteeUri);
        if (newId == null)
        {
            return false;
        }

        var newEventDetails = await _calendlyService.GetEventDetails(newId.Id);

        return await _eventReschedulingHandler.Handle(oldId, newEventDetails);
    }

    private async Task<bool> ProcessEventCancellation(WebhookDto dto)
    {
        var id = _calendlyIdService.GetIdFromEventUri(dto.Payload.EventUri);
        if (id == null)
        {
            return false;
        }

        return await _eventCancellationHandler.Handle(id);
    }

    private static bool IsRescheduling(WebhookDto dto)
    {
        if (dto.Event == Event.EventCancelled && dto.Payload.Rescheduled)
        {
            return true;
        }

        if (dto.Event == Event.EventCreated && !string.IsNullOrEmpty(dto.Payload.OldInviteeUri))
        {
            return true;
        }

        return false;
    }
}