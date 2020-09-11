// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.HttpRepl.Telemetry.Events
{
    internal class CommandExecutedEvent : TelemetryEventBase
    {
        public CommandExecutedEvent(string commandName, bool wasSuccessful) : base(TelemetryEventNames.CommandExecuted)
        {
            SetProperty(TelemetryPropertyNames.CommandExecuted_CommandName, commandName);
            SetProperty(TelemetryPropertyNames.CommandExecuted_WasSuccessful, wasSuccessful);
        }
    }
}
