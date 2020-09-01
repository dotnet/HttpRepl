// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.HttpRepl.Telemetry.Events
{
    internal class PreferenceEvent : TelemetryEventBase
    {
        public PreferenceEvent(string getOrSet, string preferenceName) : base("Preference")
        {
            SetProperty("GetOrSet", getOrSet);
            SetProperty("PreferenceName", preferenceName);
        }
    }
}
