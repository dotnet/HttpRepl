// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.DotNet.PlatformAbstractions;

namespace Microsoft.HttpRepl.Telemetry
{
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "We don't want any errors in telemetry to cause failures in the product.")]
    [SuppressMessage("Naming", "CA1724: Type names should not match namespaces", Justification = "Keeping it consistent with source implementations.")]
    public sealed class Telemetry : ITelemetry
    {
        internal static string CurrentSessionId = null;
        private TelemetryClient _client = null;
        private Dictionary<string, string> _commonProperties = null;
        private Dictionary<string, double> _commonMeasurements = null;
        private Task _trackEventTask = null;

        private const string InstrumentationKey = "469489a6-628b-4bb9-80db-ec670f70d874";
        public const string TelemetryOptout = "DOTNET_HTTPREPL_TELEMETRY_OPTOUT";

        public Telemetry(
            string productVersion,
            IFirstTimeUseNoticeSentinel sentinel = null,
            string sessionId = null,
            bool blockThreadInitialization = false)
        {
            FirstTimeUseNoticeSentinel = sentinel ?? new FirstTimeUseNoticeSentinel(productVersion);
            Enabled = !GetEnvironmentVariableAsBool(TelemetryOptout) && PermissionExists(FirstTimeUseNoticeSentinel);

            if (!Enabled)
            {
                return;
            }

            // Store the session ID in a static field so that it can be reused
            CurrentSessionId = sessionId ?? Guid.NewGuid().ToString();

            if (blockThreadInitialization)
            {
                InitializeTelemetry(productVersion);
            }
            else
            {
                //initialize in task to offload to parallel thread
                _trackEventTask = Task.Factory.StartNew(() => InitializeTelemetry(productVersion), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
            }
        }

        public bool Enabled { get; }

        public IFirstTimeUseNoticeSentinel FirstTimeUseNoticeSentinel { get; }

        public static bool SkipFirstTimeExperience => GetEnvironmentVariableAsBool(HttpRepl.Telemetry.FirstTimeUseNoticeSentinel.SkipFirstTimeExperienceEnvironmentVariableName, false);

        public static bool IsRunningInDockerContainer => GetEnvironmentVariableAsBool("DOTNET_RUNNING_IN_CONTAINER", false);

        private static bool GetEnvironmentVariableAsBool(string name, bool defaultValue = false)
        {
            var str = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrEmpty(str))
            {
                return defaultValue;
            }

            switch (str.ToUpperInvariant())
            {
                case "TRUE":
                case "1":
                case "YES":
                    return true;
                case "FALSE":
                case "0":
                case "NO":
                    return false;
                default:
                    return defaultValue;
            }
        }

        private bool PermissionExists(IFirstTimeUseNoticeSentinel sentinel)
        {
            if (sentinel == null)
            {
                return false;
            }

            return sentinel.Exists();
        }

        public void TrackEvent(string eventName, IReadOnlyDictionary<string, string> properties,
            IReadOnlyDictionary<string, double> measurements)
        {
            if (!Enabled)
            {
                return;
            }

            //continue task in existing parallel thread
            _trackEventTask = _trackEventTask.ContinueWith(
                x => TrackEventTask(eventName, properties, measurements),
                TaskScheduler.Default
            );
        }

        private void ThreadBlockingTrackEvent(string eventName, IReadOnlyDictionary<string, string> properties, IReadOnlyDictionary<string, double> measurements)
        {
            if (!Enabled)
            {
                return;
            }
            TrackEventTask(eventName, properties, measurements);
        }

        private void InitializeTelemetry(string productVersion)
        {
            try
            {
#pragma warning disable CS0618 // Type or member is obsolete
                _client = new TelemetryClient();
#pragma warning restore CS0618 // Type or member is obsolete
                _client.InstrumentationKey = InstrumentationKey;
                _client.Context.Session.Id = CurrentSessionId;
                _client.Context.Device.OperatingSystem = RuntimeEnvironment.OperatingSystem;

                _commonProperties = new TelemetryCommonProperties(productVersion).GetTelemetryCommonProperties();
                _commonMeasurements = new Dictionary<string, double>();
            }
            catch (Exception e)
            {
                _client = null;
                // we dont want to fail the tool if telemetry fails.
                Debug.Fail(e.ToString());
            }
        }

        private void TrackEventTask(
            string eventName,
            IReadOnlyDictionary<string, string> properties,
            IReadOnlyDictionary<string, double> measurements)
        {
            if (_client == null)
            {
                return;
            }

            try
            {
                Dictionary<string, string> eventProperties = GetEventProperties(properties);
                Dictionary<string, double> eventMeasurements = GetEventMeasures(measurements);

                _client.TrackEvent(PrependProducerNamespace(eventName), eventProperties, eventMeasurements);
                _client.Flush();
            }
            catch (Exception e)
            {
                Debug.Fail(e.ToString());
            }
        }

        private static string PrependProducerNamespace(string eventName)
        {
            return "dotnet/httprepl/" + eventName;
        }

        private Dictionary<string, double> GetEventMeasures(IReadOnlyDictionary<string, double> measurements)
        {
            Dictionary<string, double> eventMeasurements = new Dictionary<string, double>(_commonMeasurements);
            if (measurements != null)
            {
                foreach (KeyValuePair<string, double> measurement in measurements)
                {
                    eventMeasurements[measurement.Key] = measurement.Value;
                }
            }
            return eventMeasurements;
        }

        private Dictionary<string, string> GetEventProperties(IReadOnlyDictionary<string, string> properties)
        {
            if (properties != null)
            {
                var eventProperties = new Dictionary<string, string>(_commonProperties);
                foreach (KeyValuePair<string, string> property in properties)
                {
                    eventProperties[property.Key] = property.Value;
                }
                return eventProperties;
            }
            else
            {
                return _commonProperties;
            }
        }
    }
}
