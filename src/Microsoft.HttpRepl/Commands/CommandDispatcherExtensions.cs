// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Repl.Commanding;
using Microsoft.Repl.Parsing;

namespace Microsoft.HttpRepl.Commands
{
    internal static class CommandDispatcherExtensions
    {
        public static T GetCommand<T>(this ICommandDispatcher<HttpState, ICoreParseResult> dispatcher) where T : ICommand<HttpState, ICoreParseResult>
        {
            return dispatcher.Commands.OfType<T>().FirstOrDefault();
        }
    }
}
