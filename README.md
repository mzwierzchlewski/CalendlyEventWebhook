# CalendlyEventWebhook

A simple project for registering and handling [Calendly event webhooks](https://help.calendly.com/hc/en-us/articles/223195488-Getting-started-with-webhooks).

## Installation

1. Add `CalendlyEventWebhook` reference to your project.
2. Use the `IServiceCollection.AddCalendlyEventWebhook` method in the `ConfigureServices` method of your `Startup` class.  
_This will add services required for creating/removing webhook subscriptions and handling Calendly requests._
3. Use the `IEndpointRouteBuilder.MapCalendlyWebhook` method in the `Configure` method of your `Startup` class.  
_This will map the route specified in `Webhook.CallbackUrl` configuration property to a middleware that handles requests from Calendly._
4. Add [configuration](#configuration) to your configuration file (e.g. `appsettings.json`).
5. Implement `IEventCancellationHandler`, `IEventReschedulingHandler`, and `IEventCreationHandler` interfaces 
whose methods will be called when a matching event is sent from Calendly.

Example:
```csharp
Host.CreateDefaultBuilder(args)
    .ConfigureWebHost(
        webHost =>
        {
            webHost.Configure(
                app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints => endpoints.MapCalendlyWebhook());
                });
        })
    .ConfigureServices(
        (context, services) =>
        {
            services.AddRouting();
            services.AddCalendlyEventWebhook(context.Configuration);
        });
```
```csharp
internal class EventCancellationHandler : IEventCancellationHandler
{
    private readonly ILogger<EventCancellationHandler> _logger;

    public EventCancellationHandler(ILogger<EventCancellationHandler> logger)
    {
        _logger = logger;
    }

    public Task<bool> Handle(CalendlyResourceIdentifier id)
    {
        _logger.LogInformation("Event {EventId} cancelled", id.Id);
        return Task.FromResult(true);
    }
}
```

## Configuration

The configuration object must be placed in `CalendlyEventWebhook` section. The following settings are available:
* `AccessToken` _(required)_ - the [Personal Access Token](https://developer.calendly.com/how-to-authenticate-with-personal-access-tokens) used to authenticate with Calendly
* `Scope` - `User` _(default)_ or `Organisation` - dictates whether the webhook subscriptions should be scoped to a user or the whole organisation
* `Webhook` object:
  * `CallbackUrl` _(required)_ - the URL which will be used by Calendly to publish event updates
  * `SigningKey` - [secret key shared between your application and Calendly used to verify origin of incoming requests](https://developer.calendly.com/api-docs/ZG9jOjM2MzE2MDM4-webhook-signatures)
  * `CleanupAllExistingWebhooks` - `false` _(default)_ or `true` - if `true`, all existing webhook subscriptions will be removed from Calendly
  * `SkipWebhookCreation` - `false` _(default)_ or `true` - if `true`, no webhook subscription will be created for given `CallbackUrl`
  * `EventCreation` - `false` _(default)_ or `true` - if `true`, the webhook subscription will include event creation events

## Remarks
Webhook subscription creation/removal will happen at the startup of your application thanks to an `IHostedService` worker. 
If both `CleanupAllExistingWebhooks` and `SkipWebhookCreation` are set to `true` the worker will not be registered with 
the DI container.

The dependency on `Newtonsoft.Json` allows (de)serializing custom enum member values.