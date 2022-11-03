using System.Diagnostics.CodeAnalysis;
using CalendlyEventWebhook.Models;

namespace CalendlyEventWebhook.Configuration;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Instance created from JSON")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Instance created from JSON")]
internal class UserCalendlyConfiguration
{
    public static string SettingsKey => Constants.Configuration.SettingsKey;

    public string? AccessToken { get; set; }

    public CalendlyScope? Scope { get; set; }

    public UserCalendlyWebhookConfiguration? Webhook { get; set; }

    internal class UserCalendlyWebhookConfiguration
    {
        public bool? CleanupAllExistingWebhooks { get; set; }

        public bool? SkipWebhookCreation { get; set; }

        public bool? EventCreation { get; set; }

        public string? CallbackUrl { get; set; }

        public string? SigningKey { get; set; }
    }
}

internal class CalendlyConfiguration
{
    public string AccessToken { get; }

    public CalendlyScope Scope { get; }

    public CalendlyWebhookConfiguration Webhook { get; }

    public CalendlyConfiguration(UserCalendlyConfiguration? userCalendlyConfiguration)
    {
        AccessToken = !string.IsNullOrEmpty(userCalendlyConfiguration?.AccessToken) ? userCalendlyConfiguration.AccessToken : string.Empty;
        Scope = userCalendlyConfiguration?.Scope ?? CalendlyScope.User;
        Webhook = new CalendlyWebhookConfiguration(userCalendlyConfiguration?.Webhook);
    }

    internal class CalendlyWebhookConfiguration
    {
        public bool CleanupAllExistingWebhooks { get; }

        public bool SkipWebhookCreation { get; }

        public bool EventCreation { get; }

        public string CallbackUrl { get; }

        public string SigningKey { get; }

        public CalendlyWebhookConfiguration(UserCalendlyConfiguration.UserCalendlyWebhookConfiguration? userConfiguration)
        {
            CleanupAllExistingWebhooks = userConfiguration?.CleanupAllExistingWebhooks ?? false;
            SkipWebhookCreation = userConfiguration?.SkipWebhookCreation ?? false;
            EventCreation = userConfiguration?.EventCreation ?? false;
            CallbackUrl = !string.IsNullOrEmpty(userConfiguration?.CallbackUrl) ? userConfiguration.CallbackUrl : string.Empty;
            SigningKey = !string.IsNullOrEmpty(userConfiguration?.SigningKey) ? userConfiguration.SigningKey : string.Empty;
        }
    }
}