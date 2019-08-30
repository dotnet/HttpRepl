// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.Fakes;
using Microsoft.HttpRepl.Preferences;
using Microsoft.Repl.Parsing;
using Xunit;

namespace Microsoft.HttpRepl.Tests.Commands
{
    public class EchoCommandTests : CommandTestsBase
    {
        [Theory]
        [InlineData("echo ON", true)]
        [InlineData("echo on", true)]
        [InlineData("echo oN", true)]
        [InlineData("echo OFF", true)]
        [InlineData("echo off", true)]
        [InlineData("echo oFf", true)]
        [InlineData("echo no", false)]
        [InlineData("echo yes", false)]
        public void CanHandle(string commandText, bool expected)
        {
            ArrangeInputs(commandText, out MockedShellState shellState, out HttpState httpState, out ICoreParseResult parseResult);

            EchoCommand echoCommand = new EchoCommand();

            bool? result = echoCommand.CanHandle(shellState, httpState, parseResult);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("echo o", "on", "off")]
        [InlineData("echo O", "on", "off")]
        [InlineData("echo on", "on")]
        [InlineData("echo of", "off")]
        [InlineData("echo off", "off")]
        public void GetArgumentSuggestionsForText(string commandText, params string[] expectedResults)
        {
            ArrangeInputs(commandText, out MockedShellState shellState, out HttpState httpState, out ICoreParseResult parseResult);

            EchoCommand echoCommand = new EchoCommand();

            IEnumerable<string> result = echoCommand.Suggest(shellState, httpState, parseResult);

            Assert.NotNull(result);

            List<string> resultList = result.ToList();

            Assert.Equal(expectedResults.Length, resultList.Count);

            for (int index = 0; index < expectedResults.Length; index++)
            {
                Assert.Contains(expectedResults[index], resultList, StringComparer.OrdinalIgnoreCase);
            }
        }

        [Fact]
        public async Task PostCommand_WithEchoOn_OnlyPrintsRequestBodyOnce()
        {
            string baseAddress = "https://localhost/";
            string path = "values/5";
            string content = "Test Post Body";
            ArrangeInputs(commandText: $"POST --content \"{content}\"",
                baseAddress: baseAddress,
                path: path,
                urlsWithResponse: new Dictionary<string, string>() { { "https://localhost/values/5", "" } },
                out var shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult,
                out MockedFileSystem fileSystem,
                out IPreferences preferences);
            httpState.EchoRequest = true;

            PostCommand postCommand = new PostCommand(fileSystem, preferences);
            await postCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            List<string> result = shellState.Output;

            int countOfOccurrences = result.Count(s => s?.Contains(content, StringComparison.Ordinal) == true);

            Assert.Equal(1, countOfOccurrences);
        }
    }
}
