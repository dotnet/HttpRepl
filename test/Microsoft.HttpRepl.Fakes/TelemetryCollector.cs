// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
