// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

namespace Microsoft.HttpRepl.Telemetry.Events
{
    internal class StartedEvent : TelemetryEventBase
    {
        public StartedEvent(bool withHelp, bool withRun, bool withOtherArgs, bool withOutputRedirection) : base(TelemetryEventNames.Started)
        {
            SetProperty(TelemetryPropertyNames.Started_WithHelp, withHelp);
            SetProperty(TelemetryPropertyNames.Started_WithRun, withRun);
            SetProperty(TelemetryPropertyNames.Started_WithOtherArgs, withOtherArgs);
            SetProperty(TelemetryPropertyNames.Started_WithOutputRedirection, withOutputRedirection);
        }
    }
}
