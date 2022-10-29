using CalendlyEventWebhook.Models;

namespace CalendlyEventWebhook.Configuration;

internal class CalendlyConfiguration
{
    public static string SettingsKey => Constants.Configuration.SettingsKey;

    public string AccessToken { get; set; }

    public CalendlyScope Scope { get; set; }

    public CalendlyWebhookConfiguration Webhook { get; set; }
}

internal class CalendlyWebhookConfiguration
{
    public bool CleanupAllExistingWebhooks { get; set; }

    public bool SkipWebhookCreation { get; set; }

    public bool EventCreation { get; set; }

    public string CallbackUrl { get; set; }

    public string SigningKey { get; set; }
}