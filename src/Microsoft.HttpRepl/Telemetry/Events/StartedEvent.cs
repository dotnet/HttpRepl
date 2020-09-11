// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
