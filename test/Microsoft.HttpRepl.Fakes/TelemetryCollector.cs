// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System.Collections.Generic;
using Microsoft.HttpRepl.Telemetry;

namespace Microsoft.HttpRepl.Fakes
{
    public class TelemetryCollector : ITelemetry
    {
        private List<CollectedTelemetry> _telemetry = new List<CollectedTelemetry>();

        public bool Enabled => true;

        public IFirstTimeUseNoticeSentinel FirstTimeUseNoticeSentinel => null;

        public void TrackEvent(string eventName, IReadOnlyDictionary<string, string> properties, IReadOnlyDictionary<string, double> measurements)
        {
            _telemetry.Add(new CollectedTelemetry(eventName, properties, measurements));
        }

        public IReadOnlyList<CollectedTelemetry> Telemetry => _telemetry;


        public class CollectedTelemetry
        {
            public string EventName { get; }
            public IReadOnlyDictionary<string, string> Properties { get; }
            public IReadOnlyDictionary<string, double> Measurements { get; }

            public CollectedTelemetry(string eventName, IReadOnlyDictionary<string, string> properties, IReadOnlyDictionary<string, double> measurements)
            {
                EventName = eventName;
                Properties = properties;
                Measurements = measurements;
            }
        }

    }
}
