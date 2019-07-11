// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.IntegrationTests.Mocks;
using Microsoft.Repl;
using Microsoft.Repl.Commanding;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Parsing;
using Xunit;

namespace Microsoft.HttpRepl.IntegrationTests.Commands
{
    public class ClearCommandTests : CommandTestsBase
    {
        [Fact]
        public void CanHandle_Name_ReturnsTrue()
        {
            ArrangeInputs("clear", out MockedShellState shellState, out HttpState httpState, out ICoreParseResult parseResult);

            ClearCommand clearCommand = new ClearCommand();

            bool? result = clearCommand.CanHandle(shellState, httpState, parseResult);

            Assert.True(result);
        }

        [Fact]
        public void CanHandle_AlternateName_ReturnsTrue()
        {
            ArrangeInputs("cls", out MockedShellState shellState, out HttpState httpState, out ICoreParseResult parseResult);

            ClearCommand clearCommand = new ClearCommand();

            bool? result = clearCommand.CanHandle(shellState, httpState, parseResult);

            Assert.True(result);
        }

        [Fact]
        public void Suggest_Cl_ReturnsBoth()
        {
            ArrangeInputs("cl", out MockedShellState shellState, out HttpState httpState, out ICoreParseResult parseResult);

            ClearCommand clearCommand = new ClearCommand();

            IEnumerable<string> result = clearCommand.Suggest(shellState, httpState, parseResult);

            Assert.NotNull(result);

            List<string> resultList = result.ToList();

            Assert.Equal(2, resultList.Count);
            Assert.Equal("clear", resultList[0]);
            Assert.Equal("cls", resultList[1]);
        }

        [Fact]
        public void Suggest_Cle_ReturnsClear()
        {
            ArrangeInputs("cle", out MockedShellState shellState, out HttpState httpState, out ICoreParseResult parseResult);

            ClearCommand clearCommand = new ClearCommand();

            IEnumerable<string> result = clearCommand.Suggest(shellState, httpState, parseResult);

            Assert.NotNull(result);

            List<string> resultList = result.ToList();

            Assert.Single(resultList);
            Assert.Equal("clear", resultList[0]);
        }

        [Fact]
        public void Suggest_Cls_ReturnsCls()
        {
            ArrangeInputs("cls", out MockedShellState shellState, out HttpState httpState, out ICoreParseResult parseResult);

            ClearCommand clearCommand = new ClearCommand();

            IEnumerable<string> result = clearCommand.Suggest(shellState, httpState, parseResult);

            Assert.NotNull(result);

            List<string> resultList = result.ToList();

            Assert.Single(resultList);
            Assert.Equal("cls", resultList[0]);
        }

        [Fact]
        public async Task ExecuteAsync_CallsConsoleManagerClear()
        {
            HttpState httpState = GetHttpState();
            ICoreParseResult parseResult = CreateCoreParseResult("clear");
            DefaultCommandDispatcher<HttpState> dispatcher = DefaultCommandDispatcher.Create((ss) => { }, httpState);
            LoggingConsoleManagerDecorator consoleManager = new LoggingConsoleManagerDecorator(new ConsoleManagerStub());
            IShellState shellState = new ShellState(dispatcher, consoleManager: consoleManager);

            ClearCommand clearCommand = new ClearCommand();

            await clearCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.True(consoleManager.WasClearCalled);
        }

        [Fact]
        public async Task ExecuteAsync_CallsDispatcherOnReady()
        {
            bool wasOnReadyCalled = false;

            HttpState httpState = GetHttpState();
            ICoreParseResult parseResult = CreateCoreParseResult("clear");
            DefaultCommandDispatcher<HttpState> dispatcher = DefaultCommandDispatcher.Create((ss) => wasOnReadyCalled = true, httpState);
            IConsoleManager consoleManager = new LoggingConsoleManagerDecorator(new ConsoleManagerStub());
            IShellState shellState = new ShellState(dispatcher, consoleManager: consoleManager);

            ClearCommand clearCommand = new ClearCommand();

            await clearCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.True(wasOnReadyCalled);
        }
    }
}
