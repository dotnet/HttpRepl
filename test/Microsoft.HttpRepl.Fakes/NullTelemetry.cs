// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.HttpRepl.Telemetry;

namespace Microsoft.HttpRepl.Fakes
{
    public class NullTelemetry : ITelemetry
    {
        private readonly IFirstTimeUseNoticeSentinel _firstTimeUseNoticeSentinel = new NullFirstTimeUseNoticeSentinel();

        public bool Enabled => false;

        public IFirstTimeUseNoticeSentinel FirstTimeUseNoticeSentinel => _firstTimeUseNoticeSentinel;

        public void TrackEvent(string eventName, IReadOnlyDictionary<string, string> properties, IReadOnlyDictionary<string, double> measurements)
        {

        }
    }

    public class NullFirstTimeUseNoticeSentinel : IFirstTimeUseNoticeSentinel
    {
        public void CreateIfNotExists() { }

        public bool Exists() => true;
    }
}
