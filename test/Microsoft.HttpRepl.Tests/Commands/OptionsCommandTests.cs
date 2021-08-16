// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

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
    public class OptionsCommandTests : CommandTestsBase
    {
        private string _baseAddress;
        private string _path;
        private IDictionary<string, string> _urlsWithResponse = new Dictionary<string, string>();

        public OptionsCommandTests()
        {
            _baseAddress = "http://localhost:5050/";
            _path = "this/is/a/test/route";

            _urlsWithResponse.Add(_baseAddress, "Header value for root OPTIONS request.");
            _urlsWithResponse.Add(_baseAddress + _path, "Header value for OPTIONS request with route.");
        }

        [Fact]
        public async Task ExecuteAsync_WithNoBasePath_VerifyError()
        {
            ArrangeInputs(commandText: "OPTIONS",
                baseAddress: null,
                path: null,
                urlsWithResponse: null,
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult,
                out MockedFileSystem fileSystem,
                out IPreferences preferences);

            string expectedErrorMessage = Strings.Error_NoBasePath.SetColor(httpState.ErrorColor);

            OptionsCommand optionsCommand = new OptionsCommand(fileSystem, preferences, new NullTelemetry());
            await optionsCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Equal(expectedErrorMessage, shellState.ErrorMessage);
        }

        [Fact]
        public async Task ExecuteAsync_WithMultipartRoute_VerifyOutput()
        {
            ArrangeInputs(commandText: "OPTIONS",
                baseAddress: _baseAddress,
                path: _path,
                urlsWithResponse: _urlsWithResponse,
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult,
                out MockedFileSystem fileSystem,
                out IPreferences preferences,
                header: "X-HTTPREPL-TESTHEADER");

            OptionsCommand optionsCommand = new OptionsCommand(fileSystem, preferences, new NullTelemetry());
            await optionsCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            string expectedHeader = "X-HTTPREPL-TESTHEADER: Header value for OPTIONS request with route.";
            List<string> result = shellState.Output;

            Assert.True(result.Count >= 2);
            Assert.Contains("HTTP/1.1 200 OK", result);
            Assert.Contains(expectedHeader, result);
        }

        [Fact]
        public async Task ExecuteAsync_WithOnlyBaseAddress_VerifyOutput()
        {
            ArrangeInputs(commandText: "Options",
                baseAddress: _baseAddress,
                path: null,
                urlsWithResponse: _urlsWithResponse,
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult,
                out MockedFileSystem fileSystem,
                out IPreferences preferences,
                header: "X-HTTPREPL-TESTHEADER");

            OptionsCommand optionsCommand = new OptionsCommand(fileSystem, preferences, new NullTelemetry());
            await optionsCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            string expectedHeader = "X-HTTPREPL-TESTHEADER: Header value for root OPTIONS request.";
            List<string> result = shellState.Output;

            Assert.True(result.Count >= 2);
            Assert.Contains("HTTP/1.1 200 OK", result);
            Assert.Contains(expectedHeader, result);
        }
    }
}
