// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.Fakes;
using Microsoft.HttpRepl.Resources;
using Microsoft.Repl;
using Microsoft.Repl.Commanding;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Parsing;
using Moq;
using Xunit;

namespace Microsoft.HttpRepl.Tests.Commands
{
    public class RunCommandTests : CommandTestsBase, IDisposable
    {
        private string _pathToScript = Path.Combine(Directory.GetCurrentDirectory(), "InputFileForRunCommand.txt");

        [Fact]
        public void CanHandle_WithNoParseResultSections_ReturnsNull()
        {
            ArrangeInputs(parseResultSections: string.Empty,
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            RunCommand runCommand = new RunCommand(new MockedFileSystem());
            bool? result = runCommand.CanHandle(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithFirstParseResultSectionNotEqualToName_ReturnsNull()
        {
            ArrangeInputs(parseResultSections: "test name.txt",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            RunCommand runCommand = new RunCommand(new MockedFileSystem());
            bool? result = runCommand.CanHandle(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithValidInput_ReturnsTrue()
        {
            ArrangeInputs(parseResultSections: "run InputFileForRunCommand.txt",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            RunCommand runCommand = new RunCommand(new MockedFileSystem());
            bool? result = runCommand.CanHandle(shellState, httpState, parseResult);

            Assert.True(result);
        }

        [Fact]
        public void GetHelpSummary_ReturnsDescription()
        {
            ArrangeInputs(parseResultSections: string.Empty,
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult _);

            RunCommand runCommand = new RunCommand(new MockedFileSystem());
            string result = runCommand.GetHelpSummary(shellState, httpState);

            Assert.Equal(Strings.RunCommand_HelpSummary, result);
        }

        [Fact]
        public void GetHelpDetails_WithEmptyParseResultSection_ReturnsNull()
        {
            ArrangeInputs(parseResultSections: string.Empty,
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult parseResult);

            RunCommand runCommand = new RunCommand(new MockedFileSystem());
            string result = runCommand.GetHelpDetails(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public void GetHelpDetails_WithFirstParseResultSectionNotEqualToName_ReturnsNull()
        {
            ArrangeInputs(parseResultSections: "section1 section2 section3",
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult parseResult);

            RunCommand runCommand = new RunCommand(new MockedFileSystem());
            string result = runCommand.GetHelpDetails(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public void GetHelpDetails_WithValidInput_HelpDetails()
        {
            ArrangeInputs(parseResultSections: "run InputFileForRunCommand.txt",
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult parseResult);

            string expected = "\u001b[1mUsage: \u001b[39mrun {path to script}" + Environment.NewLine + Environment.NewLine +
                "Runs the specified script." + Environment.NewLine +
                "A script is a text file containing one CLI command per line. Each line will be run as if it was typed into the CLI." + Environment.NewLine + Environment.NewLine +
                "When +history option is specified, commands specified in the text file will be added to command history." + Environment.NewLine;

            RunCommand runCommand = new RunCommand(new MockedFileSystem());
            string result = runCommand.GetHelpDetails(shellState, httpState, parseResult);

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task ExecuteAsync_IfFileDoesNotExist_WritesToConsoleManagerError()
        {
            ArrangeInputs(parseResultSections: "run InputFileForRunCommand.txt",
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult parseResult);

            RunCommand runCommand = new RunCommand(new MockedFileSystem());
            await runCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            VerifyErrorMessageWasWrittenToConsoleManagerError(shellState);
        }

        [Fact]
        public async Task ExecuteAsync_WithValidInput_ExecutesTheCommandsInTheScript()
        {
            string commands = @"set header name value1 value2";

            if (!File.Exists(_pathToScript))
            {
                File.WriteAllText(_pathToScript, commands);
            }

            string parseResultSections = "run " + _pathToScript;
            ArrangeInputs(parseResultSections: parseResultSections,
                 out MockedShellState _,
                 out HttpState httpState,
                 out ICoreParseResult parseResult);

            IShellState shellState = GetShellState(commands, httpState);
            MockedFileSystem mockedFileSystem = new MockedFileSystem();
            mockedFileSystem.AddFile(_pathToScript, commands);
            RunCommand runCommand = new RunCommand(mockedFileSystem);

            await runCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Dictionary<string, IEnumerable<string>> headers = httpState.Headers;
            KeyValuePair<string, IEnumerable<string>> firstHeader = headers.First();
            KeyValuePair<string, IEnumerable<string>> secondHeader = headers.ElementAt(1);

            Assert.Equal(2, httpState.Headers.Count);
            Assert.Equal("User-Agent", firstHeader.Key);
            Assert.Equal("HTTP-REPL", firstHeader.Value.First());

            Assert.Equal("name", secondHeader.Key);
            Assert.Equal("value1", secondHeader.Value.First());
            Assert.Equal("value2", secondHeader.Value.ElementAt(1));
        }

        [Fact]
        public async Task ExecuteAsync_WithHistoryOption_AddsCommandsExecutedFromScriptToCommandHistory()
        {
            string commands = @"set header name value1 value2";

            if (!File.Exists(_pathToScript))
            {
                File.WriteAllText(_pathToScript, commands);
            }

            string parseResultSections = "run " + _pathToScript + " +history";
            ArrangeInputs(parseResultSections: parseResultSections,
                 out MockedShellState _,
                 out HttpState httpState,
                 out ICoreParseResult parseResult);

            IShellState shellState = GetShellState(commands, httpState);
            MockedFileSystem mockedFileSystem = new MockedFileSystem();
            mockedFileSystem.AddFile(_pathToScript, commands);
            RunCommand runCommand = new RunCommand(mockedFileSystem);

            await runCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            string previousCommand = shellState.CommandHistory.GetPreviousCommand();

            Assert.Equal(commands, previousCommand);
        }

        [Fact]
        public async Task ExecuteAsync_WithoutHistoryOption_AvoidsAddingCommandsExecutedFromScriptToCommandHistory()
        {
            string commands = @"set header name value1 value2";

            if (!File.Exists(_pathToScript))
            {
                File.WriteAllText(_pathToScript, commands);
            }

            string parseResultSections = "run " + _pathToScript;
            ArrangeInputs(parseResultSections: parseResultSections,
                 out MockedShellState _,
                 out HttpState httpState,
                 out ICoreParseResult parseResult);

            IShellState shellState = GetShellState(commands, httpState);
            MockedFileSystem mockedFileSystem = new MockedFileSystem();
            mockedFileSystem.AddFile(_pathToScript, commands);
            RunCommand runCommand = new RunCommand(mockedFileSystem);

            await runCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            string previousCommand = shellState.CommandHistory.GetPreviousCommand();

            Assert.True(string.IsNullOrEmpty(previousCommand));
        }

        [Fact]
        public void Suggest_WithSelectedSectionAtZeroAndEmptyParseResultSection_ReturnsName()
        {
            ArrangeInputs(parseResultSections: string.Empty,
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult,
                caretPosition: 0);

            string expected = "run";

            RunCommand runCommand = new RunCommand(new MockedFileSystem());
            IEnumerable<string> result = runCommand.Suggest(shellState, httpState, parseResult);

            Assert.Single(result);
            Assert.Equal(expected, result.First());
        }

        [Fact]
        public void Suggest_WithSelectedSectionAtZeroAndParseResultSectionStartsWithName_ReturnsName()
        {
            ArrangeInputs(parseResultSections: "r",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult,
                caretPosition: 0);

            string expected = "run";

            RunCommand runCommand = new RunCommand(new MockedFileSystem());
            IEnumerable<string> result = runCommand.Suggest(shellState, httpState, parseResult);

            Assert.Single(result);
            Assert.Equal(expected, result.First());
        }

        [Fact]
        public void Suggest_WithSelectedSectionAtOneAndValidParseResultSection_ReturnsCompletionEntries()
        {
            string pathToScript = Path.Combine(Directory.GetCurrentDirectory(), "InputFileForRunCommand.txt");
            string parseResultSections = "run " + pathToScript;

            ArrangeInputs(parseResultSections: parseResultSections,
                 out MockedShellState _,
                 out HttpState httpState,
                 out ICoreParseResult parseResult,
                 caretPosition: 7);

            IShellState shellState = GetShellState(string.Empty, httpState);
            MockedFileSystem mockedFileSystem = new MockedFileSystem();
            mockedFileSystem.AddFile(pathToScript, string.Empty);

            RunCommand runCommand = new RunCommand(mockedFileSystem);
            IEnumerable<string> result = runCommand.Suggest(shellState, httpState, parseResult);

            Assert.NotEmpty(result);
        }

        private IShellState GetShellState(string inputBuffer, HttpState httpState)
        {
            DefaultCommandDispatcher<HttpState> defaultCommandDispatcher = DefaultCommandDispatcher.Create(x => { }, httpState);
            defaultCommandDispatcher.AddCommand(new SetHeaderCommand());

            Mock<IConsoleManager> mockConsoleManager = new Mock<IConsoleManager>();
            MockInputManager mockInputManager = new MockInputManager(inputBuffer);

            ShellState shellState = new ShellState(defaultCommandDispatcher,
                consoleManager: mockConsoleManager.Object,
                commandHistory: new CommandHistory(),
                inputManager: mockInputManager);

            Shell shell = new Shell(shellState);

            return shell.ShellState;
        }

        public void Dispose()
        {
            if (File.Exists(_pathToScript))
            {
                File.Delete(_pathToScript);
            }
        }
    }
}
