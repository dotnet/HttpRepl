// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.HttpRepl.Telemetry
{
    public interface ITelemetry
    {
        bool Enabled { get; }
        void TrackEvent(string eventName, IReadOnlyDictionary<string, string> properties, IReadOnlyDictionary<string, double> measurements);
        IFirstTimeUseNoticeSentinel FirstTimeUseNoticeSentinel { get; }
    }
}
