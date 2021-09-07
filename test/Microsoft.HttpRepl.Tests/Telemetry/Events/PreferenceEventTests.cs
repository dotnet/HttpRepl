// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using Microsoft.HttpRepl.Telemetry.Events;
using Xunit;

namespace Microsoft.HttpRepl.Tests.Telemetry.Events
{
    public class PreferenceEventTests
    {
        [Theory]
        [InlineData("get", null)]
        [InlineData("get", "")]
        [InlineData("get", " ")]
        [InlineData("get", "editor.command.default")]
        [InlineData("get", "not.really.a.preference")]
        [InlineData("set", null)]
        [InlineData("set", "")]
        [InlineData("set", " ")]
        [InlineData("set", "editor.command.default")]
        [InlineData("set", "not.really.a.preference")]
        public void Constructor_DoesNotThrow(string getOrSet, string preferenceName)
        {
            PreferenceEvent preferenceEvent = new PreferenceEvent(getOrSet, preferenceName);
        }
    }
}
