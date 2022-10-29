﻿using CalendlyEventWebhook.Handlers;
using CalendlyEventWebhook.Services;
using CalendlyEventWebhook.Webhook.Dtos;
using Microsoft.Extensions.Logging;

namespace CalendlyEventWebhook.Webhook;

internal class RequestProcessor : IRequestProcessor
{
    private readonly IRequestContentAccessor _requestContentAccessor;

    private readonly ICalendlyService _calendlyService;

    private readonly ICalendlyIdService _calendlyIdService;
    
    private readonly IEventCreationHandler _eventCreationHandler;

    private readonly IEventReschedulingHandler _eventReschedulingHandler;

    private readonly IEventCancellationHandler _eventCancellationHandler;

    private readonly ILogger<RequestProcessor> _logger;

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

        return (dto.Event, dto.Payload.Rescheduled) switch
        {
            (Event.EventCreated, false)   => await ProcessEventCreation(dto),
            (Event.EventCancelled, true)  => await ProcessEventRescheduling(dto),
            (Event.EventCancelled, false) => await ProcessEventCancellation(dto),
            _                             => ProcessInvalidPayload(),
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

    private bool ProcessInvalidPayload()
    {
        _logger.LogWarning("Invalid webhook request payload (event/rescheduled combination)");
        return false;
    }
}