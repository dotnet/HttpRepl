// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System;
using System.Linq;

namespace Microsoft.HttpRepl.Telemetry.Events
{
    internal class SetHeaderEvent : TelemetryEventBase
    {
        public SetHeaderEvent(string headerName, bool isValueEmpty) : base(TelemetryEventNames.SetHeader)
        {
            SetProperty(TelemetryPropertyNames.SetHeader_HeaderName, SanitizeHeaderName(headerName));
            SetProperty(TelemetryPropertyNames.SetHeader_IsValueEmpty, isValueEmpty);
        }

        private static string SanitizeHeaderName(string headerName)
        {
            if (string.IsNullOrEmpty(headerName) ||
                WellKnownHeaders.CommonHeaders.Contains(headerName, StringComparer.OrdinalIgnoreCase))
            {
                return headerName;
            }

            return Sha256Hasher.Hash(headerName);
        }
    }
}
