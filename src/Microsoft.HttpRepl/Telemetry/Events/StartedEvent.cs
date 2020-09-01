// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.HttpRepl.Telemetry.Events
{
    internal class StartedEvent : TelemetryEventBase
    {
        public StartedEvent(bool withHelp, bool withRun, bool withOtherArgs, bool withOutputRedirection) : base("Started")
        {
            SetProperty("WithHelp", withHelp);
            SetProperty("WithRun", withRun);
            SetProperty("WithOtherArgs", withOtherArgs);
            SetProperty("WithOutputRedirection", withOutputRedirection);
        }
    }
}
