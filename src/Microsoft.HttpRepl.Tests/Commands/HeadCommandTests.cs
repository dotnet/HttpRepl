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
    public class HeadCommandTests : CommandTestsBase
    {
        [Fact]
        public async Task ExecuteAsync_WithNoBasePath_VerifyError()
        {
            ArrangeInputs(commandText: "HEAD",
                baseAddress: null,
                path: null,
                urlsWithResponse: null,
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult,
                out IFileSystem fileSystem,
                out IPreferences preferences);

            string expectedErrorMessage = Strings.Error_NoBasePath.SetColor(httpState.ErrorColor);

            HeadCommand headCommand = new HeadCommand(fileSystem, preferences);
            await headCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Equal(expectedErrorMessage, shellState.ErrorMessage);
        }

        [Fact]
        public async Task ExecuteAsync_WithMultipartRoute_VerifyHeaders()
        {
            IDictionary<string, string> urlsWithResponse = new Dictionary<string, string>();
            string baseAddress = "http://localhost:5050";
            string path = "this/is/a/test/route";
            string expectedHeader = "X-HTTPREPL-TESTHEADER: Header value for HEAD request with route.";

            urlsWithResponse.Add(baseAddress, "This is a response from the root.");
            urlsWithResponse.Add(baseAddress + "/" + path, "Header value for HEAD request with route.");

            ArrangeInputs(commandText: "HEAD",
                baseAddress: baseAddress,
                path: path,
                urlsWithResponse: urlsWithResponse,
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult,
                out IFileSystem fileSystem,
                out IPreferences preferences,
                header: "X-HTTPREPL-TESTHEADER");

            HeadCommand headCommand = new HeadCommand(fileSystem, preferences);
            await headCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            List<string> result = shellState.Output;

            Assert.Equal(2, result.Count);
            Assert.Contains("HTTP/1.1 200 OK", result);
            Assert.Contains(expectedHeader, result);
        }

        [Fact]
        public async Task ExecuteAsync_WithOnlyBaseAddress_VerifyHeaders()
        {
            IDictionary<string, string> urlsWithResponse = new Dictionary<string, string>();
            string baseAddress = "http://localhost:5050";
            string expectedHeader = "X-HTTPREPL-TESTHEADER: Header value for root HEAD request.";

            urlsWithResponse.Add(baseAddress, "This is a response from the root.");
            urlsWithResponse.Add(baseAddress + "/", "Header value for root HEAD request.");

            ArrangeInputs(commandText: "HEAD",
                baseAddress: baseAddress,
                path: null,
                urlsWithResponse: urlsWithResponse,
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult,
                out IFileSystem fileSystem,
                out IPreferences preferences,
                header: "X-HTTPREPL-TESTHEADER");

            HeadCommand headCommand = new HeadCommand(fileSystem, preferences);
            await headCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            List<string> result = shellState.Output;

            Assert.Equal(2, result.Count);
            Assert.Contains("HTTP/1.1 200 OK", result);
            Assert.Contains(expectedHeader, result);
        }
    }
}
