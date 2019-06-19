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
                0,
                previousCommand,
                null,
                out CancellationTokenSource cancellationTokenSource);

            // Verify the input buffer is empty before the UpArrow key press event
            Assert.Equal(string.Empty, shell.ShellState.InputManager.GetCurrentBuffer());

            shell.RunAsync(cancellationTokenSource.Token);

            // Verify the input buffer has previous command after the UpArrow key press event
            Assert.Equal(previousCommand, shell.ShellState.InputManager.GetCurrentBuffer());
        }

        [Fact]
        public void RunAsync_WithDownArrowKeyPress_UpdatesCurrentBufferWithNextCommand()
        {
            string nextCommand = "get";
            Shell shell = CreateShell(ConsoleKey.DownArrow,
                0,
                null,
                nextCommand,
                out CancellationTokenSource cancellationTokenSource);

            // Verify the input buffer is empty before the DownArrow key press event
            Assert.Equal(string.Empty, shell.ShellState.InputManager.GetCurrentBuffer());

            shell.RunAsync(cancellationTokenSource.Token);

            // Verify the input buffer has next command after the DownArrow key press event
            Assert.Equal(nextCommand, shell.ShellState.InputManager.GetCurrentBuffer());
        }

        [Fact]
        public void RunAsync_WithDeleteKeyPress_DeletesCurrentCharacterInTheInputBuffer()
        {
            Shell shell = CreateShell(ConsoleKey.Delete,
                2,
                null,
                null,
                out CancellationTokenSource cancellationTokenSource);

            string inputBufferTextBeforeKeyPress = "get";
            string inputBufferTextAfterKeyPress = "ge";

            IShellState shellState = shell.ShellState;
            shellState.InputManager.SetInput(shellState, inputBufferTextBeforeKeyPress);

            // Verify the input buffer contents before delete key press
            Assert.Equal(inputBufferTextBeforeKeyPress, shell.ShellState.InputManager.GetCurrentBuffer());

            shell.RunAsync(cancellationTokenSource.Token);

            // Verify the input buffer contents after delete key press
            Assert.Equal(inputBufferTextAfterKeyPress, shell.ShellState.InputManager.GetCurrentBuffer());
        }

        [Fact]
        public void RunAsync_WithBackspaceKeyPress_DeletesPreviousCharacterInTheInputBuffer()
        {
            Shell shell = CreateShell(ConsoleKey.Backspace,
                2,
                null,
                null,
                out CancellationTokenSource cancellationTokenSource);

            string inputBufferTextBeforeKeyPress = "get";
            string inputBufferTextAfterKeyPress = "gt";

            IShellState shellState = shell.ShellState;
            shellState.InputManager.SetInput(shellState, inputBufferTextBeforeKeyPress);

            // Verify the input buffer contents before backspace key press
            Assert.Equal(inputBufferTextBeforeKeyPress, shell.ShellState.InputManager.GetCurrentBuffer());

            shell.RunAsync(cancellationTokenSource.Token);

            // Verify the input buffer contents after backspace key press
            Assert.Equal(inputBufferTextAfterKeyPress, shell.ShellState.InputManager.GetCurrentBuffer());
        }

        [Fact]
        public void RunAsync_WithEscapeKeyPress_UpdatesInputBufferWithEmptyString()
        {
            Shell shell = CreateShell(ConsoleKey.Escape,
                0,
                null,
                null,
                out CancellationTokenSource cancellationTokenSource);

            string inputBufferTextBeforeKeyPress = "get";
            string inputBufferTextAfterKeyPress = string.Empty;

            IShellState shellState = shell.ShellState;
            shellState.InputManager.SetInput(shellState, inputBufferTextBeforeKeyPress);

            // Verify the input buffer contents before escape key press
            Assert.Equal(inputBufferTextBeforeKeyPress, shell.ShellState.InputManager.GetCurrentBuffer());

            shell.RunAsync(cancellationTokenSource.Token);

            // Verify the input buffer contents after escape key press
            Assert.Equal(inputBufferTextAfterKeyPress, shell.ShellState.InputManager.GetCurrentBuffer());
        }

        [Fact]
        public void RunAsync_WithInsertKeyPress_FlipsIsOverwriteModeInInputManager()
        {
            Shell shell = CreateShell(ConsoleKey.Insert,
                0,
                null,
                null,
                out CancellationTokenSource cancellationTokenSource);

            // Verify IsOverwriteMode flag in input manager is set to true
            Assert.False(shell.ShellState.InputManager.IsOverwriteMode);

            shell.RunAsync(cancellationTokenSource.Token);

            // Verify IsOverwriteMode flag in input manager is set to false
            Assert.True(shell.ShellState.InputManager.IsOverwriteMode);
        }

        [Fact]
        public void RunAsync_WithUnhandledKeyPress_DoesNothing()
        {
            Shell shell = CreateShell(ConsoleKey.F1,
                0,
                null,
                null,
                out CancellationTokenSource cancellationTokenSource);

            string inputBufferText = "get";

            IShellState shellState = shell.ShellState;
            shellState.InputManager.SetInput(shellState, inputBufferText);

            // Verify the input buffer contents before F1 key press
            Assert.Equal(inputBufferText, shell.ShellState.InputManager.GetCurrentBuffer());

            shell.RunAsync(cancellationTokenSource.Token);

            // Verify the input buffer contents after F1 key press
            Assert.Equal(inputBufferText, shell.ShellState.InputManager.GetCurrentBuffer());
        }

        private Shell CreateShell(ConsoleKey consoleKey, int caretPosition, string previousCommand, string nextCommand, out CancellationTokenSource cancellationTokenSource)
        {
            HttpState httpState = new HttpState();
            var defaultCommandDispatcher = DefaultCommandDispatcher.Create(httpState.GetPrompt, httpState);

            Mock<IConsoleManager> mockConsoleManager = new Mock<IConsoleManager>();
            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo('\0', consoleKey, false, false, false);
            mockConsoleManager.SetupSequence(s => s.ReadKey(cancellationToken))
                        .Returns(consoleKeyInfo);
            mockConsoleManager.Setup(s => s.CaretPosition)
                        .Returns(caretPosition);

            Mock<ICommandHistory> mockCommandHistory = new Mock<ICommandHistory>();
            mockCommandHistory.Setup(s => s.GetPreviousCommand())
                        .Returns(previousCommand);
            mockCommandHistory.Setup(s => s.GetNextCommand())
            .Returns(nextCommand);

            ShellState shellState = new ShellState(defaultCommandDispatcher,
                consoleManager: mockConsoleManager.Object,
                commandHistory: mockCommandHistory.Object);

            return new Shell(shellState);
        }
    }
}
