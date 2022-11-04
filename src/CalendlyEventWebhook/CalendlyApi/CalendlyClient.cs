using System.Net.Http.Headers;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text;
using CalendlyEventWebhook.CalendlyApi.Dtos;
using CalendlyEventWebhook.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CalendlyEventWebhook.CalendlyApi;

internal class CalendlyClient
{
    private static readonly JsonSerializerSettings JsonSerializerSettings = GetJsonSerializerSettings();

    private readonly HttpClient _httpClient;

    private readonly ILogger<CalendlyClient> _logger;

    public CalendlyClient(HttpClient httpClient, ILogger<CalendlyClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<EventDto?> GetEvent(Guid eventId)
    {
        try
        {
            var url = string.Format(Constants.Calendly.CalendlyEventUrlFormat, eventId);
            var response = await _httpClient.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Invalid status code ({StatusCode}) when getting event with id: {EventId}; Response: {ResponseContent}", response.StatusCode, eventId, responseContent);
                return null;
            }

            return JsonConvert.DeserializeObject<EventDto>(responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception when getting event with id: {EventId}", eventId);
            return null;
        }
    }

    public async Task<UserDto?> GetCurrentUser()
    {
        try
        {
            var response = await _httpClient.GetAsync(Constants.Calendly.CalendlyCurrentUserUrl);
            var responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Invalid status code ({StatusCode}) when getting current user; Response: {ResponseContent}", response.StatusCode, responseContent);
                return null;
            }

            return JsonConvert.DeserializeObject<UserDto>(responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception when current user");
            return null;
        }
    }

    public async IAsyncEnumerable<WebhookDto> ListWebhookSubscriptions(WebhookScope scope, CalendlyUserIdentifier userIdentifier, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var url = scope switch
        {
            WebhookScope.Organisation => string.Format(Constants.Calendly.CalendlyWebhookListOrganisationUrlFormat, userIdentifier.Organisation.Uri),
            WebhookScope.User         => string.Format(Constants.Calendly.CalendlyWebhookListUserUrlFormat, userIdentifier.User.Uri, userIdentifier.Organisation.Uri),
            _                         => throw new ArgumentOutOfRangeException(nameof(scope), "Invalid webhook scope"),
        };

        while (!cancellationToken.IsCancellationRequested)
        {
            WebhookListDto? webhookListDto;
            try
            {
                var response = await _httpClient.GetAsync(url, cancellationToken);
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Invalid status code ({StatusCode}) when listing {Scope} webhooks; Response: {ResponseContent}", response.StatusCode, scope, responseContent);
                    yield break;
                }

                webhookListDto = JsonConvert.DeserializeObject<WebhookListDto>(responseContent);
                if (webhookListDto == null)
                {
                    _logger.LogError("Invalid response when listing {Scope} webhooks: {ResponseContent}", scope, responseContent);
                    yield break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception when listing {Scope} webhooks", scope);
                yield break;
            }

            foreach (var webhookDto in webhookListDto.Collection)
            {
                yield return webhookDto;
            }

            if (string.IsNullOrEmpty(webhookListDto.Pagination.NextPageUrl))
            {
                break;
            }

            url = webhookListDto.Pagination.NextPageUrl;
        }
    }

    public async Task<bool> CreateWebhookSubscription(string callbackUrl, WebhookScope scope, CalendlyUserIdentifier userIdentifier, string signingKey, IReadOnlyCollection<WebhookEvent> webhookEvents)
    {
        try
        {
            var requestDto = scope switch
            {
                WebhookScope.User         => CreateWebhookSubscriptionDto.UserDto(callbackUrl, userIdentifier, signingKey, webhookEvents),
                WebhookScope.Organisation => CreateWebhookSubscriptionDto.OrganisationDto(callbackUrl, userIdentifier, signingKey, webhookEvents),
                _                         => throw new ArgumentOutOfRangeException(nameof(scope), scope, "Invalid Calendly webhook scope"),
            };
            var requestContent = new StringContent(JsonConvert.SerializeObject(requestDto, JsonSerializerSettings), Encoding.UTF8, MediaTypeNames.Application.Json);
            var response = await _httpClient.PostAsync(Constants.Calendly.CalendlyWebhookUrl, requestContent);
            var responseString = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Invalid status code ({StatusCode}) when creating webhook for {Scope}: {Uri}; Response: {ResponseContent}", response.StatusCode, scope.ToString().ToLowerInvariant(), userIdentifier, responseString);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception when creating webhook for {Scope}: {Uri}", scope.ToString().ToLowerInvariant(), userIdentifier);
            return false;
        }
    }

    public async Task<bool> DeleteWebhookSubscription(string webhookUri)
    {
        try
        {
            var response = await _httpClient.DeleteAsync(webhookUri);
            var responseString = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Invalid status code ({StatusCode}) when deleting webhook with uri: {WebhookUri}; Response: {ResponseContent}", response.StatusCode, webhookUri, responseString);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception when when deleting webhook with uri: {WebhookUri}", webhookUri);
            return false;
        }
    }

    internal static void ConfigureCalendlyHttpClient(HttpClient client, string accessToken)
    {
        if (string.IsNullOrEmpty(accessToken))
        {
            throw new ArgumentNullException(nameof(accessToken));
        }

        client.BaseAddress = new Uri(Constants.Http.CalendlyApiUrl);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Constants.Http.BearerAuthenticationScheme, accessToken);
    }

    private static JsonSerializerSettings GetJsonSerializerSettings()
    {
        var settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
        settings.Converters.Add(new StringEnumConverter());
        return settings;
    }
}