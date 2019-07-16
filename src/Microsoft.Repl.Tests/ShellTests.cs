using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Fakes;
using Microsoft.Repl.Commanding;
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
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo(keyChar: '\0',
                key: ConsoleKey.UpArrow,
                shift: false,
                alt: false,
                control: false);
            Shell shell = CreateShell(consoleKeyInfo,
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
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo(keyChar: '\0',
                key: ConsoleKey.UpArrow,
                shift: false,
                alt: false,
                control: false);
            Shell shell = CreateShell(consoleKeyInfo,
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
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo(keyChar: '\0',
                key: ConsoleKey.DownArrow,
                shift: false,
                alt: false,
                control: false);
            Shell shell = CreateShell(consoleKeyInfo,
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
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo(keyChar: '\0',
                key: ConsoleKey.Delete,
                shift: false,
                alt: false,
                control: false);
            Shell shell = CreateShell(consoleKeyInfo,
                previousCommand: null,
                nextCommand: null,
                out CancellationTokenSource cancellationTokenSource);

            string inputBufferTextBeforeKeyPress = "get";
            string inputBufferTextAfterKeyPress = "ge";

            IShellState shellState = shell.ShellState;
            shellState.InputManager.SetInput(shellState, inputBufferTextBeforeKeyPress);

            shellState.ConsoleManager.MoveCaret(2);

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            // Verify the input buffer contents after Delete key press event
            Assert.Equal(inputBufferTextAfterKeyPress, shell.ShellState.InputManager.GetCurrentBuffer());
        }

        [Fact]
        public async Task RunAsync_WithBackspaceKeyPress_DeletesPreviousCharacterInTheInputBuffer()
        {
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo(keyChar: '\0',
                key: ConsoleKey.Backspace,
                shift: false,
                alt: false,
                control: false);
            Shell shell = CreateShell(consoleKeyInfo,
                previousCommand: null,
                nextCommand: null,
                out CancellationTokenSource cancellationTokenSource);

            string inputBufferTextBeforeKeyPress = "get";
            string inputBufferTextAfterKeyPress = "gt";

            IShellState shellState = shell.ShellState;
            shellState.InputManager.SetInput(shellState, inputBufferTextBeforeKeyPress);

            shellState.ConsoleManager.MoveCaret(2);

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            // Verify the input buffer contents after Backspace key press event
            Assert.Equal(inputBufferTextAfterKeyPress, shell.ShellState.InputManager.GetCurrentBuffer());
        }

        [Fact]
        public async Task RunAsync_WithEscapeKeyPress_UpdatesInputBufferWithEmptyString()
        {
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo(keyChar: '\0',
                key: ConsoleKey.Escape,
                shift: false,
                alt: false,
                control: false);
            Shell shell = CreateShell(consoleKeyInfo,
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
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo(keyChar: '\0',
                key: ConsoleKey.U,
                shift: false,
                alt: false,
                control: true);
            Shell shell = CreateShell(consoleKeyInfo,
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
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo(keyChar: '\0',
                key: ConsoleKey.Insert,
                shift: false,
                alt: false,
                control: false);
            Shell shell = CreateShell(consoleKeyInfo,
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
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo(keyChar: '\0',
                key: ConsoleKey.F1,
                shift: false,
                alt: false,
                control: false);
            Shell shell = CreateShell(consoleKeyInfo,
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
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo(keyChar: '\0',
                key: ConsoleKey.Tab,
                shift: false,
                alt: false,
                control: false);
            Shell shell = CreateShell(consoleKeyInfo,
                 previousCommand: null,
                 nextCommand: null,
                 out CancellationTokenSource cancellationTokenSource);

            string inputBufferTextBeforeKeyPress = "c";
            string inputBufferTextAfterKeyPress = "cd";

            IShellState shellState = shell.ShellState;
            shellState.InputManager.SetInput(shellState, inputBufferTextBeforeKeyPress);

            DefaultCommandDispatcher<object> defaultCommandDispatcher = shellState.CommandDispatcher as DefaultCommandDispatcher<object>;
            string cdCommandName = "cd";
            defaultCommandDispatcher.AddCommand(new MockCommand(cdCommandName));
            string clearCommandName = "clear";
            defaultCommandDispatcher.AddCommand(new MockCommand(clearCommandName));

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            // Verify the input buffer contents after tab key press event
            Assert.Equal(inputBufferTextAfterKeyPress, shell.ShellState.InputManager.GetCurrentBuffer());
        }

        [Fact]
        public async Task RunAsync_WithShiftTabKeyPress_UpdatesInputBufferWithLastEntryFromSuggestionList()
        {
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo(keyChar: '\0',
                key: ConsoleKey.Tab,
                shift: true,
                alt: false,
                control: false);
            Shell shell = CreateShell(consoleKeyInfo,
                 previousCommand: null,
                 nextCommand: null,
                 out CancellationTokenSource cancellationTokenSource);

            string inputBufferTextBeforeKeyPress = "c";
            string inputBufferTextAfterKeyPress = "clear";

            IShellState shellState = shell.ShellState;
            shellState.InputManager.SetInput(shellState, inputBufferTextBeforeKeyPress);

            DefaultCommandDispatcher<object> defaultCommandDispatcher = shellState.CommandDispatcher as DefaultCommandDispatcher<object>;
            string cdCommandName = "cd";
            defaultCommandDispatcher.AddCommand(new MockCommand(cdCommandName));
            string clearCommandName = "clear";
            defaultCommandDispatcher.AddCommand(new MockCommand(clearCommandName));

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            // Verify the input buffer contents after Shift + Tab key press event
            Assert.Equal(inputBufferTextAfterKeyPress, shell.ShellState.InputManager.GetCurrentBuffer());
        }

        [Fact]
        public async Task RunAsync_WithTabKeyPressAndNoSuggestions_DoesNothing()
        {
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo(keyChar: '\0',
                key: ConsoleKey.Tab,
                shift: false,
                alt: false,
                control: false);
            Shell shell = CreateShell(consoleKeyInfo,
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
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo(keyChar: '\0',
                key: ConsoleKey.Tab,
                shift: true,
                alt: false,
                control: false);
            Shell shell = CreateShell(consoleKeyInfo,
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
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo(keyChar: '\0',
                key: ConsoleKey.Enter,
                shift: false,
                alt: false,
                control: false);
            Shell shell = CreateShell(consoleKeyInfo,
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
        public async Task RunAsync_WithLeftArrowKeyPress_VerifyCaretPositionWasUpdated()
        {
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo(keyChar: '\0',
                key: ConsoleKey.LeftArrow,
                shift: false,
                alt: false,
                control: false);
            Shell shell = CreateShell(consoleKeyInfo,
                previousCommand: null,
                nextCommand: null,
                out CancellationTokenSource cancellationTokenSource);

            string inputBufferContents = "set base \"https://localhost:44366/\"";

            IShellState shellState = shell.ShellState;
            shellState.InputManager.SetInput(shellState, inputBufferContents);

            shellState.ConsoleManager.MoveCaret(3);

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            Assert.Equal(2, shellState.ConsoleManager.CaretPosition);
        }

        [Fact]
        public async Task RunAsync_WithControlLeftArrowKeyPress_VerifyCaretPositionWasUpdated()
        {
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo(keyChar: '\0',
                key: ConsoleKey.LeftArrow,
                shift: false,
                alt: false,
                control: true);
            Shell shell = CreateShell(consoleKeyInfo,
                previousCommand: null,
                nextCommand: null,
                out CancellationTokenSource cancellationTokenSource);

            string inputBufferContents = "set base \"https://localhost:44366/\"";

            IShellState shellState = shell.ShellState;
            shellState.InputManager.SetInput(shellState, inputBufferContents);

            shellState.ConsoleManager.MoveCaret(7);

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            Assert.Equal(4, shellState.ConsoleManager.CaretPosition);
        }

        [Fact]
        public async Task RunAsync_WithRightArrowKeyPress_VerifyCaretPositionWasUpdated()
        {
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo(keyChar: '\0',
                key: ConsoleKey.RightArrow,
                shift: false,
                alt: false,
                control: false);
            Shell shell = CreateShell(consoleKeyInfo,
                previousCommand: null,
                nextCommand: null,
                out CancellationTokenSource cancellationTokenSource);

            string inputBufferContents = "set base \"https://localhost:44366/\"";

            IShellState shellState = shell.ShellState;
            shellState.InputManager.SetInput(shellState, inputBufferContents);

            shellState.ConsoleManager.MoveCaret(3);

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            Assert.Equal(4, shellState.ConsoleManager.CaretPosition);
        }

        [Fact]
        public async Task RunAsync_WithControlRightArrowKeyPress_VerifyCaretPositionWasUpdated()
        {
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo(keyChar: '\0',
                key: ConsoleKey.RightArrow,
                shift: false,
                alt: false,
                control: true);
            Shell shell = CreateShell(consoleKeyInfo,
                previousCommand: null,
                nextCommand: null,
                out CancellationTokenSource cancellationTokenSource);

            string inputBufferContents = "set base \"https://localhost:44366/\"";

            IShellState shellState = shell.ShellState;
            shellState.InputManager.SetInput(shellState, inputBufferContents);

            shellState.ConsoleManager.MoveCaret(4);

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            Assert.Equal(9, shellState.ConsoleManager.CaretPosition);
        }

        [Fact]
        public async Task RunAsync_WithHomeKeyPress_VerifyCaretPositionWasUpdated()
        {
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo(keyChar: '\0',
                key: ConsoleKey.Home,
                shift: false,
                alt: false,
                control: false);
            Shell shell = CreateShell(consoleKeyInfo,
                previousCommand: null,
                nextCommand: null,
                out CancellationTokenSource cancellationTokenSource);

            string inputBufferContents = "set base \"https://localhost:44366/\"";

            IShellState shellState = shell.ShellState;
            shellState.InputManager.SetInput(shellState, inputBufferContents);

            shellState.ConsoleManager.MoveCaret(3);

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            Assert.Equal(0, shellState.ConsoleManager.CaretPosition);
        }

        [Fact]
        public async Task RunAsync_WithCtrlAKeyPress_VerifyCaretPositionWasUpdated()
        {
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo(keyChar: '\0',
                key: ConsoleKey.A,
                shift: false,
                alt: false,
                control: true);
            Shell shell = CreateShell(consoleKeyInfo,
                previousCommand: null,
                nextCommand: null,
                out CancellationTokenSource cancellationTokenSource);

            string inputBufferContents = "set base \"https://localhost:44366/\"";

            IShellState shellState = shell.ShellState;
            shellState.InputManager.SetInput(shellState, inputBufferContents);

            shellState.ConsoleManager.MoveCaret(3);

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            Assert.Equal(0, shellState.ConsoleManager.CaretPosition);
        }

        [Fact]
        public async Task RunAsync_WithEndKeyPress_VerifyCaretPositionWasUpdated()
        {
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo(keyChar: '\0',
                key: ConsoleKey.End,
                shift: false,
                alt: false,
                control: false);
            Shell shell = CreateShell(consoleKeyInfo,
                previousCommand: null,
                nextCommand: null,
                out CancellationTokenSource cancellationTokenSource);

            string inputBufferContents = "set base \"https://localhost:44366/\"";

            IShellState shellState = shell.ShellState;
            shellState.InputManager.SetInput(shellState, inputBufferContents);

            shellState.ConsoleManager.MoveCaret(3);

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            Assert.Equal(35, shellState.ConsoleManager.CaretPosition);
        }

        [Fact]
        public async Task RunAsync_WithCtrlEKeyPress_VerifyCaretPositionWasUpdated()
        {
            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo(keyChar: '\0',
                key: ConsoleKey.E,
                shift: false,
                alt: false,
                control: true);
            Shell shell = CreateShell(consoleKeyInfo,
                previousCommand: null,
                nextCommand: null,
                out CancellationTokenSource cancellationTokenSource);

            string inputBufferContents = "set base \"https://localhost:44366/\"";

            IShellState shellState = shell.ShellState;
            shellState.InputManager.SetInput(shellState, inputBufferContents);

            shellState.ConsoleManager.MoveCaret(3);

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            Assert.Equal(35, shellState.ConsoleManager.CaretPosition);
        }

        private Shell CreateShell(ConsoleKeyInfo consoleKeyInfo,
            string previousCommand,
            string nextCommand,
            out CancellationTokenSource cancellationTokenSource)
        {
            var defaultCommandDispatcher = DefaultCommandDispatcher.Create(x => { }, new object());

            cancellationTokenSource = new CancellationTokenSource();
            MockConsoleManager mockConsoleManager = new MockConsoleManager(consoleKeyInfo, cancellationTokenSource);

            Mock<ICommandHistory> mockCommandHistory = new Mock<ICommandHistory>();
            mockCommandHistory.Setup(s => s.GetPreviousCommand())
                .Returns(previousCommand);
            mockCommandHistory.Setup(s => s.GetNextCommand())
                .Returns(nextCommand);

            ShellState shellState = new ShellState(defaultCommandDispatcher,
                consoleManager: mockConsoleManager,
                commandHistory: mockCommandHistory.Object);

            return new Shell(shellState);
        }
    }
}
