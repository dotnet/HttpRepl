// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

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
