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
using Microsoft.HttpRepl.Telemetry;
using Microsoft.Repl.Parsing;
using Xunit;

namespace Microsoft.HttpRepl.Tests.Commands
{
    public class SetQueryStringCommandTests : CommandTestsBase
    {
        [Fact]
        public void CanHandle_WithParseResultSectionsLessThanTwo_ReturnsNull()
        {
            ArrangeInputs(parseResultSections: "set",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            SetQueryStringCommand setQueryStringCommand= new SetQueryStringCommand(new NullTelemetry());
            bool? result = setQueryStringCommand.CanHandle(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithFirstParseResultSectionNotEqualToName_ReturnsNull()
        {
            ArrangeInputs(parseResultSections: "test queryString name",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            SetQueryStringCommand setQueryStringCommand = new SetQueryStringCommand(new NullTelemetry());
            bool? result = setQueryStringCommand.CanHandle(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithSecondParseResultSectionNotEqualToSubCommand_ReturnsNull()
        {
            ArrangeInputs(parseResultSections: "set base name",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            SetQueryStringCommand setQueryStringCommand = new SetQueryStringCommand(new NullTelemetry());
            bool? result = setQueryStringCommand.CanHandle(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithValidInput_ReturnsTrue()
        {
            ArrangeInputs(parseResultSections: "set queryString name",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            SetQueryStringCommand setQueryStringCommand = new SetQueryStringCommand(new NullTelemetry());
            bool? result = setQueryStringCommand.CanHandle(shellState, httpState, parseResult);

            Assert.True(result);
        }

        [Fact]
        public void GetHelpSummary_ReturnsDescription()
        {
            ArrangeInputs(parseResultSections: string.Empty,
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult _);

            SetQueryStringCommand setQueryStringCommand = new SetQueryStringCommand(new NullTelemetry());
            string result = setQueryStringCommand.GetHelpSummary(shellState, httpState);

            Assert.Equal(SetQueryStringCommand.Description, result);
        }

        [Fact]
        public void GetHelpDetails_ReturnsHelpDetails()
        {
            ArrangeInputs(parseResultSections: "set queryString",
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult parseResult);

            string expected = "\u001b[1mUsage: \u001b[39mset queryString {name} [value]" + Environment.NewLine + Environment.NewLine + "Sets or clears a query string key and value. When [value] is empty the header is cleared. The value will be UrlEncoded." + Environment.NewLine;

            SetQueryStringCommand setQueryStringCommand = new SetQueryStringCommand(new NullTelemetry());
            string result = setQueryStringCommand.GetHelpDetails(shellState, httpState, parseResult);

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task ExecuteAsync_WithExactlyThreeValidParseResultSections_DoesNotUpdateHeaders()
        {
            ArrangeInputs(parseResultSections: "set queryString test",
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult parseResult);

            SetQueryStringCommand setQueryStringCommand = new SetQueryStringCommand(new NullTelemetry());
            await setQueryStringCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Dictionary<string, IEnumerable<string>> headers = httpState.Headers;
            KeyValuePair<string, IEnumerable<string>> firstHeader = headers.First();

            Assert.Equal(0,httpState.QueryString.Count);
        }

        [Fact]
        public async Task ExecuteAsync_WithMoreThanThreeValidParseResultSections_AddsEntryToHeaders()
        {
            ArrangeInputs(parseResultSections: "set queryString name value1 value2",
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult parseResult);

            SetQueryStringCommand setQueryStringCommand = new SetQueryStringCommand(new NullTelemetry());
            await setQueryStringCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Dictionary<string, IEnumerable<string>> queryString = httpState.QueryString;

            Assert.Equal(1, httpState.QueryString.Count);

            Assert.True(queryString.ContainsKey("name"));

            queryString.TryGetValue("name", out IEnumerable<string> nameHeaderValues);

            Assert.Contains("value1", nameHeaderValues);
            Assert.Contains("value2", nameHeaderValues);
        }

     
        [Fact]
        public async Task ExecuteAsync__SendsTelemetryWithHashedHeaderName()
        {
            ArrangeInputs(parseResultSections: "set queryString name value",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            TelemetryCollector telemetry = new TelemetryCollector();

            SetQueryStringCommand setQueryStringCommand = new SetQueryStringCommand(telemetry);
            await setQueryStringCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Single(telemetry.Telemetry);
            TelemetryCollector.CollectedTelemetry collectedTelemetry = telemetry.Telemetry[0];
            Assert.Equal("SetQueryString", collectedTelemetry.EventName);
            Assert.Equal(Sha256Hasher.Hash("name"), collectedTelemetry.Properties["QueryStringKey"]);
            Assert.Equal("False", collectedTelemetry.Properties["IsValueEmpty"]);
        }
    }
}
