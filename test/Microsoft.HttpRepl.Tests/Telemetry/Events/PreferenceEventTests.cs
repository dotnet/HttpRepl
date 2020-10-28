// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
