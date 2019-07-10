// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.FileSystem;
using Microsoft.HttpRepl.IntegrationTests.Mocks;
using Microsoft.HttpRepl.Preferences;
using Microsoft.HttpRepl.UserProfile;
using Microsoft.Repl;
using Microsoft.Repl.Parsing;
using Xunit;

namespace Microsoft.HttpRepl.IntegrationTests.Commands
{
    public class EchoCommandTests
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
            Arrange(commandText, out EchoCommand echoCommand, out IShellState shellState, out HttpState httpState, out ICoreParseResult parseResult);

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
            Arrange(commandText, out EchoCommand echoCommand, out IShellState shellState, out HttpState httpState, out ICoreParseResult parseResult);

            IEnumerable<string> result = echoCommand.Suggest(shellState, httpState, parseResult);

            Assert.NotNull(result);

            List<string> resultList = result.ToList();

            Assert.Equal(expectedResults.Length, resultList.Count);

            for (int index = 0; index < expectedResults.Length; index++)
            {
                Assert.Contains(expectedResults[index], resultList, StringComparer.OrdinalIgnoreCase);
            }
        }

        private void Arrange(string commandText, out EchoCommand echoCommand, out IShellState shellState, out HttpState httpState, out ICoreParseResult parseResult)
        {
            IFileSystem fileSystem = new MockedFileSystem();
            IUserProfileDirectoryProvider userProfileDirectoryProvider = new UserProfileDirectoryProvider();
            IPreferences preferences = new UserFolderPreferences(fileSystem, userProfileDirectoryProvider, null);
            shellState = new MockedShellState();
            echoCommand = new EchoCommand();
            httpState = new HttpState(fileSystem, preferences, new HttpClient());
            parseResult = CoreParseResultHelper.Create(commandText);
        }
    }
}
