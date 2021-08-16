// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

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
