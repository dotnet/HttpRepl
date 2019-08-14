// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.Fakes;
using Microsoft.HttpRepl.Preferences;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Parsing;
using Xunit;

namespace Microsoft.HttpRepl.Tests.Commands
{
    public class ListCommandTests : CommandTestsBase
    {
        [Fact]
        public async Task ExecuteAsync_NoBaseAddress_ShowsWarning()
        {
            ArrangeInputs(commandText: "ls",
                          baseAddress: "http://lcoalhost/",
                          path: "/",
                          urlsWithResponse: null,
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out _,
                          out IPreferences preferences);

            httpState.BaseAddress = null;
            httpState.SwaggerEndpoint = new Uri("http://localhost/swagger.json");
            httpState.Structure = new DirectoryStructure(null);

            ListCommand listCommand = new ListCommand(preferences);

            await listCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Single(shellState.Output);
            Assert.Equal(Resources.Strings.ListCommand_Error_NoBaseAddress, shellState.Output[0]);
        }

        [Fact]
        public async Task ExecuteAsync_NoSwagger_ShowsWarning()
        {
            ArrangeInputs(commandText: "ls",
                          baseAddress: "http://localhost/",
                          path: "/",
                          urlsWithResponse: null,
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out _,
                          out IPreferences preferences);

            httpState.BaseAddress = new Uri("http://localhost/swagger.json");
            httpState.SwaggerEndpoint = null;
            httpState.Structure = new DirectoryStructure(null);

            ListCommand listCommand = new ListCommand(preferences);

            await listCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Single(shellState.Output);
            Assert.Equal(Resources.Strings.ListCommand_Error_NoDirectoryStructure, shellState.Output[0]);
        }

        [Fact]
        public async Task ExecuteAsync_NoStructure_ShowsWarning()
        {
            ArrangeInputs(commandText: "ls",
                          baseAddress: "http://localhost/",
                          path: "/",
                          urlsWithResponse: null,
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out _,
                          out IPreferences preferences);

            httpState.BaseAddress = new Uri("http://localhost/swagger.json");
            httpState.SwaggerEndpoint = new Uri("http://localhost/swagger.json");
            httpState.Structure = null;

            ListCommand listCommand = new ListCommand(preferences);

            await listCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Single(shellState.Output);
            Assert.Equal(Resources.Strings.ListCommand_Error_NoDirectoryStructure, shellState.Output[0]);
        }

        [Fact]
        public void Suggest_WithBlankSecondParameter_NoExceptionAndNullSuggestions()
        {
            ArrangeInputs(commandText: "dir ",
                          baseAddress: "https://localhost/",
                          path: "/",
                          urlsWithResponse: null,
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out _,
                          out IPreferences preferences);

            ListCommand listCommand = new ListCommand(preferences);

            IEnumerable<string> suggestions = listCommand.Suggest(shellState, httpState, parseResult);

            Assert.Empty(suggestions);
        }
    }
}
