// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.Fakes;
using Microsoft.HttpRepl.Preferences;
using Microsoft.Repl;
using Microsoft.Repl.Commanding;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Parsing;
using Xunit;

namespace Microsoft.HttpRepl.Tests.Commands
{
    public class HelpCommandTests : CommandTestsBase
    {
        [Theory]
        [InlineData("help", true)]
        [InlineData("HELP", true)]
        [InlineData("hElP", true)]
        [InlineData("hep", null)]
        public void CanHandle(string commandText, bool? expected)
        {
            HttpState httpState = GetHttpState(out _, out _);
            ICoreParseResult parseResult = CreateCoreParseResult(commandText);
            IShellState shellState = new MockedShellState();

            HelpCommand helpCommand = new HelpCommand();

            bool? result = helpCommand.CanHandle(shellState, httpState, parseResult);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("help c", "clear", "cls", "cd")]
        [InlineData("help r", "run")]
        [InlineData("help z")]
        public void Suggest(string commandText, params string[] expectedResults)
        {
            HttpState httpState = GetHttpState(out MockedFileSystem fileSystem, out _);
            ICoreParseResult parseResult = CreateCoreParseResult(commandText);
            IConsoleManager consoleManager = new LoggingConsoleManagerDecorator(new NullConsoleManager());
            DefaultCommandDispatcher<HttpState> commandDispatcher = DefaultCommandDispatcher.Create((ss) => { }, httpState);
            commandDispatcher.AddCommand(new ClearCommand());
            commandDispatcher.AddCommand(new ChangeDirectoryCommand());
            commandDispatcher.AddCommand(new RunCommand(fileSystem));
            IShellState shellState = new ShellState(commandDispatcher, consoleManager: consoleManager);

            HelpCommand helpCommand = new HelpCommand();

            IEnumerable<string> result = helpCommand.Suggest(shellState, httpState, parseResult);

            Assert.NotNull(result);

            List<string> resultList = result.ToList();

            Assert.Equal(expectedResults.Length, resultList.Count);

            for (int index = 0; index < expectedResults.Length; index++)
            {
                Assert.Contains(expectedResults[index], resultList, StringComparer.OrdinalIgnoreCase);
            }
        }
    }
}
