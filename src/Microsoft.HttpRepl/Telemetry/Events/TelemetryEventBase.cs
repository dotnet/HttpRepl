// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.HttpRepl.Telemetry.Events
{
    internal abstract class TelemetryEventBase
    {
        private readonly string _name;
        private readonly Dictionary<string, string> _properties = new Dictionary<string, string>();
        private readonly Dictionary<string, double> _measurements = new Dictionary<string, double>();

        public string Name => _name;
        public IReadOnlyDictionary<string, string> Properties => _properties;
        public IReadOnlyDictionary<string, double> Measurements => _measurements;

        public TelemetryEventBase(string name)
        {
            _name = name;
        }

        protected void SetProperty(string name, bool value) => SetProperty(name, value.ToString());
        protected void SetProperty(string name, string value) => _properties[name] = value;
        protected string GetProperty(string name, string defaultValue = default)
        {
            if (_properties.TryGetValue(name, out string value))
            {
                return value;
            }

            return defaultValue;
        }

        protected void SetMeasurement(string name, double value) => _measurements[name] = value;
        protected double GetMeasurement(string name, double defaultValue = default)
        {
            if (_measurements.TryGetValue(name, out double value))
            {
                return value;
            }

            return defaultValue;
        }
    }
}
