// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
