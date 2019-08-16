// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.Fakes;
using Microsoft.HttpRepl.FileSystem;
using Microsoft.HttpRepl.OpenApi;
using Microsoft.HttpRepl.Preferences;
using Microsoft.HttpRepl.UserProfile;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Parsing;
using Xunit;

namespace Microsoft.HttpRepl.Tests.Commands
{
    public class ChangeDirectoryCommandTests
    {
        [Fact]
        public void ExecuteAsync_UnknownEndpoint_DisplaysWarning()
        {
            ChangeDirectoryCommand command = new ChangeDirectoryCommand();

            Setup(commandText: "cd NotAnEndpoint", out MockedShellState mockedShellState, out HttpState httpState, out ICoreParseResult parseResult);

            ApiDefinition apiDefinition = new ApiDefinition()
            {
                DirectoryStructure = new DirectoryStructure(null)
            };
            httpState.ApiDefinition = apiDefinition;

            string expectedFirstLine = string.Format(Resources.Strings.ChangeDirectoryCommand_Warning_UnknownEndpoint, "/NotAnEndpoint").SetColor(httpState.WarningColor);
            string expectedSecondLine = "/NotAnEndpoint    []";

            command.ExecuteAsync(mockedShellState, httpState, parseResult, CancellationToken.None);

            Assert.Equal(2, mockedShellState.Output.Count);
            Assert.Equal(expectedFirstLine, mockedShellState.Output[0]);
            Assert.Equal(expectedSecondLine, mockedShellState.Output[1]);
        }

        [Fact]
        public void ExecuteAsync_KnownEndpointWithRequestMethods_NoWarning()
        {
            ChangeDirectoryCommand command = new ChangeDirectoryCommand();

            Setup(commandText: "cd AnEndpoint", out MockedShellState mockedShellState, out HttpState httpState, out ICoreParseResult parseResult);

            DirectoryStructure directoryStructure = new DirectoryStructure(null);
            DirectoryStructure childDirectory = directoryStructure.DeclareDirectory("AnEndpoint");
            RequestInfo childRequestInfo = new RequestInfo();
            childRequestInfo.AddMethod("GET");
            childDirectory.RequestInfo = childRequestInfo;
            ApiDefinition apiDefinition = new ApiDefinition()
            {
                DirectoryStructure = directoryStructure
            };
            httpState.ApiDefinition = apiDefinition;

            string expectedOutput = "/AnEndpoint    [GET]";

            command.ExecuteAsync(mockedShellState, httpState, parseResult, CancellationToken.None);

            Assert.Single(mockedShellState.Output);
            Assert.Equal(expectedOutput, mockedShellState.Output[0]);
        }

        [Fact]
        public void ExecuteAsync_KnownEndpointWithSubdirectory_NoWarning()
        {
            ChangeDirectoryCommand command = new ChangeDirectoryCommand();

            Setup(commandText: "cd AnEndpoint", out MockedShellState mockedShellState, out HttpState httpState, out ICoreParseResult parseResult);

            DirectoryStructure directoryStructure = new DirectoryStructure(null);
            DirectoryStructure childDirectory = directoryStructure.DeclareDirectory("AnEndpoint");
            DirectoryStructure grandchildDirectory = childDirectory.DeclareDirectory("AnotherEndpoint");
            ApiDefinition apiDefinition = new ApiDefinition()
            {
                DirectoryStructure = directoryStructure
            };
            httpState.ApiDefinition = apiDefinition;

            string expectedOutput = "/AnEndpoint    []";

            command.ExecuteAsync(mockedShellState, httpState, parseResult, CancellationToken.None);

            Assert.Single(mockedShellState.Output);
            Assert.Equal(expectedOutput, mockedShellState.Output[0]);
        }

        private void Setup(string commandText, out MockedShellState mockedShellState, out HttpState httpState, out ICoreParseResult parseResult)
        {
            mockedShellState = new MockedShellState();
            IFileSystem fileSystem = new RealFileSystem();
            IUserProfileDirectoryProvider userProfileDirectoryProvider = new UserProfileDirectoryProvider();
            IPreferences preferences = new UserFolderPreferences(fileSystem, userProfileDirectoryProvider, null);
            parseResult = CoreParseResultHelper.Create(commandText);
            HttpClient httpClient = new HttpClient();

            httpState = new HttpState(fileSystem, preferences, httpClient);
        }
    }
}
