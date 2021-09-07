// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

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
