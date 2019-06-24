using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Commands;
using Microsoft.Repl;
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
            string actualErrorMessage = null;

            IShellState shellState = MockHelpers.GetMockedShellState(errorMessageCallback: (s) => actualErrorMessage = s);

            ICoreParseResult parseResult = CoreParseResultHelper.Create(commandText);

            await command.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Equal(expectedErrorMessage, actualErrorMessage);
        }

        protected async Task VerifyResponse(BaseHttpCommand command, string commandText, string baseAddress, string path, int expectedResponseLines, string expectedResponseContent)
        {
            HttpState httpState = GetHttpState(baseAddress, path);

            List<string> actualResponseContent = new List<string>();

            IShellState shellState = MockHelpers.GetMockedShellState(writeLineCallback: (s) => actualResponseContent.Add(s));

            ICoreParseResult parseResult = CoreParseResultHelper.Create(commandText);

            await command.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Equal(expectedResponseLines, actualResponseContent.Count);
            Assert.Equal(expectedResponseContent, actualResponseContent[expectedResponseLines - 1]);
        }

        protected async Task VerifyHeaders(BaseHttpCommand command, string commandText, string baseAddress, string path, int expectedResponseLines, string expectedHeader)
        {
            List<string> actual = new List<string>();

            IShellState shellState = MockHelpers.GetMockedShellState(writeLineCallback: (s) => actual.Add(s));

            HttpState httpState = GetHttpState(baseAddress, path);

            ICoreParseResult parseResult = CoreParseResultHelper.Create(commandText);

            await command.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Equal(expectedResponseLines, actual.Count);
            Assert.Equal(expectedHeader, actual[expectedResponseLines - 2]);
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
