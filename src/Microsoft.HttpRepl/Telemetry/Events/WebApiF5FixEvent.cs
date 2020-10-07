namespace Microsoft.HttpRepl.Telemetry.Events
{
    internal class WebApiF5FixEvent : TelemetryEventBase
    {
        public WebApiF5FixEvent(bool skippedByPreference = false) : base(TelemetryEventNames.WebApiF5Fix)
        {
            SetProperty(TelemetryPropertyNames.WebApiF5Fix_SkippedByPreference, skippedByPreference);
        }
    }
}
