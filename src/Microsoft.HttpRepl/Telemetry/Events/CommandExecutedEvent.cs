// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

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
