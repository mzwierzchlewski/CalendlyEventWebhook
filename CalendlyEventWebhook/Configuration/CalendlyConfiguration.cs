using CalendlyEventWebhook.Models;

namespace CalendlyEventWebhook.Configuration;

internal class CalendlyConfiguration
{
    public static string SettingsKey => Constants.Configuration.SettingsKey;

    public string AccessToken { get; set; }

    public CalendlyWebhookConfiguration Webhook { get; set; }
}

internal class CalendlyWebhookConfiguration
{
    public CalendlyWebhookScope Scope { get; set; }

    public string CallbackUrl { get; set; }

    public string SigningKey { get; set; }
}