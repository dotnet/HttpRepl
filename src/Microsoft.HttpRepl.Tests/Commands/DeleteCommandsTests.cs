// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.Fakes;
using Microsoft.HttpRepl.FileSystem;
using Microsoft.HttpRepl.Preferences;
using Microsoft.HttpRepl.Resources;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Parsing;
using Xunit;

namespace Microsoft.HttpRepl.Tests.Commands
{
    public class DeleteCommandTests : CommandTestsBase
    {
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
                out IFileSystem fileSystem,
                out IPreferences preferences);

            string expectedErrorMessage = Strings.Error_NoBasePath.SetColor(httpState.ErrorColor);

            DeleteCommand deleteCommand = new DeleteCommand(fileSystem, preferences);
            await deleteCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Equal(expectedErrorMessage, shellState.ErrorMessage);
        }

        [Fact]
        public async Task ExecuteAsync_WithMultipartRoute_VerifyOutput()
        {
            IDictionary<string, string> urlsWithResponse = new Dictionary<string, string>();
            string baseAddress = "http://localhost:5050";
            string path = "a/file/path.txt";
            string response = "File path delete received successfully.";

            urlsWithResponse.Add(baseAddress, "Root delete received successfully.");
            urlsWithResponse.Add(baseAddress + "/" + path, response);

            ArrangeInputs(commandText: "DELETE",
                baseAddress: baseAddress,
                path: path,
                urlsWithResponse: urlsWithResponse,
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult,
                out IFileSystem fileSystem,
                out IPreferences preferences);

            DeleteCommand deleteCommand = new DeleteCommand(fileSystem, preferences);
            await deleteCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            List<string> result = shellState.Output;

            Assert.Equal(2, result.Count);
            Assert.Contains("HTTP/1.1 200 OK", result);
            Assert.Contains(response, result);
        }

        [Fact]
        public async Task ExecuteAsync_WithOnlyBaseAddress_VerifyOutput()
        {
            IDictionary<string, string> urlsWithResponse = new Dictionary<string, string>();
            string baseAddress = "http://localhost:5050";
            string response = "File path delete received successfully.";

            urlsWithResponse.Add(baseAddress, "Root delete received successfully.");
            urlsWithResponse.Add(baseAddress + "/", response);

            ArrangeInputs(commandText: "DELETE",
                baseAddress: baseAddress,
                path: null,
                urlsWithResponse: urlsWithResponse,
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult,
                out IFileSystem fileSystem,
                out IPreferences preferences);

            DeleteCommand deleteCommand = new DeleteCommand(fileSystem, preferences);
            await deleteCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            List<string> result = shellState.Output;

            Assert.Equal(2, result.Count);
            Assert.Contains("HTTP/1.1 200 OK", result);
            Assert.Contains(response, result);
        }
    }
}
