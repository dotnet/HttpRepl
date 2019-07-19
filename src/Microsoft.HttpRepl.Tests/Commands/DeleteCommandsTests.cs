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
    public class DeleteCommandTests : CommandTestsBase
    {
        private string _baseAddress;
        private string _path;
        private IDictionary<string, string> _urlsWithResponse = new Dictionary<string, string>();

        public DeleteCommandTests()
        {
            _baseAddress = "http://localhost:5050/";
            _path = "a/file/path.txt";

            _urlsWithResponse.Add(_baseAddress, "Root delete received successfully.");
            _urlsWithResponse.Add(_baseAddress + _path, "File path delete received successfully.");
        }

        [Fact]
        public async Task ExecuteAsync_WithNoBasePath_VerifyError()
        {
            ArrangeInputs(commandText: "DELETE",
                baseAddress: null,
                path: null,
                urlsWithResponse: null,
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult,
                out MockedFileSystem fileSystem,
                out IPreferences preferences);

            string expectedErrorMessage = Strings.Error_NoBasePath.SetColor(httpState.ErrorColor);

            DeleteCommand deleteCommand = new DeleteCommand(fileSystem, preferences);
            await deleteCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Equal(expectedErrorMessage, shellState.ErrorMessage);
        }

        [Fact]
        public async Task ExecuteAsync_WithMultipartRoute_VerifyOutput()
        {
            ArrangeInputs(commandText: "DELETE",
                baseAddress: _baseAddress,
                path: _path,
                urlsWithResponse: _urlsWithResponse,
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult,
                out MockedFileSystem fileSystem,
                out IPreferences preferences);

            DeleteCommand deleteCommand = new DeleteCommand(fileSystem, preferences);
            await deleteCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            string expectedResponse = "File path delete received successfully.";
            List<string> result = shellState.Output;

            Assert.Equal(2, result.Count);
            Assert.Contains("HTTP/1.1 200 OK", result);
            Assert.Contains(expectedResponse, result);
        }

        [Fact]
        public async Task ExecuteAsync_WithOnlyBaseAddress_VerifyOutput()
        {
            ArrangeInputs(commandText: "DELETE",
                baseAddress: _baseAddress,
                path: null,
                urlsWithResponse: _urlsWithResponse,
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult,
                out MockedFileSystem fileSystem,
                out IPreferences preferences);

            DeleteCommand deleteCommand = new DeleteCommand(fileSystem, preferences);
            await deleteCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            string expectedResponse = "Root delete received successfully.";
            List<string> result = shellState.Output;

            Assert.Equal(2, result.Count);
            Assert.Contains("HTTP/1.1 200 OK", result);
            Assert.Contains(expectedResponse, result);
        }
    }
}
