// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using Microsoft.Repl.Commanding;
using Microsoft.Repl.Parsing;

namespace Microsoft.HttpRepl.Telemetry
{
    internal static class DefaultCommandDispatcherExtensions
    {
        public static void AddCommandWithTelemetry(this DefaultCommandDispatcher<HttpState, ICoreParseResult> defaultCommandDispatcher,
                                                   ITelemetry telemetry,
                                                   ICommand<HttpState, ICoreParseResult> command)
        {
            defaultCommandDispatcher.AddCommand(new TelemetryCommandWrapper(telemetry, command));
        }
    }
}
