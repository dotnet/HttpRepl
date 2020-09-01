// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
