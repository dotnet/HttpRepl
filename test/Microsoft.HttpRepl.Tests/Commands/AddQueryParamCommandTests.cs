// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.Fakes;
using Microsoft.HttpRepl.Telemetry;
using Microsoft.Repl.Parsing;
using Xunit;

namespace Microsoft.HttpRepl.Tests.Commands
{
    public class AddQueryParamCommandTests : CommandTestsBase
    {
        [Fact]
        public void CanHandle_WithParseResultSectionsLessThanTwo_ReturnsNull()
        {
            ArrangeInputs(parseResultSections: "add",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            AddQueryParamCommand addQueryParamCommand= new AddQueryParamCommand(new NullTelemetry());
            bool? result = addQueryParamCommand.CanHandle(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithFirstParseResultSectionNotEqualToName_ReturnsNull()
        {
            ArrangeInputs(parseResultSections: "test query-param name",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            AddQueryParamCommand addQueryParamCommand = new AddQueryParamCommand(new NullTelemetry());
            bool? result = addQueryParamCommand.CanHandle(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithSecondParseResultSectionNotEqualToSubCommand_ReturnsNull()
        {
            ArrangeInputs(parseResultSections: "add base name",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            AddQueryParamCommand addQueryParamCommand = new AddQueryParamCommand(new NullTelemetry());
            bool? result = addQueryParamCommand.CanHandle(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithValidInput_ReturnsTrue()
        {
            ArrangeInputs(parseResultSections: "add query-param name value",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            AddQueryParamCommand setQueryParamCommand = new AddQueryParamCommand(new NullTelemetry());
            bool? result = setQueryParamCommand.CanHandle(shellState, httpState, parseResult);

            Assert.True(result);
        }

        [Fact]
        public void GetHelpDetails_ReturnsHelpDetails()
        {
            ArrangeInputs(parseResultSections: "add query-param",
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult parseResult);

            string expected = "\u001b[1mUsage: \u001b[39madd query-param {name} [value]" + Environment.NewLine
                + Environment.NewLine + "Adds a key and value pair to the query string. " +
                "The key and value will be UrlEncoded. Multiple values may be mapped to the same key." + Environment.NewLine;

            AddQueryParamCommand addQueryParamCommand = new AddQueryParamCommand(new NullTelemetry());
            string result = addQueryParamCommand.GetHelpDetails(shellState, httpState, parseResult);

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task ExecuteAsync_ValidCommandInvalidInput_ThrowsException()
        {
            ArrangeInputs(parseResultSections: "add query-param test",
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult parseResult);

            AddQueryParamCommand addQueryParamCommand = new AddQueryParamCommand(new NullTelemetry());
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>  await addQueryParamCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None));

            Assert.Empty(httpState.QueryParam);
        }

        [Fact]
        public async Task ExecuteAsync_WithMultipleValuesMapped()
        {
            ArrangeInputs(parseResultSections: "add query-param name value1 name value2",
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult parseResult);

            AddQueryParamCommand addQueryParamCommand = new AddQueryParamCommand(new NullTelemetry());
            await addQueryParamCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Dictionary<string, IEnumerable<string>> queryParam = httpState.QueryParam;

            Assert.Single(httpState.QueryParam);

            Assert.True(queryParam.ContainsKey("name"));

            queryParam.TryGetValue("name", out IEnumerable<string> nameHeaderValues);

            Assert.Contains("value1", nameHeaderValues);

            ArrangeInputs(parseResultSections: "add query-param name value2",
                 out MockedShellState shellStateTwo,
                 out HttpState httpStateTwo,
                 out ICoreParseResult parseResultTwo);

            await addQueryParamCommand.ExecuteAsync(shellStateTwo, httpState, parseResultTwo, CancellationToken.None);
            queryParam = httpState.QueryParam;

            Assert.Single(httpState.QueryParam);

            Assert.True(queryParam.ContainsKey("name"));

            queryParam.TryGetValue("name", out IEnumerable<string> nameHeaderValuesTwo);

            Assert.Contains("value1", nameHeaderValuesTwo);
            Assert.Contains("value2", nameHeaderValuesTwo);
        }


        [Fact]
        public async Task ExecuteAsync__SendsTelemetryWithHashedHeaderName()
        {
            ArrangeInputs(parseResultSections: "add query-param name value",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            TelemetryCollector telemetry = new TelemetryCollector();

            AddQueryParamCommand addQueryParamCommand = new AddQueryParamCommand(telemetry);
            await addQueryParamCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Single(telemetry.Telemetry);
            TelemetryCollector.CollectedTelemetry collectedTelemetry = telemetry.Telemetry[0];
            Assert.Equal("AddQueryParam", collectedTelemetry.EventName);
            Assert.Equal(Sha256Hasher.Hash("name"), collectedTelemetry.Properties["QueryParamKey"]);
            Assert.Equal("False", collectedTelemetry.Properties["IsValueEmpty"]);
        }
    }
}
