// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.Fakes;
using Microsoft.Repl.Parsing;
using Xunit;

namespace Microsoft.HttpRepl.Tests.Commands
{
    public class ExitCommandTests : CommandTestsBase
    {
        [Fact]
        public async Task ExecuteAsync_Always_IsExitingIsTrue()
        {
            ArrangeInputs("exit", out MockedShellState shellState, out HttpState httpState, out ICoreParseResult parseResult);

            ExitCommand exitCommand = new ExitCommand();

            await exitCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.True(shellState.IsExiting);
        }

        [Fact]
        public async Task ExecuteAsync_Always_DoesNotWritePrompt()
        {
            ArrangeInputs("exit", out MockedShellState shellState, out HttpState httpState, out ICoreParseResult parseResult);

            ExitCommand exitCommand = new ExitCommand();

            await exitCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Empty(shellState.Output);
        }
    }
}
