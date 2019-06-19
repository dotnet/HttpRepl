using System;
using System.Threading;
using Microsoft.HttpRepl;
using Microsoft.Repl.Commanding;
using Microsoft.Repl.ConsoleHandling;
using Moq;
using Xunit;

namespace Microsoft.Repl.Tests
{
    public class ShellTests
    {
        [Fact]
        public void RunAsync_WithUpArrowKeyPress_UpdatesCurrentBufferWithPreviousCommand()
        {
            string previousCommand = "set base \"https://localhost:44366/\"";
            Shell shell = CreateShell(ConsoleKey.UpArrow,
                previousCommand,
                out CancellationTokenSource cancellationTokenSource);

            // Verify the input buffer is empty before the UpArrow key press event
            Assert.Equal(string.Empty, shell.ShellState.InputManager.GetCurrentBuffer());

            shell.RunAsync(cancellationTokenSource.Token);

            // Verify the input buffer has previous command after the UpArrow key press event
            Assert.Equal(previousCommand, shell.ShellState.InputManager.GetCurrentBuffer());
        }

        [Fact]
        public void RunAsync_WithEndKeyPress_UpdatesConsoleManagerCaretPositionToEndOfInput()
        {
            Shell shell = CreateShell(ConsoleKey.End,
                null,
                out CancellationTokenSource cancellationTokenSource);

            IShellState shellState = shell.ShellState;
            shellState.InputManager.SetInput(shellState, "get");

            shell.RunAsync(cancellationTokenSource.Token);

            Assert.Equal(35, shell.ShellState.ConsoleManager.CaretPosition);
        }

        [Fact]
        public void RunAsync_WithHomeKeyPress_UpdatesConsoleManagerCaretPositionToZero()
        {
            Shell shell = CreateShell(ConsoleKey.Home,
                null,
                out CancellationTokenSource cancellationTokenSource);

            shell.RunAsync(cancellationTokenSource.Token);

            Assert.Equal(0, shell.ShellState.ConsoleManager.CaretPosition);
        }

        private Shell CreateShell(ConsoleKey consoleKey, string previousCommand, out CancellationTokenSource cancellationTokenSource)
        {
            HttpState httpState = new HttpState();
            var defaultCommandDispatcher = DefaultCommandDispatcher.Create(httpState.GetPrompt, httpState);

            Mock<IConsoleManager> mockConsoleManager = new Mock<IConsoleManager>();
            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo('\0', consoleKey, false, false, false);
            mockConsoleManager.SetupSequence(s => s.ReadKey(cancellationToken))
                        .Returns(consoleKeyInfo);

            Mock<ICommandHistory> mockCommandHistory = new Mock<ICommandHistory>();
            mockCommandHistory.Setup(s => s.GetPreviousCommand())
                        .Returns(previousCommand);

            ShellState shellState = new ShellState(defaultCommandDispatcher,
                consoleManager: mockConsoleManager.Object,
                commandHistory: mockCommandHistory.Object);

            return new Shell(shellState);
        }
    }
}
