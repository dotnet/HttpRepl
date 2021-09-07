// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using Microsoft.HttpRepl.Telemetry.Events;

namespace Microsoft.HttpRepl.Telemetry
{
    internal static class TelemetryExtensions
    {
        public static void TrackEvent(this ITelemetry telemetry, TelemetryEventBase telemetryEvent)
        {
            telemetry.TrackEvent(telemetryEvent.Name, telemetryEvent.Properties, telemetryEvent.Measurements);
        }

        public static void TrackStartedEvent(this ITelemetry telemetry, bool withHelp = false, bool withRun = false, bool withOtherArgs = false, bool withOutputRedirection = false)
        {
            telemetry.TrackEvent(new StartedEvent(withHelp, withRun, withOtherArgs, withOutputRedirection));
        }
    }
}
