namespace CalendlyEventWebhook;

internal static class Constants
{
    public static class Configuration
    {
        public static string SettingsKey = "CalendlyEventWebhook";
    }

    public static class Http
    {
        public static string CalendlyApiUrl = "https://api.calendly.com";

        public static string BearerAuthenticationScheme = "Bearer";
    }

    public static class Calendly
    {
        public static string CalendlyEventUrlFormat = "/scheduled_events/{0}";

        public static string CalendlyCurrentUserUrl = "/users/me";

        public static string CalendlyWebhookListUrlFormat = "/webhook_subscriptions?scope={0}&{0}={1}";

        public static string CalendlyWebhookUrl = "/webhook_subscriptions";
    }
}