﻿using System.Runtime.CompilerServices;
using CalendlyEventWebhook.CalendlyApi;
using CalendlyEventWebhook.CalendlyApi.Dtos;
using CalendlyEventWebhook.Configuration;
using CalendlyEventWebhook.Models;

[assembly: InternalsVisibleTo("CalendlyEventWebhookConsole")]

namespace CalendlyEventWebhook.Services;

internal class CalendlyService
{
    private static CalendlyResourceIdentifier? _currentUserIdCache;

    private static readonly SemaphoreSlim CurrentUserIdCacheLock = new (1, 1);

    private readonly CalendlyConfiguration _configuration;
    
    private readonly CalendlyClient _client;

    private readonly CalendlyIdService _calendlyIdService;

    public CalendlyService(CalendlyConfiguration configuration, CalendlyClient client, CalendlyIdService calendlyIdService)
    {
        _configuration = configuration;
        _client = client;
        _calendlyIdService = calendlyIdService;
    }

    public async Task<CalendlyEventDetails> GetEventDetails(Guid eventId)
    {
        var eventDto = await _client.GetEvent(eventId);
        if (eventDto == null)
        {
            throw new InvalidOperationException("Failed to get event information");
        }

        var id = _calendlyIdService.GetIdFromEventUri(eventDto.Resource.Uri) ?? throw new InvalidOperationException("Failed to retrieve identifier from event uri");
        return new CalendlyEventDetails(id, eventDto.Resource.StartTime, eventDto.Resource.EndTime);
    } 
    
    public async Task<bool> CreateWebhookSubscription(string callbackUrl, string signingKey, params CalendlyWebhookEvent[] events)
    {
        var userId = await GetCurrentUserId();

        return await _client.CreateWebhookSubscription(callbackUrl, _configuration.Scope.ToWebhookScope(), userId.Uri, signingKey, events.ToWebhookEvents());
    }
    
    public async IAsyncEnumerable<CalendlyWebhook> ListWebhookSubscriptions([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var userId = await GetCurrentUserId();
        await foreach (var webhook in _client.ListWebhookSubscriptions(_configuration.Scope.ToWebhookScope(), userId.Uri, cancellationToken))
        {
            var webhookId = _calendlyIdService.GetIdFromWebhookUri(webhook.Uri) ?? throw new InvalidOperationException("Failed to retrieve identifier from webhook uri");
            yield return new CalendlyWebhook(webhookId, webhook.CallbackUrl, webhook.State.ToCalendlyWebhookState(), webhook.Events.ToCalendlyWebhookEvents());
        }
    }
    
    public async Task<bool> DeleteWebhookSubscription(CalendlyResourceIdentifier webhookId) => await _client.DeleteWebhookSubscription(webhookId.Uri);

    private async Task<CalendlyResourceIdentifier> GetCurrentUserId()
    {
        if (_currentUserIdCache != null)
        {
            return _currentUserIdCache;
        }
        
        await CurrentUserIdCacheLock.WaitAsync();
        if (_currentUserIdCache == null)
        {
            var currentUserDto = await _client.GetCurrentUser();
            if (currentUserDto != null)
            {
                _currentUserIdCache = _configuration.Scope switch
                {
                    CalendlyScope.User         => _calendlyIdService.GetIdFromUserUri(currentUserDto.Resource.Uri),
                    CalendlyScope.Organisation => _calendlyIdService.GetIdFromOrganisationUri(currentUserDto.Resource.OrganizationUri),
                    _                          => throw new ArgumentOutOfRangeException(nameof(_configuration.Scope), "Invalid Calendly scope"),
                };
            }
        }
        
        CurrentUserIdCacheLock.Release();
        
        return _currentUserIdCache ?? throw new InvalidOperationException("Failed to get current user information");
    }
}

internal static class CalendlyExtensions
{
    public static WebhookScope ToWebhookScope(this CalendlyScope scope) => scope switch
    {
        CalendlyScope.User         => WebhookScope.User,
        CalendlyScope.Organisation => WebhookScope.Organisation,
        _                                 => throw new ArgumentOutOfRangeException(nameof(scope), scope, "Invalid Calendly webhook scope"),
    };
    
    public static CalendlyWebhookState ToCalendlyWebhookState(this WebhookState state) => state switch
    {
        WebhookState.Active   => CalendlyWebhookState.Active,
        WebhookState.Disabled => CalendlyWebhookState.Disabled,
        _                     => throw new ArgumentOutOfRangeException(nameof(state), state, "Invalid Calendly webhook state"),
    };

    public static IReadOnlyCollection<CalendlyWebhookEvent> ToCalendlyWebhookEvents(this IReadOnlyCollection<WebhookEvent> events)
        => events.Select(
            @event => @event switch
            {
                WebhookEvent.EventCancelled        => CalendlyWebhookEvent.EventCancelled,
                WebhookEvent.EventCreated          => CalendlyWebhookEvent.EventCreated,
                WebhookEvent.FormSubmissionCreated => CalendlyWebhookEvent.FormSubmissionCreated,
                _                                  => throw new ArgumentOutOfRangeException(nameof(@event), @event, "Invalid Calendly webhook event"),
            }).ToArray();

    public static IReadOnlyCollection<WebhookEvent> ToWebhookEvents(this IReadOnlyCollection<CalendlyWebhookEvent> events)
        => events.Select(
            @event => @event switch
            {
                CalendlyWebhookEvent.EventCancelled        => WebhookEvent.EventCancelled,
                CalendlyWebhookEvent.EventCreated          => WebhookEvent.EventCreated,
                CalendlyWebhookEvent.FormSubmissionCreated => WebhookEvent.FormSubmissionCreated,
                _                                          => throw new ArgumentOutOfRangeException(nameof(@event), @event, "Invalid Calendly webhook event"),
            }).ToArray();
}