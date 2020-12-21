namespace Microsoft.HttpRepl.Telemetry.Events
{
    internal class SetQueryStringEvent : TelemetryEventBase
    {
        public SetQueryStringEvent(string key, bool isValueEmpty) : base(TelemetryEventNames.SetQueryString)
        {
            SetProperty(TelemetryPropertyNames.SetQueryString_Key, SanitizeKey(key));
            SetProperty(TelemetryPropertyNames.SetQueryString_IsValueEmpty, isValueEmpty);
        }

        private static string SanitizeKey(string headerName)
        {
            if (string.IsNullOrEmpty(headerName))
            {
                return headerName;
            }

            return Sha256Hasher.Hash(headerName);
        }
    }
}
