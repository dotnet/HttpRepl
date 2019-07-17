// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using Microsoft.HttpRepl.Fakes;
using Microsoft.HttpRepl.FileSystem;
using Microsoft.HttpRepl.Preferences;
using Microsoft.HttpRepl.Tests.Preferences;
using Microsoft.HttpRepl.UserProfile;
using Microsoft.Repl;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Parsing;
using Moq;

namespace Microsoft.HttpRepl.Tests.Commands
{
    public class CommandTestsBase
    {
        protected static void ArrangeInputs(string parseResultSections,
            out MockedShellState shellState,
            out HttpState httpState,
            out ICoreParseResult parseResult,
            int caretPosition = -1,
            string responseContent = "")
        {
            parseResult = CoreParseResultHelper.Create(parseResultSections, caretPosition);
            shellState = new MockedShellState();
            httpState = GetHttpState(responseContent);
        }

        protected static void VerifyErrorMessageWasWrittenToConsoleManagerError(IShellState shellState)
        {
            Mock<IWritable> error = Mock.Get(shellState.ConsoleManager.Error);

            error.Verify(s => s.WriteLine(It.IsAny<string>()), Times.Once);
        }


        protected static HttpState GetHttpState(string content = null)
        {
            return GetHttpState(content, out _, out _);
        }

        protected static HttpState GetHttpState(string content, out IFileSystem fileSystem, out IPreferences preferences)
        {
            fileSystem = new MockedFileSystem();
            preferences = new UserFolderPreferences(fileSystem, new UserProfileDirectoryProvider(), TestDefaultPreferences.GetDefaultPreferences());

            HttpResponseMessage responseMessage = new HttpResponseMessage();
            responseMessage.Content = new MockHttpContent(content);
            MockHttpMessageHandler messageHandler = new MockHttpMessageHandler(responseMessage);
            HttpClient httpClient = new HttpClient(messageHandler);

            return new HttpState(fileSystem, preferences, httpClient);
        }

        protected ICoreParseResult CreateCoreParseResult(string commandText)
        {
            return CoreParseResultHelper.Create(commandText);
        }
    }
}
