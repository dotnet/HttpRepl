using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Repl.Commanding;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Tests.Mocks;
using Moq;
using Xunit;

namespace Microsoft.Repl.Tests
{
    public class ShellTests
    {
        [Fact]
        public async Task RunAsync_WithUpArrowKeyPress_UpdatesCurrentBufferWithPreviousCommand()
        {
            string previousCommand = "set base \"https://localhost:44366/\"";
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo('\0', ConsoleKey.UpArrow, false, false, false);
            Shell shell = CreateShell(consoleKeyInfo,
                caretPosition: 0,
                previousCommand: previousCommand,
                nextCommand: null,
                out CancellationTokenSource cancellationTokenSource);

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            // Verify the input buffer has previous command after the UpArrow key press event
            Assert.Equal(previousCommand, shell.ShellState.InputManager.GetCurrentBuffer());
        }

        [Fact]
        public async Task RunAsync_WithUpArrowKeyPress_VerifyInputBufferContentsBeforeAndAfterKeyPressEvent()
        {
            string previousCommand = "set base \"https://localhost:44366/\"";
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo('\0', ConsoleKey.UpArrow, false, false, false);
            Shell shell = CreateShell(consoleKeyInfo,
                caretPosition: 0,
                previousCommand: previousCommand,
                nextCommand: null,
                out CancellationTokenSource cancellationTokenSource);

            // Verify the input buffer is empty before the UpArrow key press event
            Assert.Equal(string.Empty, shell.ShellState.InputManager.GetCurrentBuffer());

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            // Verify the input buffer has previous command after the UpArrow key press event
            Assert.Equal(previousCommand, shell.ShellState.InputManager.GetCurrentBuffer());
        }

        [Fact]
        public async Task RunAsync_WithDownArrowKeyPress_UpdatesCurrentBufferWithNextCommand()
        {
            string nextCommand = "get";
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false);
            Shell shell = CreateShell(consoleKeyInfo,
                caretPosition: 0,
                previousCommand: null,
                nextCommand: nextCommand,
                out CancellationTokenSource cancellationTokenSource);

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            // Verify the input buffer has next command after the DownArrow key press event
            Assert.Equal(nextCommand, shell.ShellState.InputManager.GetCurrentBuffer());
        }

        [Fact]
        public async Task RunAsync_WithDeleteKeyPress_DeletesCurrentCharacterInTheInputBuffer()
        {
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo('\0', ConsoleKey.Delete, false, false, false);
            Shell shell = CreateShell(consoleKeyInfo,
                caretPosition: 2,
                previousCommand: null,
                nextCommand: null,
                out CancellationTokenSource cancellationTokenSource);

            string inputBufferTextBeforeKeyPress = "get";
            string inputBufferTextAfterKeyPress = "ge";

            IShellState shellState = shell.ShellState;
            shellState.InputManager.SetInput(shellState, inputBufferTextBeforeKeyPress);

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            // Verify the input buffer contents after Delete key press event
            Assert.Equal(inputBufferTextAfterKeyPress, shell.ShellState.InputManager.GetCurrentBuffer());
        }

        [Fact]
        public async Task RunAsync_WithBackspaceKeyPress_DeletesPreviousCharacterInTheInputBuffer()
        {
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo('\0', ConsoleKey.Backspace, false, false, false);
            Shell shell = CreateShell(consoleKeyInfo,
                caretPosition: 2,
                previousCommand: null,
                nextCommand: null,
                out CancellationTokenSource cancellationTokenSource);

            string inputBufferTextBeforeKeyPress = "get";
            string inputBufferTextAfterKeyPress = "gt";

            IShellState shellState = shell.ShellState;
            shellState.InputManager.SetInput(shellState, inputBufferTextBeforeKeyPress);

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            // Verify the input buffer contents after Backspace key press event
            Assert.Equal(inputBufferTextAfterKeyPress, shell.ShellState.InputManager.GetCurrentBuffer());
        }

        [Fact]
        public async Task RunAsync_WithEscapeKeyPress_UpdatesInputBufferWithEmptyString()
        {
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo('\0', ConsoleKey.Escape, false, false, false);
            Shell shell = CreateShell(consoleKeyInfo,
                caretPosition: 0,
                previousCommand: null,
                nextCommand: null,
                out CancellationTokenSource cancellationTokenSource);

            string inputBufferTextBeforeKeyPress = "get";
            string inputBufferTextAfterKeyPress = string.Empty;

            IShellState shellState = shell.ShellState;
            shellState.InputManager.SetInput(shellState, inputBufferTextBeforeKeyPress);

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            // Verify the input buffer contents after Escape key press event
            Assert.Equal(inputBufferTextAfterKeyPress, shell.ShellState.InputManager.GetCurrentBuffer());
        }

        [Fact]
        public async Task RunAsync_WithCtrlUKeyPress_UpdatesInputBufferWithEmptyString()
        {
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo('\0', ConsoleKey.U, false, false, true);
            Shell shell = CreateShell(consoleKeyInfo,
                caretPosition: 0,
                previousCommand: null,
                nextCommand: null,
                out CancellationTokenSource cancellationTokenSource);

            string inputBufferTextBeforeKeyPress = "get";
            string inputBufferTextAfterKeyPress = string.Empty;

            IShellState shellState = shell.ShellState;
            shellState.InputManager.SetInput(shellState, inputBufferTextBeforeKeyPress);

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            // Verify the input buffer contents after Ctrl + U key press event
            Assert.Equal(inputBufferTextAfterKeyPress, shell.ShellState.InputManager.GetCurrentBuffer());
        }

        [Fact]
        public async Task RunAsync_WithInsertKeyPress_FlipsIsOverwriteModeInInputManager()
        {
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo('\0', ConsoleKey.Insert, false, false, false);
            Shell shell = CreateShell(consoleKeyInfo,
                caretPosition: 0,
                previousCommand: null,
                nextCommand: null,
                out CancellationTokenSource cancellationTokenSource);

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            // Verify IsOverwriteMode flag in input manager is set to false after Insert key press event
            Assert.True(shell.ShellState.InputManager.IsOverwriteMode);
        }

        [Fact]
        public async Task RunAsync_WithUnhandledKeyPress_DoesNothing()
        {
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo('\0', ConsoleKey.F1, false, false, false);
            Shell shell = CreateShell(consoleKeyInfo,
                caretPosition: 0,
                previousCommand: null,
                nextCommand: null,
                out CancellationTokenSource cancellationTokenSource);

            string inputBufferText = "get";

            IShellState shellState = shell.ShellState;
            shellState.InputManager.SetInput(shellState, inputBufferText);

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            // Verify the input buffer contents after F1 key press event
            Assert.Equal(inputBufferText, shell.ShellState.InputManager.GetCurrentBuffer());
        }

        [Fact]
        public async Task RunAsync_WithTabKeyPress_UpdatesInputBufferWithFirstEntryFromSuggestionList()
        {
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo('\0', ConsoleKey.Tab, false, false, false);
            Shell shell = CreateShell(consoleKeyInfo,
                 caretPosition: 0,
                 previousCommand: null,
                 nextCommand: null,
                 out CancellationTokenSource cancellationTokenSource);

            string inputBufferTextBeforeKeyPress = "g";
            string inputBufferTextAfterKeyPress = "get";

            IShellState shellState = shell.ShellState;
            shellState.InputManager.SetInput(shellState, inputBufferTextBeforeKeyPress);

            DefaultCommandDispatcher<object> defaultCommandDispatcher = shellState.CommandDispatcher as DefaultCommandDispatcher<object>;
            string commandName = "get";
            defaultCommandDispatcher.AddCommand(new MockCommand(commandName));

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            // Verify the input buffer contents after tab key press event
            Assert.Equal(inputBufferTextAfterKeyPress, shell.ShellState.InputManager.GetCurrentBuffer());
        }

        [Fact]
        public async Task RunAsync_WithShiftTabKeyPress_UpdatesInputBufferWithFirstEntryFromSuggestionList()
        {
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo('\0', ConsoleKey.Tab, true, false, false);
            Shell shell = CreateShell(consoleKeyInfo,
                 caretPosition: 0,
                 previousCommand: null,
                 nextCommand: null,
                 out CancellationTokenSource cancellationTokenSource);

            string inputBufferTextBeforeKeyPress = "g";
            string inputBufferTextAfterKeyPress = "get";

            IShellState shellState = shell.ShellState;
            shellState.InputManager.SetInput(shellState, inputBufferTextBeforeKeyPress);

            DefaultCommandDispatcher<object> defaultCommandDispatcher = shellState.CommandDispatcher as DefaultCommandDispatcher<object>;
            string commandName = "get";
            defaultCommandDispatcher.AddCommand(new MockCommand(commandName));

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            // Verify the input buffer contents after Shift + Tab key press event
            Assert.Equal(inputBufferTextAfterKeyPress, shell.ShellState.InputManager.GetCurrentBuffer());
        }

        [Fact]
        public async Task RunAsync_WithTabKeyPressAndNoSuggestions_DoesNothing()
        {
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo('\0', ConsoleKey.Tab, false, false, false);
            Shell shell = CreateShell(consoleKeyInfo,
                 caretPosition: 0,
                 previousCommand: null,
                 nextCommand: null,
                 out CancellationTokenSource cancellationTokenSource);

            string inputBufferTextBeforeKeyPress = "z";

            IShellState shellState = shell.ShellState;
            shellState.InputManager.SetInput(shellState, inputBufferTextBeforeKeyPress);

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            // Verify the input buffer contents after tab key press event
            Assert.Equal(inputBufferTextBeforeKeyPress, shell.ShellState.InputManager.GetCurrentBuffer());
        }

        [Fact]
        public async Task RunAsync_WithShiftTabKeyPressAndNoSuggestions_DoesNothing()
        {
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo('\0', ConsoleKey.Tab, true, false, false);
            Shell shell = CreateShell(consoleKeyInfo,
                 caretPosition: 0,
                 previousCommand: null,
                 nextCommand: null,
                 out CancellationTokenSource cancellationTokenSource);

            string inputBufferTextBeforeKeyPress = "z";

            IShellState shellState = shell.ShellState;
            shellState.InputManager.SetInput(shellState, inputBufferTextBeforeKeyPress);

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            // Verify the input buffer contents after Shift + Tab key press event
            Assert.Equal(inputBufferTextBeforeKeyPress, shell.ShellState.InputManager.GetCurrentBuffer());
        }

        [Fact]
        public async Task RunAsync_WithEnterKeyPress_UpdatesInputBufferWithEmptyString()
        {
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo('\0', ConsoleKey.Enter, false, false, false);
            Shell shell = CreateShell(consoleKeyInfo,
                caretPosition: 0,
                previousCommand: null,
                nextCommand: null,
                out CancellationTokenSource cancellationTokenSource);

            string inputBufferContents = "set base \"https://localhost:44366/\"";

            IShellState shellState = shell.ShellState;
            shellState.InputManager.SetInput(shellState, inputBufferContents);

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            Assert.Equal(string.Empty, shell.ShellState.InputManager.GetCurrentBuffer());
        }

        [Fact]
        public async Task RunAsync_WithLeftArrowKeyPress_VerifyMoveCaretWasCalled()
        {
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo('\0', ConsoleKey.LeftArrow, false, false, false);
            Shell shell = CreateShell(consoleKeyInfo,
                caretPosition: 3,
                previousCommand: null,
                nextCommand: null,
                out CancellationTokenSource cancellationTokenSource);

            string inputBufferContents = "set base \"https://localhost:44366/\"";

            IShellState shellState = shell.ShellState;
            shellState.InputManager.SetInput(shellState, inputBufferContents);

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            VerifyMoveCaretMethodWasCalledOnce(shellState, -1, Times.Once());
        }

        [Fact]
        public async Task RunAsync_WithControlLeftArrowKeyPress_VerifyMoveCaretWasCalled()
        {
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo('\0', ConsoleKey.LeftArrow, false, false, true);
            Shell shell = CreateShell(consoleKeyInfo,
                caretPosition: 7,
                previousCommand: null,
                nextCommand: null,
                out CancellationTokenSource cancellationTokenSource);

            string inputBufferContents = "set base \"https://localhost:44366/\"";

            IShellState shellState = shell.ShellState;
            shellState.InputManager.SetInput(shellState, inputBufferContents);

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            VerifyMoveCaretMethodWasCalledOnce(shellState, -3, Times.Once());
        }

        [Fact]
        public async Task RunAsync_WithRightArrowKeyPress_VerifyMoveCaretWasCalled()
        {
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo('\0', ConsoleKey.RightArrow, false, false, false);
            Shell shell = CreateShell(consoleKeyInfo,
                caretPosition: 3,
                previousCommand: null,
                nextCommand: null,
                out CancellationTokenSource cancellationTokenSource);

            string inputBufferContents = "set base \"https://localhost:44366/\"";

            IShellState shellState = shell.ShellState;
            shellState.InputManager.SetInput(shellState, inputBufferContents);

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            VerifyMoveCaretMethodWasCalledOnce(shellState, 1, Times.Once());
        }

        [Fact]
        public async Task RunAsync_WithControlRightArrowKeyPress_VerifyMoveCaretWasCalled()
        {
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo('\0', ConsoleKey.RightArrow, false, false, true);
            Shell shell = CreateShell(consoleKeyInfo,
                caretPosition: 4,
                previousCommand: null,
                nextCommand: null,
                out CancellationTokenSource cancellationTokenSource);

            string inputBufferContents = "set base \"https://localhost:44366/\"";

            IShellState shellState = shell.ShellState;
            shellState.InputManager.SetInput(shellState, inputBufferContents);

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            VerifyMoveCaretMethodWasCalledOnce(shellState, 5, Times.Once());
        }

        [Fact]
        public async Task RunAsync_WithHomeKeyPress_VerifyMoveCaretWasCalled()
        {
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo('\0', ConsoleKey.Home, false, false, false);
            Shell shell = CreateShell(consoleKeyInfo,
                caretPosition: 3,
                previousCommand: null,
                nextCommand: null,
                out CancellationTokenSource cancellationTokenSource);

            string inputBufferContents = "set base \"https://localhost:44366/\"";

            IShellState shellState = shell.ShellState;
            shellState.InputManager.SetInput(shellState, inputBufferContents);

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            // The first method call is from SetInput(..) and the second one is from End key press event
            VerifyMoveCaretMethodWasCalledOnce(shellState, -3, Times.Exactly(2));
        }

        [Fact]
        public async Task RunAsync_WithCtrlAKeyPress_VerifyMoveCaretWasCalled()
        {
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo('\0', ConsoleKey.A, false, false, true);
            Shell shell = CreateShell(consoleKeyInfo,
                caretPosition: 3,
                previousCommand: null,
                nextCommand: null,
                out CancellationTokenSource cancellationTokenSource);

            string inputBufferContents = "set base \"https://localhost:44366/\"";

            IShellState shellState = shell.ShellState;
            shellState.InputManager.SetInput(shellState, inputBufferContents);

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            // The first method call is from SetInput(..) and the second one is from End key press event
            VerifyMoveCaretMethodWasCalledOnce(shellState, -3, Times.Exactly(2));
        }

        [Fact]
        public async Task RunAsync_WithEndKeyPress_VerifyMoveCaretWasCalled()
        {
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo('\0', ConsoleKey.End, false, false, false);
            Shell shell = CreateShell(consoleKeyInfo,
                caretPosition: 3,
                previousCommand: null,
                nextCommand: null,
                out CancellationTokenSource cancellationTokenSource);

            string inputBufferContents = "set base \"https://localhost:44366/\"";

            IShellState shellState = shell.ShellState;
            shellState.InputManager.SetInput(shellState, inputBufferContents);

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            VerifyMoveCaretMethodWasCalledOnce(shellState, 32, Times.Once());
        }

        [Fact]
        public async Task RunAsync_WithCtrlEKeyPress_VerifyMoveCaretWasCalled()
        {
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo('\0', ConsoleKey.E, false, false, true);
            Shell shell = CreateShell(consoleKeyInfo,
                caretPosition: 3,
                previousCommand: null,
                nextCommand: null,
                out CancellationTokenSource cancellationTokenSource);

            string inputBufferContents = "set base \"https://localhost:44366/\"";

            IShellState shellState = shell.ShellState;
            shellState.InputManager.SetInput(shellState, inputBufferContents);

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            VerifyMoveCaretMethodWasCalledOnce(shellState, 32, Times.Once());
        }

        private Shell CreateShell(ConsoleKeyInfo consoleKeyInfo, int caretPosition, string previousCommand, string nextCommand, out CancellationTokenSource cancellationTokenSource)
        {
            var defaultCommandDispatcher = DefaultCommandDispatcher.Create(x => { }, new object());

            Mock<IConsoleManager> mockConsoleManager = new Mock<IConsoleManager>();
            var cts = new CancellationTokenSource();
            CancellationToken cancellationToken = cts.Token;
            mockConsoleManager.Setup(s => s.ReadKey(cancellationToken))
                .Callback(() => cts.Cancel()) // This is required as we want to break the loop in StartAsync(..) in InputManager after intended key press event, so that Shell.RunAsync() returns
                .Returns(consoleKeyInfo);
            mockConsoleManager.Setup(s => s.CaretPosition)
                .Returns(caretPosition);
            mockConsoleManager.Setup(s => s.Error.Write(It.IsAny<char>()));

            Mock<ICommandHistory> mockCommandHistory = new Mock<ICommandHistory>();
            mockCommandHistory.Setup(s => s.GetPreviousCommand())
                .Returns(previousCommand);
            mockCommandHistory.Setup(s => s.GetNextCommand())
                .Returns(nextCommand);

            ShellState shellState = new ShellState(defaultCommandDispatcher,
                consoleManager: mockConsoleManager.Object,
                commandHistory: mockCommandHistory.Object);

            cancellationTokenSource = cts;

            return new Shell(shellState);
        }

        private void VerifyMoveCaretMethodWasCalledOnce(IShellState shellState, int caretPosition, Times times)
        {
            Mock<IConsoleManager> consoleMananger = Mock.Get(shellState.ConsoleManager);

            consoleMananger.Verify(s => s.MoveCaret(caretPosition), times);
        }
    }
}
