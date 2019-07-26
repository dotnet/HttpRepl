// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.Fakes;
using Microsoft.Repl.Parsing;
using Xunit;

namespace Microsoft.HttpRepl.Tests.Commands
{
    public class SetBaseCommandTests : CommandTestsBase
    {
        [Fact]
        public void CanHandle_WithParseResultSectionsLessThanTwo_ReturnsNull()
        {
            ArrangeInputs(parseResultSections: "set",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            SetBaseCommand setBaseCommand = new SetBaseCommand();
            bool? result = setBaseCommand.CanHandle(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithFirstParseResultSectionNotEqualToName_ReturnsNull()
        {
            ArrangeInputs(parseResultSections: "test header name",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            SetBaseCommand setBaseCommand = new SetBaseCommand();
            bool? result = setBaseCommand.CanHandle(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithSecondParseResultSectionNotEqualToSubCommand_ReturnsNull()
        {
            ArrangeInputs(parseResultSections: "set header name",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            SetBaseCommand setBaseCommand = new SetBaseCommand();
            bool? result = setBaseCommand.CanHandle(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithValidInput_ReturnsTrue()
        {
            ArrangeInputs(parseResultSections: "set base \"https://localhost:44366/\"",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            SetBaseCommand setBaseCommand = new SetBaseCommand();
            bool? result = setBaseCommand.CanHandle(shellState, httpState, parseResult);

            Assert.True(result);
        }

        [Fact]
        public void GetHelpSummary_ReturnsDescription()
        {
            ArrangeInputs(parseResultSections: string.Empty,
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult _);

            SetBaseCommand setBaseCommand = new SetBaseCommand();
            string result = setBaseCommand.GetHelpSummary(shellState, httpState);

            Assert.Equal(setBaseCommand.Description, result);
        }

        [Fact]
        public void GetHelpDetails_ReturnsHelpDetails()
        {
            ArrangeInputs(parseResultSections: "set base",
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult parseResult);

            string expected = "\u001b[1mUsage: \u001b[39mset base [uri]" + Environment.NewLine + Environment.NewLine + "Set the base URI. e.g. `set base http://locahost:5000`" + Environment.NewLine;

            SetBaseCommand setBaseCommand = new SetBaseCommand();
            string result = setBaseCommand.GetHelpDetails(shellState, httpState, parseResult);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Suggest_WithNoParseResultSections_ReturnsName()
        {
            ArrangeInputs(parseResultSections: string.Empty,
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            string expected = "set";

            SetBaseCommand setBaseCommand = new SetBaseCommand();
            IEnumerable<string> result = setBaseCommand.Suggest(shellState, httpState, parseResult);

            Assert.Single(result);
            Assert.Equal(expected, result.First());
        }

        [Fact]
        public void Suggest_WithSelectedSectionAtZeroAndParseResultSectionStartsWithName_ReturnsName()
        {
            ArrangeInputs(parseResultSections: "s",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult,
                caretPosition: 0);

            string expected = "set";

            SetBaseCommand setBaseCommand = new SetBaseCommand();
            IEnumerable<string> result = setBaseCommand.Suggest(shellState, httpState, parseResult);

            Assert.Single(result);
            Assert.Equal(expected, result.First());
        }

        [Fact]
        public void Suggest_WithNameParseResultSectionAndSelectedSectionAtOne_ReturnsSubCommand()
        {
            ArrangeInputs(parseResultSections: "set ",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult,
                caretPosition: 4);

            string expected = "base";

            SetBaseCommand setBaseCommand = new SetBaseCommand();
            IEnumerable<string> result = setBaseCommand.Suggest(shellState, httpState, parseResult);

            Assert.Single(result);
            Assert.Equal(expected, result.First());
        }

        [Fact]
        public async Task ExecuteAsync_WithExactlyTwoParseResultSections_SetsBaseAddressAndSwaggerStructureToNull()
        {
            ArrangeInputs(parseResultSections: "set base",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            SetBaseCommand setBaseCommand = new SetBaseCommand();
            await setBaseCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Null(httpState.BaseAddress);
            Assert.Null(httpState.Structure);
        }

        [Fact]
        public async Task ExecuteAsync_IfCancellationIsRequested_SetsSwaggerStructureToNull()
        {
            string response = @"{
  ""swagger"": ""2.0"",
  ""paths"": {
    ""/api"": {
      ""get"": {
        ""tags"": [ ""Employees"" ],
        ""operationId"": ""GetEmployee"",
        ""consumes"": [],
        ""produces"": [ ""text/plain"", ""application/json"", ""text/json"" ],
        ""parameters"": [],
        ""responses"": {
          ""200"": {
            ""description"": ""Success"",
            ""schema"": {
              ""uniqueItems"": false,
              ""type"": ""array""
            }
          }
        }
      }
    }
  }
}";
            ArrangeInputs(parseResultSections: "set base \"https://localhost:44366/\"",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult,
                responseContent: response,
                baseAddress: "https://localhost:44366/");

            CancellationTokenSource cts = new CancellationTokenSource();
            cts.Cancel();

            httpState.BaseAddress = new Uri("https://localhost:44366/");

            SetBaseCommand setBaseCommand = new SetBaseCommand();
            await setBaseCommand.ExecuteAsync(shellState, httpState, parseResult, cts.Token);

            Assert.Null(httpState.Structure);
        }

        [Fact]
        public async Task ExecuteAsync_WithEmptyUri_WritesErrorToConsole()
        {
            ArrangeInputs(parseResultSections: "set base ",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            SetBaseCommand setBaseCommand = new SetBaseCommand();
            await setBaseCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            VerifyErrorMessageWasWrittenToConsoleManagerError(shellState);
        }

        [Fact]
        public async Task ExecuteAsync_WithValidInput_CreatesDirectoryStructureForSwaggerEndpoint()
        {
            string response = @"{
  ""swagger"": ""2.0"",
  ""paths"": {
    ""/api"": {
      ""get"": {
        ""tags"": [ ""Employees"" ],
        ""operationId"": ""GetEmployee"",
        ""consumes"": [],
        ""produces"": [ ""text/plain"", ""application/json"", ""text/json"" ],
        ""parameters"": [],
        ""responses"": {
          ""200"": {
            ""description"": ""Success"",
            ""schema"": {
              ""uniqueItems"": false,
              ""type"": ""array""
            }
          }
        }
      }
    }
  }
}";
            ArrangeInputs(parseResultSections: "set base \"https://localhost:44366/\"",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult,
                responseContent: response,
                baseAddress: "https://localhost:44366/swagger.json");

            SetBaseCommand setBaseCommand = new SetBaseCommand();
            await setBaseCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            IDirectoryStructure directoryStructure = httpState.Structure;

            List<string> directoryNames = directoryStructure.DirectoryNames.ToList();
            string expectedDirectoryName = "api";

            Assert.Single(directoryNames);
            Assert.Equal(expectedDirectoryName, directoryNames.First());

            IDirectoryStructure childDirectoryStructure = directoryStructure.GetChildDirectory(expectedDirectoryName);

            Assert.Empty(childDirectoryStructure.DirectoryNames);
        }
    }
}
