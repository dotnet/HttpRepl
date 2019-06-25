using System.Collections.Generic;
using Microsoft.Repl;
using Microsoft.Repl.Commanding;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Input;
using Microsoft.Repl.Suggestions;
using Moq;

namespace Microsoft.HttpRepl.IntegrationTests.Mocks
{
    internal class MockedShellState : IShellState
    {
        public MockedShellState()
        {
            Mock<IConsoleManager> mockedConsoleManager = new Mock<IConsoleManager>();
            Mock<IWritable> mockedErrorWritable = new Mock<IWritable>();
            mockedErrorWritable.Setup(x => x.WriteLine(It.IsAny<string>())).Callback((string s) => ErrorMessage = s);
            mockedConsoleManager.Setup(x => x.Error).Returns(mockedErrorWritable.Object);
            mockedConsoleManager.Setup(x => x.WriteLine(It.IsAny<string>())).Callback((string s) => Output.Add(s));

            ConsoleManager = mockedConsoleManager.Object;
        }

        public IInputManager InputManager { get; } = new Mock<IInputManager>().Object;

        public ICommandHistory CommandHistory { get; } = new Mock<ICommandHistory>().Object;

        public IConsoleManager ConsoleManager { get; }

        public ICommandDispatcher CommandDispatcher { get; } = new Mock<ICommandDispatcher>().Object;

        public ISuggestionManager SuggestionManager { get; } = new Mock<ISuggestionManager>().Object;

        public bool IsExiting { get; set; }

        public string ErrorMessage { get; private set; }

        public List<string> Output { get; } = new List<string>();
    }
}
