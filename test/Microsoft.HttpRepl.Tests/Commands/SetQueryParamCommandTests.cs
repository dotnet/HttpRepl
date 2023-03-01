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
    public class SetQueryParamCommandTests : CommandTestsBase
    {
        [Fact]
        public void CanHandle_WithParseResultSectionsLessThanTwo_ReturnsNull()
        {
            ArrangeInputs(parseResultSections: "set",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            SetQueryParamCommand setQueryParamCommand= new SetQueryParamCommand(new NullTelemetry());
            bool? result = setQueryParamCommand.CanHandle(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithFirstParseResultSectionNotEqualToName_ReturnsNull()
        {
            ArrangeInputs(parseResultSections: "test query-param name",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            SetQueryParamCommand setQueryParamCommand = new SetQueryParamCommand(new NullTelemetry());
            bool? result = setQueryParamCommand.CanHandle(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithSecondParseResultSectionNotEqualToSubCommand_ReturnsNull()
        {
            ArrangeInputs(parseResultSections: "set base name",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            SetQueryParamCommand setQueryParamCommand = new SetQueryParamCommand(new NullTelemetry());
            bool? result = setQueryParamCommand.CanHandle(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithValidInput_ReturnsTrue()
        {
            ArrangeInputs(parseResultSections: "set query-param name",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            SetQueryParamCommand setQueryParamCommand = new SetQueryParamCommand(new NullTelemetry());
            bool? result = setQueryParamCommand.CanHandle(shellState, httpState, parseResult);

            Assert.True(result);
        }

        [Fact]
        public void GetHelpSummary_ReturnsDescription()
        {
            ArrangeInputs(parseResultSections: string.Empty,
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult _);

            SetQueryParamCommand setQueryParamCommand = new SetQueryParamCommand(new NullTelemetry());
            string result = setQueryParamCommand.GetHelpSummary(shellState, httpState);

            Assert.Equal(SetQueryParamCommand.Description, result);
        }

        [Fact]
        public void GetHelpDetails_ReturnsHelpDetails()
        {
            ArrangeInputs(parseResultSections: "set query-param",
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult parseResult);

            string expected = "\u001b[1mUsage: \u001b[39mset query-param {name} [value]" + Environment.NewLine + Environment.NewLine + "Sets or clears a query string key and value. When [value] is empty the header is cleared. The key and value will be UrlEncoded." + Environment.NewLine;

            SetQueryParamCommand setQueryParamCommand = new SetQueryParamCommand(new NullTelemetry());
            string result = setQueryParamCommand.GetHelpDetails(shellState, httpState, parseResult);

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task ExecuteAsync_WithExactlyThreeValidParseResultSections_DoesNotUpdateHeaders()
        {
            ArrangeInputs(parseResultSections: "set query-param test",
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult parseResult);

            SetQueryParamCommand setQueryParamCommand = new SetQueryParamCommand(new NullTelemetry());
            await setQueryParamCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Dictionary<string, IEnumerable<string>> headers = httpState.Headers;
            KeyValuePair<string, IEnumerable<string>> firstHeader = headers.First();

            Assert.Empty(httpState.QueryParam);
        }

        [Fact]
        public async Task ExecuteAsync_WithMoreThanThreeValidParseResultSections_AddsEntryToHeaders()
        {
            ArrangeInputs(parseResultSections: "set query-param name value1 value2",
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult parseResult);

            SetQueryParamCommand setQueryParamCommand = new SetQueryParamCommand(new NullTelemetry());
            await setQueryParamCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Dictionary<string, IEnumerable<string>> queryParam = httpState.QueryParam;

            Assert.Single(httpState.QueryParam);

            Assert.True(queryParam.ContainsKey("name"));

            queryParam.TryGetValue("name", out IEnumerable<string> nameHeaderValues);

            Assert.Contains("value1", nameHeaderValues);
            Assert.Contains("value2", nameHeaderValues);
        }


        [Fact]
        public async Task ExecuteAsync__SendsTelemetryWithHashedHeaderName()
        {
            ArrangeInputs(parseResultSections: "set query-param name value",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            TelemetryCollector telemetry = new TelemetryCollector();

            SetQueryParamCommand setQueryParamCommand = new SetQueryParamCommand(telemetry);
            await setQueryParamCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Single(telemetry.Telemetry);
            TelemetryCollector.CollectedTelemetry collectedTelemetry = telemetry.Telemetry[0];
            Assert.Equal("SetQueryParam", collectedTelemetry.EventName);
            Assert.Equal(Sha256Hasher.Hash("name"), collectedTelemetry.Properties["QueryParamKey"]);
            Assert.Equal("False", collectedTelemetry.Properties["IsValueEmpty"]);
        }
    }
}
