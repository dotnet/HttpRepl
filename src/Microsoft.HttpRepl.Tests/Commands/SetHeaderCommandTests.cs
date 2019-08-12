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
using Microsoft.Repl.Parsing;
using Xunit;

namespace Microsoft.HttpRepl.Tests.Commands
{
    public class SetHeaderCommandTests : CommandTestsBase
    {
        [Fact]
        public void CanHandle_WithParseResultSectionsLessThanTwo_ReturnsNull()
        {
            ArrangeInputs(parseResultSections: "set",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            SetHeaderCommand setHeaderCommand = new SetHeaderCommand();
            bool? result = setHeaderCommand.CanHandle(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithFirstParseResultSectionNotEqualToName_ReturnsNull()
        {
            ArrangeInputs(parseResultSections: "test header name",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            SetHeaderCommand setHeaderCommand = new SetHeaderCommand();
            bool? result = setHeaderCommand.CanHandle(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithSecondParseResultSectionNotEqualToSubCommand_ReturnsNull()
        {
            ArrangeInputs(parseResultSections: "set base name",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            SetHeaderCommand setHeaderCommand = new SetHeaderCommand();
            bool? result = setHeaderCommand.CanHandle(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithValidInput_ReturnsTrue()
        {
            ArrangeInputs(parseResultSections: "set header name",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            SetHeaderCommand setHeaderCommand = new SetHeaderCommand();
            bool? result = setHeaderCommand.CanHandle(shellState, httpState, parseResult);

            Assert.True(result);
        }

        [Fact]
        public void GetHelpSummary_ReturnsDescription()
        {
            ArrangeInputs(parseResultSections: string.Empty,
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult _);

            SetHeaderCommand setHeaderCommand = new SetHeaderCommand();
            string result = setHeaderCommand.GetHelpSummary(shellState, httpState);

            Assert.Equal(setHeaderCommand.Description, result);
        }

        [Fact]
        public void GetHelpDetails_ReturnsHelpDetails()
        {
            ArrangeInputs(parseResultSections: "set header",
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult parseResult);

            string expected = "\u001b[1mUsage: \u001b[39mset header {name} [value]" + Environment.NewLine + Environment.NewLine + "Sets or clears a header. When [value] is empty the header is cleared." + Environment.NewLine;

            SetHeaderCommand setHeaderCommand = new SetHeaderCommand();
            string result = setHeaderCommand.GetHelpDetails(shellState, httpState, parseResult);

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task ExecuteAsync_WithExactlyThreeValidParseResultSections_DoesNotUpdateHeaders()
        {
            ArrangeInputs(parseResultSections: "set header test",
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult parseResult);

            SetHeaderCommand setHeaderCommand = new SetHeaderCommand();
            await setHeaderCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Dictionary<string, IEnumerable<string>> headers = httpState.Headers;
            KeyValuePair<string, IEnumerable<string>> firstHeader = headers.First();

            Assert.Single(httpState.Headers);
            Assert.Equal("User-Agent", firstHeader.Key);
            Assert.Equal("HTTP-REPL", firstHeader.Value.First());
        }

        [Fact]
        public async Task ExecuteAsync_WithMoreThanThreeValidParseResultSections_AddsEntryToHeaders()
        {
            ArrangeInputs(parseResultSections: "set header name value1 value2",
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult parseResult);

            SetHeaderCommand setHeaderCommand = new SetHeaderCommand();
            await setHeaderCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Dictionary<string, IEnumerable<string>> headers = httpState.Headers;

            Assert.Equal(2, httpState.Headers.Count);

            Assert.True(headers.ContainsKey("User-Agent"));
            Assert.True(headers.ContainsKey("name"));

            headers.TryGetValue("User-Agent", out IEnumerable<string> userAgentHeaderValues);
            headers.TryGetValue("name", out IEnumerable<string> nameHeaderValues);

            Assert.Contains("HTTP-REPL", userAgentHeaderValues);
            Assert.Contains("value1", nameHeaderValues);
            Assert.Contains("value2", nameHeaderValues);
        }

        [Fact]
        public void Suggest_WithNoParseResultSections_ReturnsName()
        {
            ArrangeInputs(parseResultSections: string.Empty,
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult parseResult);

            SetHeaderCommand setHeaderCommand = new SetHeaderCommand();
            IEnumerable<string> suggestions = setHeaderCommand.Suggest(shellState, httpState, parseResult);

            Assert.Single(suggestions);
            Assert.Equal("set", suggestions.First());
        }

        [Fact]
        public void Suggest_WithOneParseResultSectionAndSelectedSectionGreaterAtZero_ReturnsName()
        {
            ArrangeInputs(parseResultSections: "set",
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult parseResult,
                 caretPosition: 0);

            SetHeaderCommand setHeaderCommand = new SetHeaderCommand();
            IEnumerable<string> suggestions = setHeaderCommand.Suggest(shellState, httpState, parseResult);

            Assert.Single(suggestions);
            Assert.Equal("set", suggestions.First());
        }

        [Fact]
        public void Suggest_WithOneParseResultSectionAndSelectedSectionGreaterThanZero_ReturnsName()
        {
            ArrangeInputs(parseResultSections: "set",
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult parseResult,
                 caretPosition: 3);

            SetHeaderCommand setHeaderCommand = new SetHeaderCommand();
            IEnumerable<string> suggestions = setHeaderCommand.Suggest(shellState, httpState, parseResult);

            Assert.Single(suggestions);
            Assert.Equal("set", suggestions.First());
        }

        [Fact]
        public void Suggest_WithMoreThanOneParseResultSectionAndSelectedSectionGreaterThanZero_ReturnsName()
        {
            ArrangeInputs(parseResultSections: "set header",
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult parseResult,
                 caretPosition: 10);

            SetHeaderCommand setHeaderCommand = new SetHeaderCommand();
            IEnumerable<string> suggestions = setHeaderCommand.Suggest(shellState, httpState, parseResult);

            Assert.Single(suggestions);
            Assert.Equal("header", suggestions.First());
        }

        [Fact]
        public void Suggest_WithMoreThanTwoParseResultSectionsAndSelectedSectionGreaterThanTwo_ReturnsHeaderCompletion()
        {
            ArrangeInputs(parseResultSections: "set header O",
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult parseResult,
                 caretPosition: 12);

            SetHeaderCommand setHeaderCommand = new SetHeaderCommand();
            IEnumerable<string> suggestions = setHeaderCommand.Suggest(shellState, httpState, parseResult);

            Assert.Single(suggestions);
            Assert.Equal("Origin", suggestions.First());
        }

        [Fact]
        public void Suggest_WithMoreThanThreeParseResultSectionsAndSelectedSectionAtThree_ReturnsValueCompletion()
        {
            ArrangeInputs(parseResultSections: "set header CONTENT-TYPE t",
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult parseResult,
                 caretPosition: 25);

            IDirectoryStructure directoryStructure = GetDirectoryStructure("testMethod", "testContentType", "testBody");
            ApiDefinition apiDefinition = new ApiDefinition();
            apiDefinition.DirectoryStructure = directoryStructure;
            httpState.ApiDefinition = apiDefinition;
            httpState.BaseAddress = new Uri("http://localhost:5050/");

            SetHeaderCommand setHeaderCommand = new SetHeaderCommand();
            List<string> suggestions = setHeaderCommand.Suggest(shellState, httpState, parseResult).ToList();

            Assert.Single(suggestions);
            Assert.Equal("testContentType", suggestions.First());
        }

        [Fact]
        public void Suggest_WithMoreThanThreeParseResultSectionsAndNoMatchingCompletions_ReturnsNothing()
        {
            ArrangeInputs(parseResultSections: "set header CONTENT-TYPE z",
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult parseResult,
                 caretPosition: 25);

            IDirectoryStructure directoryStructure = GetDirectoryStructure("testMethod", "testContentType", "testBody");
            ApiDefinition apiDefinition = new ApiDefinition();
            apiDefinition.DirectoryStructure = directoryStructure;
            httpState.ApiDefinition = apiDefinition;
            httpState.BaseAddress = new Uri("http://localhost:5050/");

            SetHeaderCommand setHeaderCommand = new SetHeaderCommand();
            IEnumerable<string> suggestions = setHeaderCommand.Suggest(shellState, httpState, parseResult);

            Assert.Empty(suggestions);
        }

        private IDirectoryStructure GetDirectoryStructure(string method, string contentType, string body)
        {
            RequestInfo requestInfo = new RequestInfo();
            requestInfo.SetRequestBody(method, contentType, body);

            DirectoryStructure directoryStructure = new DirectoryStructure(null);
            DirectoryStructure childDirectoryStructure = directoryStructure.DeclareDirectory(contentType);
            childDirectoryStructure.RequestInfo = requestInfo;

            return childDirectoryStructure;
        }
    }
}
