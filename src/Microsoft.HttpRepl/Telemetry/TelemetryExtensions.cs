// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
