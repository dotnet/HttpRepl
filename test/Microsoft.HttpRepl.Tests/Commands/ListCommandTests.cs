// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.Fakes;
using Microsoft.HttpRepl.OpenApi;
using Microsoft.HttpRepl.Preferences;
using Microsoft.Repl.Commanding;
using Microsoft.Repl.Parsing;
using Xunit;

namespace Microsoft.HttpRepl.Tests.Commands
{
    public class ListCommandTests : CommandTestsBase
    {
        [Fact]
        public async Task ExecuteAsync_NoBaseAddressOnHttpState_ShowsWarning()
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

            httpState.BaseAddress = null;
            httpState.SwaggerEndpoint = new Uri("http://localhost/swagger.json");
            ApiDefinition apiDefinition = new ApiDefinition()
            {
                DirectoryStructure = new DirectoryStructure(null)
            };
            httpState.ApiDefinition = apiDefinition;

            ListCommand listCommand = new ListCommand(preferences);

            await listCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            string actualOutput = string.Join(Environment.NewLine, shellState.Output);
            Assert.Equal(Resources.Strings.ListCommand_Error_NoBaseAddress, actualOutput);
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

            httpState.SwaggerEndpoint = null;
            ApiDefinition apiDefinition = new ApiDefinition()
            {
                DirectoryStructure = new DirectoryStructure(null)
            };
            httpState.ApiDefinition = apiDefinition;

            ListCommand listCommand = new ListCommand(preferences);

            await listCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            string actualOutput = string.Join(Environment.NewLine, shellState.Output);
            Assert.Equal(Resources.Strings.ListCommand_Error_NoDirectoryStructure, actualOutput);
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

            httpState.SwaggerEndpoint = new Uri("http://localhost/swagger.json");
            httpState.ApiDefinition = null;

            ListCommand listCommand = new ListCommand(preferences);

            await listCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            string actualOutput = string.Join(Environment.NewLine, shellState.Output);
            Assert.Equal(Resources.Strings.ListCommand_Error_NoDirectoryStructure, actualOutput);
        }

        [Fact]
        public async Task ExecuteAsync_WithBaseAddressSwaggerAndStructure_NoWarning()
        {
            string response = @"{
  ""swagger"": ""2.0"",
  ""info"": {
    ""title"": ""OpenAPI v2 Spec"",
    ""version"": ""v1""
  },
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

            ArrangeInputs(commandText: "ls",
                          baseAddress: "http://localhost/",
                          path: "/",
                          urlsWithResponse: new Dictionary<string, string>() { { "http://localhost/swagger.json", response } },
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out _,
                          out IPreferences preferences);

            httpState.SwaggerEndpoint = new Uri("http://localhost/swagger.json");

            ListCommand listCommand = new ListCommand(preferences);

            await listCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            string actualOutput = string.Join(Environment.NewLine, shellState.Output);
            Assert.DoesNotContain(Resources.Strings.ListCommand_Error_NoBaseAddress, actualOutput, StringComparison.Ordinal);
            Assert.DoesNotContain(Resources.Strings.ListCommand_Error_NoDirectoryStructure, actualOutput, StringComparison.Ordinal);
        }

        [Fact]
        public void Suggest_WithBlankSecondParameter_NoExceptionAndCorrectSuggestions()
        {
            // Arrange
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

            // Act
            List<string> suggestions = listCommand.Suggest(shellState, httpState, parseResult).ToList();

            // Assert
            for (int optionIndex = 0; optionIndex < listCommand.InputSpec.Options.Count; optionIndex++)
            {
                CommandOptionSpecification option = listCommand.InputSpec.Options[optionIndex];
                for (int formIndex = 0; formIndex < option.Forms.Count; formIndex++)
                {
                    Assert.Contains(option.Forms[formIndex], suggestions, StringComparer.Ordinal);
                }
            }
        }

        [Fact]
        public async Task ExecuteAsync_WithMethods_MethodsAreUppercase()
        {
            string response = @"{
  ""swagger"": ""2.0"",
  ""info"": {
    ""title"": ""OpenAPI v2 Spec"",
    ""version"": ""v1""
  },
  ""paths"": {
    ""/api"": {
      ""get"": {
      },
      ""post"": {
      }
    }
  }
}";

            ArrangeInputs(commandText: "ls",
                          baseAddress: "http://localhost/",
                          path: "/api",
                          urlsWithResponse: new Dictionary<string, string>() { { "http://localhost/swagger.json", response } },
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out _,
                          out IPreferences preferences);

            httpState.SwaggerEndpoint = new Uri("http://localhost/swagger.json");

            ListCommand listCommand = new ListCommand(preferences);

            await listCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            string actualOutput = string.Join(Environment.NewLine, shellState.Output);
            Assert.Contains("[GET|POST]", actualOutput, StringComparison.Ordinal);
        }
    }
}
