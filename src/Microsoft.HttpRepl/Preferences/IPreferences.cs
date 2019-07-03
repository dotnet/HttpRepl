// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.HttpRepl.Preferences
{
    public interface IPreferences
    {
        Dictionary<string, string> ReadPreferences(IReadOnlyDictionary<string, string> defaultPreferences);
        bool WritePreferences(Dictionary<string, string> preferences, IReadOnlyDictionary<string, string> defaultPreferences);
    }
}
