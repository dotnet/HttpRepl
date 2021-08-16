// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using Microsoft.HttpRepl.Preferences;
using Microsoft.Repl.ConsoleHandling;

namespace Microsoft.HttpRepl.Fakes
{
    public sealed class FakePreferences : IPreferences
    {
        private readonly Dictionary<string, string> _currentPreferences;

        public FakePreferences()
        {
            DefaultPreferences = new Dictionary<string, string>();
            _currentPreferences = new();
        }

        public IReadOnlyDictionary<string, string> DefaultPreferences { get; }
        public IReadOnlyDictionary<string, string> CurrentPreferences => _currentPreferences;

        public bool GetBoolValue(string preference, bool defaultValue = false)
        {
            if (CurrentPreferences.TryGetValue(preference, out string value) && bool.TryParse(value, out bool result))
            {
                return result;
            }

            return defaultValue;
        }

        public AllowedColors GetColorValue(string preference, AllowedColors defaultValue = AllowedColors.None)
        {
            if (CurrentPreferences.TryGetValue(preference, out string value) && Enum.TryParse(value, true, out AllowedColors result))
            {
                return result;
            }

            return defaultValue;
        }

        public int GetIntValue(string preference, int defaultValue = 0)
        {
            if (CurrentPreferences.TryGetValue(preference, out string value) && int.TryParse(value, out int result))
            {
                return result;
            }

            return defaultValue;
        }

        public string GetValue(string preference, string defaultValue = null)
        {
            if (CurrentPreferences.TryGetValue(preference, out string value))
            {
                return value;
            }

            return defaultValue;
        }

        public bool SetValue(string preference, string value)
        {
            _currentPreferences[preference] = value;
            return true;
        }

        public bool TryGetValue(string preference, out string value) => CurrentPreferences.TryGetValue(preference, out value);
    }
}
