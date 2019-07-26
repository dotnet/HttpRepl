// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.Fakes;
using Microsoft.HttpRepl.Preferences;
using Microsoft.HttpRepl.Resources;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Parsing;
using Xunit;

namespace Microsoft.HttpRepl.Tests.Commands
{
    public class GetCommandTests : CommandTestsBase
    {
        private string _baseAddress;
        private string _path;
        private IDictionary<string, string> _urlsWithResponse = new Dictionary<string, string>();

        public GetCommandTests()
        {
            _baseAddress = "http://localhost:5050/";
            _path = "this/is/a/test/route";

            _urlsWithResponse.Add(_baseAddress, "This is a response from the root.");
            _urlsWithResponse.Add(_baseAddress + _path, "This is a test response.");
        }

        [Fact]
        public async Task ExecuteAsync_WithNoBasePath_VerifyError()
        {
            ArrangeInputs(commandText: "GET",
                baseAddress: null,
                path: null,
                urlsWithResponse: null,
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult,
                out MockedFileSystem fileSystem,
                out IPreferences preferences);

            string expectedErrorMessage = Strings.Error_NoBasePath.SetColor(httpState.ErrorColor);

            GetCommand getCommand = new GetCommand(fileSystem, preferences);
            await getCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Equal(expectedErrorMessage, shellState.ErrorMessage);
        }

        [Fact]
        public async Task ExecuteAsync_WithMultipartRoute_VerifyOutput()
        {
            ArrangeInputs(commandText: "GET",
                baseAddress: _baseAddress,
                path: _path,
                urlsWithResponse: _urlsWithResponse,
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult,
                out MockedFileSystem fileSystem,
                out IPreferences preferences);

            GetCommand getCommand = new GetCommand(fileSystem, preferences);
            await getCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            string expectedResponse = "This is a test response.";
            List<string> result = shellState.Output;

            Assert.Equal(2, result.Count);
            Assert.Equal("HTTP/1.1 200 OK", result[0]);
            Assert.Equal(expectedResponse, result[1]);
        }

        [Fact]
        public async Task ExecuteAsync_WithOnlyBaseAddress_VerifyOutput()
        {
            ArrangeInputs(commandText: "GET",
                baseAddress: _baseAddress,
                path: null,
                urlsWithResponse: _urlsWithResponse,
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult,
                out MockedFileSystem fileSystem,
                out IPreferences preferences);

            GetCommand getCommand = new GetCommand(fileSystem, preferences);
            await getCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            string expectedResponse = "This is a response from the root.";
            List<string> result = shellState.Output;

            Assert.Equal(2, result.Count);
            Assert.Equal("HTTP/1.1 200 OK", result[0]);
            Assert.Equal(expectedResponse, result[1]);
        }

        [Fact]
        public async Task ExecuteAsync_WithJsonContentTypeInHeader_FormatsResponseContent()
        {
            string json = @"{
""swagger"": ""2.0"",
""info"": {
""version"": ""v1""
},
""paths"": {
""/api/Employees"": {
}
}
}";
            string path  = "this/is/a/test/route/for/formatting";

            _urlsWithResponse.Add(_baseAddress + path, json);

            ArrangeInputs(commandText: "GET",
                baseAddress: _baseAddress,
                path: path,
                urlsWithResponse: _urlsWithResponse,
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult,
                out MockedFileSystem fileSystem,
                out IPreferences preferences,
                contentType: "application/json");

            GetCommand getCommand = new GetCommand(fileSystem, preferences);
            await getCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            string expectedResponse = @"{
  ""swagger"": ""2.0"",
  ""info"": {
    ""version"": ""v1""
  },
  ""paths"": {
    ""/api/Employees"": {
    }
  }
}";
            List<string> result = shellState.Output;

            Assert.Equal(3, result.Count);
            Assert.Equal("HTTP/1.1 200 OK", result[0]);
            Assert.Equal("Content-Type: application/json", result[1]);
            Assert.Equal(expectedResponse, result[2]);
        }

        [Fact]
        public async Task ExecuteAsync_WithTextContentTypeInHeader_DoesNotFormatResponseContent()
        {
            string unformattedResponse = "This is an        unformatted response.      ";

            string path = "this/is/a/test/route/for/formatting";

            _urlsWithResponse.Add(_baseAddress + path, unformattedResponse);

            ArrangeInputs(commandText: "GET",
                baseAddress: _baseAddress,
                path: path,
                urlsWithResponse: _urlsWithResponse,
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult,
                out MockedFileSystem fileSystem,
                out IPreferences preferences,
                contentType: "text/plain");

            GetCommand getCommand = new GetCommand(fileSystem, preferences);
            await getCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            List<string> result = shellState.Output;

            Assert.Equal(3, result.Count);
            Assert.Equal("HTTP/1.1 200 OK", result[0]);
            Assert.Equal("Content-Type: text/plain", result[1]);
            Assert.Equal(unformattedResponse, result[2]);
        }

        [Fact]
        public async Task ExecuteAsync_WithHeaderOptionAndEchoOn_VerifyOutputContainsRequestAndReaponseHeaders()
        {
            string unformattedResponse = "This is a test response.";

            string path = "this/is/a/test/route/for/formatting";

            _urlsWithResponse.Add(_baseAddress + path, unformattedResponse);

            ArrangeInputs(commandText: "GET --header Accept=text/plain",
                baseAddress: _baseAddress,
                path: path,
                urlsWithResponse: _urlsWithResponse,
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult,
                out MockedFileSystem fileSystem,
                out IPreferences preferences,
                contentType: "text/plain");

            httpState.EchoRequest = true;

            GetCommand getCommand = new GetCommand(fileSystem, preferences);
            await getCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            List<string> result = shellState.Output;

            Assert.Equal(8, result.Count);
            Assert.Equal("Request to http://localhost:5050...", result[0]);
            Assert.Equal("GET /this/is/a/test/route/for/formatting HTTP/1.1", result[1]);
            Assert.Equal("Accept: text/plain", result[2]);
            Assert.Equal("User-Agent: HTTP-REPL", result[3]);
            Assert.Equal("Response from http://localhost:5050...", result[4]);
            Assert.Equal("HTTP/1.1 200 OK", result[5]);
            Assert.Equal("Content-Type: text/plain", result[6]);
            Assert.Equal(unformattedResponse, result[7]);
        }
    }
}
