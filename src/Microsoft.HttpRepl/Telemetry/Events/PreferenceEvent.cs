// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.HttpRepl.Preferences;

namespace Microsoft.HttpRepl.Telemetry.Events
{
    internal class PreferenceEvent : TelemetryEventBase
    {
        public PreferenceEvent(string getOrSet, string preferenceName) : base(TelemetryEventNames.Preference)
        {
            SetProperty(TelemetryPropertyNames.Preference_GetOrSet, getOrSet);
            SetProperty(TelemetryPropertyNames.Preference_PreferenceName, SanitizePreferenceName(preferenceName));
        }

        private static string SanitizePreferenceName(string preferenceName)
        {
            if (string.IsNullOrEmpty(preferenceName) ||
                WellKnownPreference.Catalog.Names.Contains(preferenceName, StringComparer.OrdinalIgnoreCase))
            {
                return preferenceName;
            }

            return Sha256Hasher.Hash(preferenceName);
        }
    }
}
