using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.IntegrationTests.Mocks;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Parsing;
using Xunit;

namespace Microsoft.HttpRepl.IntegrationTests.Commands
{
    public abstract class BaseHttpCommandTests
    {
        protected async Task VerifyErrorMessage(BaseHttpCommand command, string commandText, string baseAddress, string path, string expectedErrorMessage)
        {
            HttpState httpState = GetHttpState(baseAddress, path);

            expectedErrorMessage = expectedErrorMessage.SetColor(httpState.ErrorColor);

            MockedShellState shellState = new MockedShellState();

            ICoreParseResult parseResult = CoreParseResultHelper.Create(commandText);

            await command.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Equal(expectedErrorMessage, shellState.ErrorMessage);
        }

        protected async Task VerifyResponse(BaseHttpCommand command, string commandText, string baseAddress, string path, int expectedResponseLines, string expectedResponseContent)
        {
            HttpState httpState = GetHttpState(baseAddress, path);

            MockedShellState shellState = new MockedShellState();

            ICoreParseResult parseResult = CoreParseResultHelper.Create(commandText);

            await command.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Equal(expectedResponseLines, shellState.Output.Count);
            Assert.Equal(expectedResponseContent, shellState.Output[expectedResponseLines - 1]);
        }

        protected async Task VerifyHeaders(BaseHttpCommand command, string commandText, string baseAddress, string path, int expectedResponseLines, string expectedHeader)
        {
            MockedShellState shellState = new MockedShellState();

            HttpState httpState = GetHttpState(baseAddress, path);

            ICoreParseResult parseResult = CoreParseResultHelper.Create(commandText);

            await command.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Equal(expectedResponseLines, shellState.Output.Count);
            Assert.Equal(expectedHeader, shellState.Output[expectedResponseLines - 2]);
        }

        private static HttpState GetHttpState(string baseAddress, string path)
        {
            HttpState httpState;

            if (string.IsNullOrWhiteSpace(baseAddress))
            {
                httpState = new HttpState();
            }
            else if (string.IsNullOrWhiteSpace(path))
            {
                httpState = HttpStateHelpers.Create(baseAddress);
            }
            else
            {
                httpState = HttpStateHelpers.Create(baseAddress, path);
            }

            return httpState;
        }
    }
}
