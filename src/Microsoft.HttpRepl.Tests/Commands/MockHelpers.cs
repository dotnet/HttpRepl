using System;
using Microsoft.Repl;
using Microsoft.Repl.Commanding;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Input;
using Microsoft.Repl.Suggestions;
using Moq;

namespace Microsoft.HttpRepl.Tests.Commands
{
    internal class MockHelpers
    {
        internal static IShellState GetMockedShellState(Action<string> writeLineCallback = null, Action<string> errorMessageCallback = null)
        {
            Mock<IConsoleManager> mockedConsoleManager = new Mock<IConsoleManager>();
            if (errorMessageCallback != null)
            {
                Mock<IWritable> mockedErrorWritable = new Mock<IWritable>();
                mockedErrorWritable.Setup(x => x.WriteLine(It.IsAny<string>())).Callback(errorMessageCallback);
                mockedConsoleManager.Setup(x => x.Error).Returns(mockedErrorWritable.Object);
            }
            if (writeLineCallback != null)
            {
                mockedConsoleManager.Setup(x => x.WriteLine(It.IsAny<string>())).Callback(writeLineCallback);
            }

            Mock<IShellState> mockedShellState = new Mock<IShellState>();
            Mock<IInputManager> mockedInputManager = new Mock<IInputManager>();
            Mock<ICommandHistory> mockedCommandHistory = new Mock<ICommandHistory>();
            Mock<ISuggestionManager> mockedSuggestionManager = new Mock<ISuggestionManager>();
            mockedShellState.Setup(x => x.InputManager).Returns(mockedInputManager.Object);
            mockedShellState.Setup(x => x.ConsoleManager).Returns(mockedConsoleManager.Object);
            mockedShellState.Setup(x => x.CommandHistory).Returns(mockedCommandHistory.Object);
            mockedShellState.Setup(x => x.SuggestionManager).Returns(mockedSuggestionManager.Object);

            return mockedShellState.Object;
        }
    }
}
