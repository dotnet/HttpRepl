// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.FileSystem;
using Microsoft.HttpRepl.IntegrationTests.Mocks;
using Microsoft.HttpRepl.Preferences;
using Microsoft.HttpRepl.UserProfile;
using Microsoft.Repl;
using Microsoft.Repl.Commanding;
using Microsoft.Repl.Parsing;
using Xunit;

namespace Microsoft.HttpRepl.IntegrationTests.Commands
{
    public class ClearCommandTests
    {
        [Fact]
        public void CanHandle_Name_ReturnsTrue()
        {
            Arrange(commandText: "clear", out ClearCommand clearCommand, out IShellState shellState, out HttpState httpState, out ICoreParseResult parseResult);

            var result = clearCommand.CanHandle(shellState, httpState, parseResult);

            Assert.True(result);
        }

        [Fact]
        public void CanHandle_AlternateName_ReturnsTrue()
        {
            Arrange(commandText: "cls", out ClearCommand clearCommand, out IShellState shellState, out HttpState httpState, out ICoreParseResult parseResult);

            var result = clearCommand.CanHandle(shellState, httpState, parseResult);

            Assert.True(result);
        }

        [Fact]
        public void Suggest_Cl_ReturnsBoth()
        {
            Arrange(commandText: "cl", out ClearCommand clearCommand, out IShellState shellState, out HttpState httpState, out ICoreParseResult parseResult);

            var result = clearCommand.Suggest(shellState, httpState, parseResult);

            Assert.NotNull(result);

            List<string> resultList = result.ToList();

            Assert.Equal(2, resultList.Count);
            Assert.Equal("clear", resultList[0]);
            Assert.Equal("cls", resultList[1]);
        }

        [Fact]
        public void Suggest_Cle_ReturnsClear()
        {
            Arrange(commandText: "cle", out ClearCommand clearCommand, out IShellState shellState, out HttpState httpState, out ICoreParseResult parseResult);

            var result = clearCommand.Suggest(shellState, httpState, parseResult);

            Assert.NotNull(result);

            List<string> resultList = result.ToList();

            Assert.Single(resultList);
            Assert.Equal("clear", resultList[0]);
        }

        [Fact]
        public void Suggest_Cls_ReturnsCls()
        {
            Arrange(commandText: "cls", out ClearCommand clearCommand, out IShellState shellState, out HttpState httpState, out ICoreParseResult parseResult);

            var result = clearCommand.Suggest(shellState, httpState, parseResult);

            Assert.NotNull(result);

            List<string> resultList = result.ToList();

            Assert.Single(resultList);
            Assert.Equal("cls", resultList[0]);
        }

        [Fact]
        public async Task ExecuteAsync_CallsConsoleManagerClear()
        {
            Arrange(commandText: "clear", out ClearCommand clearCommand, out HttpState httpState, out ICoreParseResult parseResult);
            ArrangeShellStateStub(httpState, (ss) => { }, out IShellState shellState, out LoggingConsoleManagerDecorator consoleManager);

            await clearCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.True(consoleManager.WasClearCalled);
        }

        [Fact]
        public async Task ExecuteAsync_CallsDispatcherOnReady()
        {
            bool wasOnReadyCalled = false;
            Arrange(commandText: "clear", out ClearCommand clearCommand, out HttpState httpState, out ICoreParseResult parseResult);
            ArrangeShellStateStub(httpState, (ss) => wasOnReadyCalled = true, out IShellState shellState, out _);

            await clearCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.True(wasOnReadyCalled);
        }

        private void ArrangeShellStateStub(HttpState httpState, Action<IShellState> onReadyAction, out IShellState shellState, out LoggingConsoleManagerDecorator consoleManager)
        {
            DefaultCommandDispatcher<HttpState> dispatcher = DefaultCommandDispatcher.Create(onReadyAction, httpState);
            consoleManager = new LoggingConsoleManagerDecorator(new ConsoleManagerStub());
            shellState = new ShellState(dispatcher, consoleManager: consoleManager);
        }

        private void Arrange(string commandText, out ClearCommand clearCommand, out IShellState shellState, out HttpState httpState, out ICoreParseResult parseResult)
        {
            shellState = new MockedShellState();
            Arrange(commandText, out clearCommand, out httpState, out parseResult);
        }

        private void Arrange(string commandText, out ClearCommand clearCommand, out HttpState httpState, out ICoreParseResult parseResult)
        {
            IFileSystem fileSystem = new MockedFileSystem();
            IUserProfileDirectoryProvider userProfileDirectoryProvider = new UserProfileDirectoryProvider();
            IPreferences preferences = new UserFolderPreferences(fileSystem, userProfileDirectoryProvider, null);
            clearCommand = new ClearCommand();
            httpState = new HttpState(fileSystem, preferences, new HttpClient());
            parseResult = CoreParseResultHelper.Create(commandText);
        }
    }
}
