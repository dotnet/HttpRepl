// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
