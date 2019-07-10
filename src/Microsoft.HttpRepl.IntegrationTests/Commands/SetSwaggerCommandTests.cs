// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.IntegrationTests.Mocks;
using Microsoft.HttpRepl.Resources;
using Microsoft.Repl.Parsing;
using Xunit;

namespace Microsoft.HttpRepl.IntegrationTests.Commands
{
    public class SetSwaggerCommandTests : CommandHelper
    {
        [Fact]
        public void GetHelpDetails_WithInvalidParseResultSection_ReturnsNull()
        {
            ArrangeInputs(parseResultSections: "section1",
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult parseResult);

            SetSwaggerCommand setSwaggerCommand = new SetSwaggerCommand();

            string result = setSwaggerCommand.GetHelpDetails(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public void GetHelpDetails_WithFirstParseResultSectionNotEqualToName_ReturnsNull()
        {
            ArrangeInputs(parseResultSections: "section1 section2 section3",
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult parseResult);

            SetSwaggerCommand setSwaggerCommand = new SetSwaggerCommand();

            string result = setSwaggerCommand.GetHelpDetails(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public void GetHelpDetails_WithValidInput_ReturnsDescription()
        {
            string parseResultSections = "set swagger https://localhost:44366/swagger/v1/swagger.json";

            ArrangeInputs(parseResultSections,
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult parseResult);

            SetSwaggerCommand setSwaggerCommand = new SetSwaggerCommand();

            string result = setSwaggerCommand.GetHelpDetails(shellState, httpState, parseResult);

            Assert.Equal(Strings.SetSwaggerCommand_Description, result);
        }

        [Fact]
        public void GetHelpSummary_ReturnsDescription()
        {
            ArrangeInputs(parseResultSections: string.Empty,
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult _);

            SetSwaggerCommand setSwaggerCommand = new SetSwaggerCommand();

            string result = setSwaggerCommand.GetHelpSummary(shellState, httpState);

            Assert.Equal(Strings.SetSwaggerCommand_Description, result);
        }

        [Fact]
        public void CanHandle_WithNoParseResultSections_ReturnsNull()
        {
            ArrangeInputs(parseResultSections: string.Empty,
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            SetSwaggerCommand setSwaggerCommand = new SetSwaggerCommand();

            bool? result = setSwaggerCommand.CanHandle(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithFirstSectionNotEqualToName_ReturnsNull()
        {
            ArrangeInputs(parseResultSections: "section1 section2 section3",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            SetSwaggerCommand setSwaggerCommand = new SetSwaggerCommand();

            bool? result = setSwaggerCommand.CanHandle(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithSecondSectionNotEqualToSubCommand_ReturnsNull()
        {
            ArrangeInputs(parseResultSections: "set section2 section3",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            SetSwaggerCommand setSwaggerCommand = new SetSwaggerCommand();

            bool? result = setSwaggerCommand.CanHandle(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithValidInput_ReturnsTrue()
        {
            ArrangeInputs(parseResultSections: "set swagger https://localhost:44366/swagger/v1/swagger.json",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            SetSwaggerCommand setSwaggerCommand = new SetSwaggerCommand();

            bool? result = setSwaggerCommand.CanHandle(shellState, httpState, parseResult);

            Assert.True(result.Value);
        }

        [Fact]
        public void Suggest_WithNoParseResultSections_ReturnsName()
        {
            ArrangeInputs(parseResultSections: string.Empty,
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            SetSwaggerCommand setSwaggerCommand = new SetSwaggerCommand();

            string expected = "set";
            IEnumerable<string> result = setSwaggerCommand.Suggest(shellState, httpState, parseResult);

            Assert.Single(result);
            Assert.Equal(expected, result.First());
        }

        [Fact]
        public void Suggest_WithSelectedSectionAtOne_ReturnsSubCommand()
        {
            ArrangeInputs(parseResultSections: "set swagger",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            SetSwaggerCommand setSwaggerCommand = new SetSwaggerCommand();

            string expected = "swagger";
            IEnumerable<string> result = setSwaggerCommand.Suggest(shellState, httpState, parseResult);

            Assert.Single(result);
            Assert.Equal(expected, result.First());
        }

        [Fact]
        public async Task ExecuteAsync_WithExactlyOneParseResultSection_WritesToConsoleManagerError()
        {
            ArrangeInputs(parseResultSections: "section1",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            SetSwaggerCommand setSwaggerCommand = new SetSwaggerCommand();

            await setSwaggerCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            VerifyErrorMessageWasWrittenToConsoleManagerError(shellState);
        }

        [Fact]
        public async Task ExecuteAsync_WithExactlyTwoParseResultSections_SetsHttpStateSwaggerStructureToNull()
        {
            ArrangeInputs(parseResultSections: "section1 sections2",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            SetSwaggerCommand setSwaggerCommand = new SetSwaggerCommand();

            await setSwaggerCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Null(httpState.SwaggerStructure);
        }

        [Fact]
        public async Task ExecuteAsync_WithEmptyThirdParseResultSection_WritesToConsoleManagerError()
        {
            ArrangeInputs(parseResultSections: "section1 sections2  ",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            SetSwaggerCommand setSwaggerCommand = new SetSwaggerCommand();

            await setSwaggerCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            VerifyErrorMessageWasWrittenToConsoleManagerError(shellState);
        }

        [Fact]
        public async Task ExecuteAsync_WhenThirdSectionIsNotAValidUri_WritesToConsoleManagerError()
        {
            ArrangeInputs(parseResultSections: "section1 sections2 section3",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            SetSwaggerCommand setSwaggerCommand = new SetSwaggerCommand();

            await setSwaggerCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            VerifyErrorMessageWasWrittenToConsoleManagerError(shellState);
        }

        [Fact]
        public async Task ExecuteAsync_WithoutChildDirectories_SetsHttpStateSwaggerStructure()
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
            string parseResultSections = "set swagger http://localhost:5050/somePath";
            IDirectoryStructure directoryStructure = await GetDirectoryStructure(response, parseResultSections).ConfigureAwait(false);
            List<string> directoryNames = directoryStructure.DirectoryNames.ToList();
            string expectedDirectoryName = "api";

            Assert.Single(directoryNames);
            Assert.Equal("api", directoryNames.First());

            IDirectoryStructure childDirectoryStructure = directoryStructure.GetChildDirectory(expectedDirectoryName);

            Assert.Empty(childDirectoryStructure.DirectoryNames);
        }

        [Fact]
        public async Task ExecuteAsync_WithChildDirectories_SetsHttpStateSwaggerStructureWithChildDirectorStuctureInfo()
        {
            string response = @"{
  ""swagger"": ""2.0"",
  ""paths"": {
    ""/api/Employees"": {
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
    },
    ""/api/Values"": {
      ""post"": {
        ""tags"": [ ""Values"" ],
        ""operationId"": ""Post"",
        ""consumes"": [ ""application/json-patch+json"", ""application/json"", ""text/json"", ""application/*+json"" ],
        ""produces"": [],
        ""parameters"": [
          {
            ""name"": ""value"",
            ""in"": ""body"",
            ""required"": false,
            ""schema"": { ""type"": ""string"" }
          }
        ],
        ""responses"": { ""200"": { ""description"": ""Success"" } }
      }
    }
  }
}";
            string parseResultSections = "set swagger http://localhost:5050/somePath";
            IDirectoryStructure directoryStructure = await GetDirectoryStructure(response, parseResultSections).ConfigureAwait(false);
            List<string> directoryNames = directoryStructure.DirectoryNames.ToList();
            string expectedDirectoryName = "api";

            Assert.Single(directoryNames);
            Assert.Equal(expectedDirectoryName, directoryNames.First());

            IDirectoryStructure childDirectoryStructure = directoryStructure.GetChildDirectory(expectedDirectoryName);
            List<string> childDirectoryNames = childDirectoryStructure.DirectoryNames.ToList();

            Assert.Equal(2, childDirectoryNames.Count);
            Assert.Equal("Employees", childDirectoryNames.First());
            Assert.Equal("Values", childDirectoryNames.ElementAt(1));
        }

        private async Task<IDirectoryStructure> GetDirectoryStructure(string response, string parseResultSections)
        {
            MockedShellState shellState = new MockedShellState();
            HttpState httpState = GetHttpState(response);
            ICoreParseResult parseResult = CoreParseResultHelper.Create(parseResultSections);
            SetSwaggerCommand setSwaggerCommand = new SetSwaggerCommand();

            await setSwaggerCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            return httpState.SwaggerStructure;
        }
    }
}
