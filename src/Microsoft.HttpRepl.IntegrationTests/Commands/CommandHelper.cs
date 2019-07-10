// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using Microsoft.HttpRepl.FileSystem;
using Microsoft.HttpRepl.IntegrationTests.Mocks;
using Microsoft.HttpRepl.IntegrationTests.Preferences;
using Microsoft.HttpRepl.Preferences;
using Microsoft.HttpRepl.UserProfile;
using Microsoft.Repl;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Parsing;
using Moq;

namespace Microsoft.HttpRepl.IntegrationTests.Commands
{
    public class CommandHelper
    {
        protected void ArrangeInputs(string parseResultSections,
            out MockedShellState shellState,
            out HttpState httpState,
            out ICoreParseResult parseResult)
        {
            parseResult = CoreParseResultHelper.Create(parseResultSections);
            shellState = new MockedShellState();
            httpState = GetHttpState(string.Empty);
        }

        protected void VerifyErrorMessageWasWrittenToConsoleManagerError(IShellState shellState)
        {
            Mock<IWritable> error = Mock.Get(shellState.ConsoleManager.Error);

            error.Verify(s => s.WriteLine(It.IsAny<string>()), Times.Once);
        }

        protected HttpState GetHttpState(string content)
        {
            IFileSystem fileSystem = new MockedFileSystem();
            IPreferences preferences = new UserFolderPreferences(fileSystem, new UserProfileDirectoryProvider(), TestDefaultPreferences.GetDefaultPreferences());

            HttpResponseMessage responseMessage = new HttpResponseMessage();
            responseMessage.Content = new MockHttpContent(content);
            MockHttpMessageHandler messageHandler = new MockHttpMessageHandler(responseMessage);
            HttpClient httpClient = new HttpClient(messageHandler);

            return new HttpState(fileSystem, preferences, httpClient);
        }
    }
}
