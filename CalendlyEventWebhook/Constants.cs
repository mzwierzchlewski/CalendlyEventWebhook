using CalendlyEventWebhook.Webhook;

namespace CalendlyEventWebhook;

internal static class Constants
{
    public static class Configuration
    {
        public static readonly string SettingsKey = "CalendlyEventWebhook";
    }

    public static class Http
    {
        public static readonly string CalendlyApiUrl = "https://api.calendly.com";

        public static readonly string BearerAuthenticationScheme = "Bearer";
    }

    public static class Calendly
    {
        public static readonly string CalendlyEventUrlFormat = "/scheduled_events/{0}";

        public static readonly string CalendlyCurrentUserUrl = "/users/me";

        public static readonly string CalendlyWebhookListUrlFormat = "/webhook_subscriptions?scope={0}&{0}={1}";

        public static readonly string CalendlyWebhookUrl = "/webhook_subscriptions";
    }

    public static class WebhookSignature
    {
        public static readonly string DataFormat = $"{{{nameof(SignatureCalculator.SignatureDataParameters.Timestamp)}}}.{{{nameof(SignatureCalculator.SignatureDataParameters.Body)}}}";

        public static class Header
        {
            public static readonly string Key = "Calendly-Webhook-Signature";

            public static class Regex
            {
                public static readonly string TimestampGroupKey = "timestamp";

                public static readonly string SignatureGroupKey = "signature";

                public static readonly string Pattern = $"^t=(?<{TimestampGroupKey}>[0-9]+),v1=(?<{SignatureGroupKey}>[a-z0-9]+)$";
            }
        }
    }
}