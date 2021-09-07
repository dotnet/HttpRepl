// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

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
