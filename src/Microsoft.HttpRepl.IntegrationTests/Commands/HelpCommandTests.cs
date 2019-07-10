// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.FileSystem;
using Microsoft.HttpRepl.IntegrationTests.Mocks;
using Microsoft.HttpRepl.Preferences;
using Microsoft.HttpRepl.UserProfile;
using Microsoft.Repl;
using Microsoft.Repl.Commanding;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Parsing;
using Xunit;

namespace Microsoft.HttpRepl.IntegrationTests.Commands
{
    public class HelpCommandTests
    {
        [Theory]
        [InlineData("help", true)]
        [InlineData("HELP", true)]
        [InlineData("hElP", true)]
        [InlineData("hep", null)]
        public void CanHandle(string commandText, bool? expected)
        {
            Arrange(commandText, addCommands: false, out HelpCommand helpCommand, out IShellState shellState, out HttpState httpState, out ICoreParseResult parseResult);

            bool? result = helpCommand.CanHandle(shellState, httpState, parseResult);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("help c", "clear", "cls", "cd")]
        [InlineData("help r", "run")]
        [InlineData("help z")]
        public void Suggest(string commandText, params string[] expectedResults)
        {
            Arrange(commandText, addCommands: true, out HelpCommand helpCommand, out IShellState shellState, out HttpState httpState, out ICoreParseResult parseResult);

            IEnumerable<string> result = helpCommand.Suggest(shellState, httpState, parseResult);

            Assert.NotNull(result);

            List<string> resultList = result.ToList();

            Assert.Equal(expectedResults.Length, resultList.Count);

            for (int index = 0; index < expectedResults.Length; index++)
            {
                Assert.Contains(expectedResults[index], resultList, StringComparer.OrdinalIgnoreCase);
            }
        }

        private void Arrange(string commandText, bool addCommands, out HelpCommand helpCommand, out IShellState shellState, out HttpState httpState, out ICoreParseResult parseResult)
        {
            IFileSystem fileSystem = new MockedFileSystem();
            IUserProfileDirectoryProvider userProfileDirectoryProvider = new UserProfileDirectoryProvider();
            IPreferences preferences = new UserFolderPreferences(fileSystem, userProfileDirectoryProvider, null);
            helpCommand = new HelpCommand(preferences);
            httpState = new HttpState(fileSystem, preferences, new HttpClient());
            if (addCommands)
            {
                DefaultCommandDispatcher<HttpState> dispatcher = DefaultCommandDispatcher.Create((ss) => { }, httpState);
                dispatcher.AddCommand(new ClearCommand());
                dispatcher.AddCommand(new ChangeDirectoryCommand());
                dispatcher.AddCommand(new RunCommand(fileSystem));
                IConsoleManager consoleManager = new LoggingConsoleManagerDecorator(new ConsoleManagerStub());
                shellState = new ShellState(dispatcher, consoleManager: consoleManager);
            }
            else
            {
                shellState = new MockedShellState();
            }
            parseResult = CoreParseResultHelper.Create(commandText);
        }
    }
}
