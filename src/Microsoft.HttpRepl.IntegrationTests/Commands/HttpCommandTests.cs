using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.FileSystem;
using Microsoft.HttpRepl.IntegrationTests.Mocks;
using Microsoft.HttpRepl.Preferences;
using Microsoft.HttpRepl.UserProfile;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Parsing;
using Xunit;

namespace Microsoft.HttpRepl.IntegrationTests.Commands
{
    public abstract class HttpCommandTests<T> where T : BaseHttpCommand
    {
        public IFileSystem FileSystem { get; } = new MockedFileSystem();
        private readonly IPreferencesProvider _preferencesProvider;
        private readonly T _command;

        public HttpCommandTests(T command)
        {
            _command = command;
            _preferencesProvider = new PreferencesProvider(FileSystem, new UserProfileDirectoryProvider());
        }

        protected async Task VerifyErrorMessage(string commandText, string baseAddress, string path, string expectedErrorMessage)
        {
            HttpState httpState = GetHttpState(baseAddress, path);

            expectedErrorMessage = expectedErrorMessage.SetColor(httpState.ErrorColor);

            MockedShellState shellState = new MockedShellState();

            ICoreParseResult parseResult = CoreParseResultHelper.Create(commandText);

            await _command.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Equal(expectedErrorMessage, shellState.ErrorMessage);
        }

        protected async Task VerifyResponse(string commandText, string baseAddress, string path, int expectedResponseLines, string expectedResponseContent)
        {
            HttpState httpState = GetHttpState(baseAddress, path);

            MockedShellState shellState = new MockedShellState();

            ICoreParseResult parseResult = CoreParseResultHelper.Create(commandText);

            await _command.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Equal(expectedResponseLines, shellState.Output.Count);
            Assert.Equal(expectedResponseContent, shellState.Output[expectedResponseLines - 1]);
        }

        protected async Task VerifyHeaders(string commandText, string baseAddress, string path, int expectedResponseLines, string expectedHeader)
        {
            MockedShellState shellState = new MockedShellState();

            HttpState httpState = GetHttpState(baseAddress, path);

            ICoreParseResult parseResult = CoreParseResultHelper.Create(commandText);

            await _command.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Equal(expectedResponseLines, shellState.Output.Count);
            Assert.Equal(expectedHeader, shellState.Output[expectedResponseLines - 2]);
        }

        private HttpState GetHttpState(string baseAddress, string path)
        {
            HttpState httpState = new HttpState(FileSystem, _preferencesProvider);

            if (!string.IsNullOrWhiteSpace(baseAddress))
            {
                httpState.BaseAddress = new Uri(baseAddress);
            }
            if (!string.IsNullOrWhiteSpace(path))
            {
                httpState.BaseAddress = new Uri(baseAddress);

                if (path != null)
                {
                    string[] pathParts = path.Split('/');

                    foreach (string pathPart in pathParts)
                    {
                        httpState.PathSections.Push(pathPart);
                    }
                }
            }

            return httpState;
        }
    }
}
