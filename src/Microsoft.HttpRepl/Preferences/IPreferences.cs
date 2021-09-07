// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System.Collections.Generic;
using Microsoft.Repl.ConsoleHandling;

namespace Microsoft.HttpRepl.Preferences
{
    public interface IPreferences
    {
        AllowedColors GetColorValue(string preference, AllowedColors defaultValue = AllowedColors.None);
        int GetIntValue(string preference, int defaultValue = default);
        bool GetBoolValue(string preference, bool defaultValue = default);
        string GetValue(string preference, string defaultValue = default);
        bool TryGetValue(string preference, out string value);
        bool SetValue(string preference, string value);
        IReadOnlyDictionary<string, string> DefaultPreferences { get; }
        IReadOnlyDictionary<string, string> CurrentPreferences { get; }
    }
}
