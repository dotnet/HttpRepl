// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.HttpRepl.Tests.Preferences
{
    internal static class TestDefaultPreferences
    {
        internal static Dictionary<string, string> GetDefaultPreferences()
        {
            // For now, we'll just use the same defaults as used by the app.
            return Program.CreateDefaultPreferences();
        }
    }
}
