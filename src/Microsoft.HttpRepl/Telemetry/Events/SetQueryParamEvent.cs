// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

namespace Microsoft.HttpRepl.Telemetry.Events
{
    internal class SetQueryParamEvent : TelemetryEventBase
    {
        public SetQueryParamEvent(string key, bool isValueEmpty) : base(TelemetryEventNames.SetQueryParam)
        {
            SetProperty(TelemetryPropertyNames.SetQueryParam_Key, SanitizeKey(key));
            SetProperty(TelemetryPropertyNames.SetQueryParam_IsValueEmpty, isValueEmpty);
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
