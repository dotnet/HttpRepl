// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.HttpRepl.Telemetry.Events;
using Xunit;

namespace Microsoft.HttpRepl.Tests.Telemetry.Events
{
    public class SetHeaderEventTests
    {
        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData(" ", false)]
        [InlineData("Content-Type", false)]
        [InlineData("X-Custom-Header", false)]
        [InlineData(null, true)]
        [InlineData("", true)]
        [InlineData(" ", true)]
        [InlineData("Content-Type", true)]
        [InlineData("X-Custom-Header", true)]
        public void Constructor_DoesNotThrow(string headerName, bool isValueEmpty)
        {
            SetHeaderEvent preferenceEvent = new SetHeaderEvent(headerName, isValueEmpty);
        }
    }
}
