using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.IntegrationTests.Mocks;
using Microsoft.Repl;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Parsing;
using Moq;
using Xunit;

namespace Microsoft.HttpRepl.IntegrationTests.Commands
{
    public class SetSwaggerCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_WithExactlyTwoParseResult_ReturnsEmptyTask()
        {
            MockedShellState shellState = new MockedShellState();
            ICoreParseResult parseResult = CoreParseResultHelper.Create("section1 sections2");
            string baseAddress = "http://localhost:5050"; 
            HttpState httpState = HttpStateHelpers.Create(baseAddress);

            SetSwaggerCommand setSwaggerCommand = new SetSwaggerCommand();

            await setSwaggerCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Null(httpState.SwaggerStructure);
        }

        [Fact]
        public async Task ExecuteAsync_WithExactlyOneParseResult_WritesToConsoleManagerError()
        {
            MockedShellState shellState = new MockedShellState();
            ICoreParseResult parseResult = CoreParseResultHelper.Create("section1");
            string baseAddress = "http://localhost:5050";
            HttpState httpState = HttpStateHelpers.Create(baseAddress);

            SetSwaggerCommand setSwaggerCommand = new SetSwaggerCommand();

            await setSwaggerCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            VerifyErrorMessageWasWrittenToConsoleManagerError(shellState);
        }

        [Fact]
        public async Task ExecuteAsync_WithEmptyThirdParseResultSection_WritesToConsoleManagerError()
        {
            MockedShellState shellState = new MockedShellState();
            ICoreParseResult parseResult = CoreParseResultHelper.Create("section1 section1  ");
            string baseAddress = "http://localhost:5050";
            HttpState httpState = HttpStateHelpers.Create(baseAddress);

            SetSwaggerCommand setSwaggerCommand = new SetSwaggerCommand();

            await setSwaggerCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            VerifyErrorMessageWasWrittenToConsoleManagerError(shellState);
        }

        [Fact]
        public async Task ExecuteAsync_WhenThirdSectionIsNotAValidUri_WritesToConsoleManagerError()
        {
            MockedShellState shellState = new MockedShellState();
            ICoreParseResult parseResult = CoreParseResultHelper.Create("section1 section1 section3");
            string baseAddress = "http://localhost:5050";
            HttpState httpState = HttpStateHelpers.Create(baseAddress);

            SetSwaggerCommand setSwaggerCommand = new SetSwaggerCommand();

            await setSwaggerCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            VerifyErrorMessageWasWrittenToConsoleManagerError(shellState);
        }

        private void VerifyErrorMessageWasWrittenToConsoleManagerError(IShellState shellState)
        {
            Mock<IWritable> error = Mock.Get(shellState.ConsoleManager.Error);

            error.Verify(s => s.WriteLine(It.IsAny<string>()), Times.Once);
        }
    }
}
