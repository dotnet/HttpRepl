namespace Microsoft.HttpRepl.Telemetry.Events
{
    internal class WebApiF5FixEvent : TelemetryEventBase
    {
        public WebApiF5FixEvent(bool skippedByPreference = false) : base("WebApiF5Fix")
        {
            SetProperty("SkippedByPreference", skippedByPreference);
        }
    }
}
