using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.IntegrationTests.Mocks;
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
            HttpClient httpClient = new HttpClient();
            HttpState httpState = new HttpState(httpClient);

            return _command.CanHandle(shellState, httpState, parseResult);
        }

        protected string GetHelpDetails(string parseResultSections)
        {
            ICoreParseResult parseResult = CoreParseResultHelper.Create(parseResultSections);
            MockedShellState shellState = new MockedShellState();
            HttpClient httpClient = new HttpClient();
            HttpState httpState = new HttpState(httpClient);

            return _command.GetHelpDetails(shellState, httpState, parseResult);
        }

        protected string GetHelpSummary()
        {
            MockedShellState shellState = new MockedShellState();
            HttpClient httpClient = new HttpClient();
            HttpState httpState = new HttpState(httpClient);

            return _command.GetHelpSummary(shellState, httpState);
        }

        protected async Task ExecuteAsyncWithInvalidParseResultSections(string parseResultSections, IShellState shellState, string baseAddress = null)
        {
            ICoreParseResult parseResult = CoreParseResultHelper.Create(parseResultSections);
            HttpClient httpClient = new HttpClient();
            HttpState httpState = new HttpState(httpClient);
            httpState.BaseAddress = null;

            await _command.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);
        }

        protected IEnumerable<string> GetSuggestions(string parseResultSections)
        {
            ICoreParseResult parseResult = CoreParseResultHelper.Create(parseResultSections);
            MockedShellState shellState = new MockedShellState();
            HttpClient httpClient = new HttpClient();
            HttpState httpState = new HttpState(httpClient);
            return _command.Suggest(shellState, httpState, parseResult);
        }

        protected void VerifyErrorMessageWasWrittenToConsoleManagerError(IShellState shellState)
        {
            Mock<IWritable> error = Mock.Get(shellState.ConsoleManager.Error);

            error.Verify(s => s.WriteLine(It.IsAny<string>()), Times.Once);
        }
    }
}
