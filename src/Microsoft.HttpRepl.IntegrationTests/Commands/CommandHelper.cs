// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.FileSystem;
using Microsoft.HttpRepl.IntegrationTests.Mocks;
using Microsoft.HttpRepl.IntegrationTests.Preferences;
using Microsoft.HttpRepl.Preferences;
using Microsoft.HttpRepl.UserProfile;
using Microsoft.Repl;
using Microsoft.Repl.Commanding;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Parsing;
using Moq;

namespace Microsoft.HttpRepl.IntegrationTests.Commands
{
    public class CommandHelper<T>
        where T : ICommand<HttpState, ICoreParseResult>
    {
        private readonly T _command;

        public CommandHelper(T command)
        {
            _command = command;
        }

        protected bool? CanHandle(string parseResultSections)
        {
            ICoreParseResult parseResult = CoreParseResultHelper.Create(parseResultSections);
            MockedShellState shellState = new MockedShellState();
            HttpState httpState = GetHttpState(string.Empty);

            return _command.CanHandle(shellState, httpState, parseResult);
        }

        protected string GetHelpDetails(string parseResultSections)
        {
            ICoreParseResult parseResult = CoreParseResultHelper.Create(parseResultSections);
            MockedShellState shellState = new MockedShellState();
            HttpState httpState = GetHttpState(string.Empty);

            return _command.GetHelpDetails(shellState, httpState, parseResult);
        }

        protected string GetHelpSummary()
        {
            MockedShellState shellState = new MockedShellState();
            HttpState httpState = GetHttpState(string.Empty);

            return _command.GetHelpSummary(shellState, httpState);
        }

        protected async Task ExecuteAsyncWithInvalidParseResultSections(string parseResultSections, IShellState shellState)
        {
            ICoreParseResult parseResult = CoreParseResultHelper.Create(parseResultSections);
            HttpState httpState = GetHttpState(string.Empty);

            await _command.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);
        }

        protected IEnumerable<string> GetSuggestions(string parseResultSections)
        {
            ICoreParseResult parseResult = CoreParseResultHelper.Create(parseResultSections);
            MockedShellState shellState = new MockedShellState();
            HttpState httpState = GetHttpState(string.Empty);
            return _command.Suggest(shellState, httpState, parseResult);
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
