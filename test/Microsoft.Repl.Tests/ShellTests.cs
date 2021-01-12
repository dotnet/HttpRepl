using System;
using System.Collections.Generic;
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
            Assert.Equal(previousCommand.Length, shell.ShellState.InputManager.CaretPosition);
        }

        [Fact]
        public async Task RunAsync_WithUpArrowKeyPressWithLongPreviousCommand_UpdatesCurrentBufferWithPreviousCommand()
        {
            string previousCommand = "connect \"https://localhost:44366/\" --base \"https://localhost:44366/api/v2/\" --openapi \"https://localhost:44366/openapi/v2/openapi.yaml\"";
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
            Assert.Equal(previousCommand.Length, shell.ShellState.InputManager.CaretPosition);
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
            Assert.Equal(0, shell.ShellState.InputManager.CaretPosition);

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            // Verify the input buffer has previous command after the UpArrow key press event
            Assert.Equal(previousCommand, shell.ShellState.InputManager.GetCurrentBuffer());
            Assert.Equal(previousCommand.Length, shell.ShellState.InputManager.CaretPosition);
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
            Assert.Equal(nextCommand.Length, shell.ShellState.InputManager.CaretPosition);
        }

        [Fact]
        public async Task RunAsync_WithDeleteKeyPress_DeletesCurrentCharacterInTheInputBuffer()
        {
            ConsoleKeyInfo[] keys = new[]
            {
                GetConsoleKeyInfo('g'),
                GetConsoleKeyInfo('e'),
                GetConsoleKeyInfo('t'),
                GetConsoleKeyInfo(ConsoleKey.LeftArrow),
                GetConsoleKeyInfo(ConsoleKey.Delete)
            };

            Shell shell = CreateShell(keys,
                previousCommand: null,
                nextCommand: null,
                out CancellationTokenSource cancellationTokenSource);

            string inputBufferTextAfterKeyPress = "ge";

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            // Verify the input buffer contents after Delete key press event
            Assert.Equal(inputBufferTextAfterKeyPress, shell.ShellState.InputManager.GetCurrentBuffer());
        }

        [Fact]
        public async Task RunAsync_WithBackspaceKeyPress_DeletesPreviousCharacterInTheInputBuffer()
        {
            ConsoleKeyInfo[] keys = new[]
            {
                GetConsoleKeyInfo('g'),
                GetConsoleKeyInfo('e'),
                GetConsoleKeyInfo('t'),
                GetConsoleKeyInfo(ConsoleKey.LeftArrow),
                GetConsoleKeyInfo(ConsoleKey.Backspace)
            };

            Shell shell = CreateShell(keys,
                previousCommand: null,
                nextCommand: null,
                out CancellationTokenSource cancellationTokenSource);

            string inputBufferTextAfterKeyPress = "gt";

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            // Verify the input buffer contents after Backspace key press event
            Assert.Equal(inputBufferTextAfterKeyPress, shell.ShellState.InputManager.GetCurrentBuffer());
        }

        [Fact]
        public async Task RunAsync_WithEscapeKeyPress_UpdatesInputBufferWithEmptyString()
        {
            ConsoleKeyInfo[] keys = new[]
{
                GetConsoleKeyInfo('g'),
                GetConsoleKeyInfo('e'),
                GetConsoleKeyInfo('t'),
                GetConsoleKeyInfo(ConsoleKey.Escape),
            };
            Shell shell = CreateShell(keys,
                previousCommand: null,
                nextCommand: null,
                out CancellationTokenSource cancellationTokenSource);

            string inputBufferTextAfterKeyPress = string.Empty;

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            // Verify the input buffer contents after Escape key press event
            Assert.Equal(inputBufferTextAfterKeyPress, shell.ShellState.InputManager.GetCurrentBuffer());
        }

        [Fact]
        public async Task RunAsync_WithCtrlUKeyPress_UpdatesInputBufferWithEmptyString()
        {
            ConsoleKeyInfo[] keys = new[]
            {
                GetConsoleKeyInfo('g'),
                GetConsoleKeyInfo('e'),
                GetConsoleKeyInfo('t'),
                new(keyChar: '\0', key: ConsoleKey.U, shift: false, alt: false, control: true),
            };

            Shell shell = CreateShell(keys,
                previousCommand: null,
                nextCommand: null,
                out CancellationTokenSource cancellationTokenSource);

            string inputBufferTextAfterKeyPress = string.Empty;

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
            ConsoleKeyInfo[] keys = new[]
            {
                GetConsoleKeyInfo('g'),
                GetConsoleKeyInfo('e'),
                GetConsoleKeyInfo('t'),
                GetConsoleKeyInfo(ConsoleKey.F1),
            };

            Shell shell = CreateShell(keys,
                previousCommand: null,
                nextCommand: null,
                out CancellationTokenSource cancellationTokenSource);

            string inputBufferText = "get";

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            // Verify the input buffer contents after F1 key press event
            Assert.Equal(inputBufferText, shell.ShellState.InputManager.GetCurrentBuffer());
        }

        [Fact]
        public async Task RunAsync_WithTabKeyPress_UpdatesInputBufferWithFirstEntryFromSuggestionList()
        {
            ConsoleKeyInfo[] keys = new[]
            {
                GetConsoleKeyInfo('c'),
                GetConsoleKeyInfo(ConsoleKey.Tab),
            };

            Shell shell = CreateShell(keys,
                 previousCommand: null,
                 nextCommand: null,
                 out CancellationTokenSource cancellationTokenSource);

            string inputBufferTextAfterKeyPress = "cd";

            DefaultCommandDispatcher<object> defaultCommandDispatcher = shell.ShellState.CommandDispatcher as DefaultCommandDispatcher<object>;
            string cdCommandName = "cd";
            defaultCommandDispatcher.AddCommand(new MockCommand(cdCommandName));
            string clearCommandName = "clear";
            defaultCommandDispatcher.AddCommand(new MockCommand(clearCommandName));

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            // Verify the input buffer contents after tab key press event
            Assert.Equal(inputBufferTextAfterKeyPress, shell.ShellState.InputManager.GetCurrentBuffer());
            Assert.Equal(inputBufferTextAfterKeyPress.Length, shell.ShellState.InputManager.CaretPosition);
        }

        [Fact]
        public async Task RunAsync_WithShiftTabKeyPress_UpdatesInputBufferWithLastEntryFromSuggestionList()
        {
            ConsoleKeyInfo[] keys = new[]
            {
                GetConsoleKeyInfo('c'),
                new(keyChar: '\0', key: ConsoleKey.Tab, shift: true, alt: false, control: false),
            };

            Shell shell = CreateShell(keys,
                 previousCommand: null,
                 nextCommand: null,
                 out CancellationTokenSource cancellationTokenSource);

            string inputBufferTextAfterKeyPress = "clear";

            DefaultCommandDispatcher<object> defaultCommandDispatcher = shell.ShellState.CommandDispatcher as DefaultCommandDispatcher<object>;
            string cdCommandName = "cd";
            defaultCommandDispatcher.AddCommand(new MockCommand(cdCommandName));
            string clearCommandName = "clear";
            defaultCommandDispatcher.AddCommand(new MockCommand(clearCommandName));

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            // Verify the input buffer contents after Shift + Tab key press event
            Assert.Equal(inputBufferTextAfterKeyPress, shell.ShellState.InputManager.GetCurrentBuffer());
            Assert.Equal(inputBufferTextAfterKeyPress.Length, shell.ShellState.InputManager.CaretPosition);
        }

        [Fact]
        public async Task RunAsync_WithTabKeyPressAndNoSuggestions_DoesNothing()
        {
            ConsoleKeyInfo[] keys = new[]
            {
                GetConsoleKeyInfo('z'),
                GetConsoleKeyInfo(ConsoleKey.Tab),
            };

            Shell shell = CreateShell(keys,
                 previousCommand: null,
                 nextCommand: null,
                 out CancellationTokenSource cancellationTokenSource);

            string inputBufferTextAfterKeyPress = "z";

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            // Verify the input buffer contents after tab key press event
            Assert.Equal(inputBufferTextAfterKeyPress, shell.ShellState.InputManager.GetCurrentBuffer());
        }

        [Fact]
        public async Task RunAsync_WithShiftTabKeyPressAndNoSuggestions_DoesNothing()
        {
            ConsoleKeyInfo[] keys = new[]
            {
                GetConsoleKeyInfo('z'),
                new(keyChar: '\0', key: ConsoleKey.Tab, shift: true, alt: false, control: false)
            };

            Shell shell = CreateShell(keys,
                 previousCommand: null,
                 nextCommand: null,
                 out CancellationTokenSource cancellationTokenSource);

            string inputBufferTextAfterKeyPress = "z";

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            // Verify the input buffer contents after Shift + Tab key press event
            Assert.Equal(inputBufferTextAfterKeyPress, shell.ShellState.InputManager.GetCurrentBuffer());
        }

        [Fact]
        public async Task RunAsync_WithEnterKeyPress_UpdatesInputBufferWithEmptyString()
        {
            string input = "set base \"https://localhost:44366/\"";
            List<ConsoleKeyInfo> keys = GetConsoleKeyInfo(input);
            keys.Add(new(keyChar: '\0', key: ConsoleKey.Enter, shift: false, alt: false, control: false));

            Shell shell = CreateShell(keys,
                previousCommand: null,
                nextCommand: null,
                out CancellationTokenSource cancellationTokenSource);

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            Assert.Equal(string.Empty, shell.ShellState.InputManager.GetCurrentBuffer());
            Assert.Equal(0, shell.ShellState.InputManager.CaretPosition);
        }

        [Fact]
        public async Task RunAsync_WithLeftArrowKeyPress_VerifyCaretPositionWasUpdated()
        {
            string input = "set base \"https://localhost:44366/\"";
            List<ConsoleKeyInfo> keys = GetConsoleKeyInfo(input);
            keys.Add(new(keyChar: '\0', key: ConsoleKey.LeftArrow, shift: false, alt: false, control: false));

            Shell shell = CreateShell(keys,
                previousCommand: null,
                nextCommand: null,
                out CancellationTokenSource cancellationTokenSource);

            IShellState shellState = shell.ShellState;

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            Assert.Equal(input.Length - 1, shellState.InputManager.CaretPosition);
        }

        [Fact]
        public async Task RunAsync_WithControlLeftArrowKeyPress_VerifyCaretPositionWasUpdated()
        {
            string input = "set base \"https://localhost:44366/\"";
            List<ConsoleKeyInfo> keys = GetConsoleKeyInfo(input);
            keys.Add(new(keyChar: '\0', key: ConsoleKey.LeftArrow, shift: false, alt: false, control: true));

            Shell shell = CreateShell(keys,
                previousCommand: null,
                nextCommand: null,
                out CancellationTokenSource cancellationTokenSource);

            IShellState shellState = shell.ShellState;

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            Assert.Equal(9, shellState.InputManager.CaretPosition);
        }

        [Fact]
        public async Task RunAsync_WithRightArrowKeyPress_VerifyCaretPositionWasUpdated()
        {
            string input = "set base \"https://localhost:44366/\"";
            List<ConsoleKeyInfo> keys = GetConsoleKeyInfo(input);
            keys.Add(new(keyChar: '\0', key: ConsoleKey.LeftArrow, shift: false, alt: false, control: false));
            keys.Add(new(keyChar: '\0', key: ConsoleKey.LeftArrow, shift: false, alt: false, control: false));
            keys.Add(new(keyChar: '\0', key: ConsoleKey.LeftArrow, shift: false, alt: false, control: false));
            keys.Add(new(keyChar: '\0', key: ConsoleKey.RightArrow, shift: false, alt: false, control: false));

            Shell shell = CreateShell(keys,
                previousCommand: null,
                nextCommand: null,
                out CancellationTokenSource cancellationTokenSource);

            IShellState shellState = shell.ShellState;

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            Assert.Equal(input.Length - 2, shellState.InputManager.CaretPosition);
        }

        [Fact]
        public async Task RunAsync_WithControlRightArrowKeyPress_VerifyCaretPositionWasUpdated()
        {
            string input = "set base \"https://localhost:44366/\"";
            List<ConsoleKeyInfo> keys = GetConsoleKeyInfo(input);
            keys.Add(new(keyChar: '\0', key: ConsoleKey.LeftArrow, shift: false, alt: false, control: false));
            keys.Add(new(keyChar: '\0', key: ConsoleKey.LeftArrow, shift: false, alt: false, control: false));
            keys.Add(new(keyChar: '\0', key: ConsoleKey.LeftArrow, shift: false, alt: false, control: false));
            keys.Add(new(keyChar: '\0', key: ConsoleKey.RightArrow, shift: false, alt: false, control: true));

            Shell shell = CreateShell(keys,
                previousCommand: null,
                nextCommand: null,
                out CancellationTokenSource cancellationTokenSource);

            IShellState shellState = shell.ShellState;

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            Assert.Equal(input.Length, shellState.InputManager.CaretPosition);
        }

        [Fact]
        public async Task RunAsync_WithHomeKeyPress_VerifyCaretPositionWasUpdated()
        {
            string input = "set base \"https://localhost:44366/\"";
            List<ConsoleKeyInfo> keys = GetConsoleKeyInfo(input);
            keys.Add(new(keyChar: '\0', key: ConsoleKey.Home, shift: false, alt: false, control: false));

            Shell shell = CreateShell(keys,
                previousCommand: null,
                nextCommand: null,
                out CancellationTokenSource cancellationTokenSource);

            IShellState shellState = shell.ShellState;

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            Assert.Equal(0, shellState.InputManager.CaretPosition);
        }

        [Fact]
        public async Task RunAsync_WithCtrlAKeyPress_VerifyCaretPositionWasUpdated()
        {
            string input = "set base \"https://localhost:44366/\"";
            List<ConsoleKeyInfo> keys = GetConsoleKeyInfo(input);
            keys.Add(new(keyChar: '\0', key: ConsoleKey.A, shift: false, alt: false, control: true));

            Shell shell = CreateShell(keys,
                previousCommand: null,
                nextCommand: null,
                out CancellationTokenSource cancellationTokenSource);

            IShellState shellState = shell.ShellState;

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            Assert.Equal(0, shellState.InputManager.CaretPosition);
        }

        [Fact]
        public async Task RunAsync_WithEndKeyPress_VerifyCaretPositionWasUpdated()
        {
            string input = "set base \"https://localhost:44366/\"";
            List<ConsoleKeyInfo> keys = GetConsoleKeyInfo(input);
            keys.Add(new(keyChar: '\0', key: ConsoleKey.LeftArrow, shift: false, alt: false, control: false));
            keys.Add(new(keyChar: '\0', key: ConsoleKey.LeftArrow, shift: false, alt: false, control: false));
            keys.Add(new(keyChar: '\0', key: ConsoleKey.LeftArrow, shift: false, alt: false, control: false));
            keys.Add(new(keyChar: '\0', key: ConsoleKey.End, shift: false, alt: false, control: false));

            Shell shell = CreateShell(keys,
                previousCommand: null,
                nextCommand: null,
                out CancellationTokenSource cancellationTokenSource);

            IShellState shellState = shell.ShellState;

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            Assert.Equal(input.Length, shellState.InputManager.CaretPosition);
        }

        [Fact]
        public async Task RunAsync_WithCtrlEKeyPress_VerifyCaretPositionWasUpdated()
        {
            string input = "set base \"https://localhost:44366/\"";
            List<ConsoleKeyInfo> keys = GetConsoleKeyInfo(input);
            keys.Add(new(keyChar: '\0', key: ConsoleKey.LeftArrow, shift: false, alt: false, control: false));
            keys.Add(new(keyChar: '\0', key: ConsoleKey.LeftArrow, shift: false, alt: false, control: false));
            keys.Add(new(keyChar: '\0', key: ConsoleKey.LeftArrow, shift: false, alt: false, control: false));
            keys.Add(new(keyChar: '\0', key: ConsoleKey.E, shift: false, alt: false, control: true));

            Shell shell = CreateShell(keys,
                previousCommand: null,
                nextCommand: null,
                out CancellationTokenSource cancellationTokenSource);

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            Assert.Equal(input.Length, shell.ShellState.InputManager.CaretPosition);
        }

        [Fact]
        public async Task RunAsync_WithInvalidCommand_VerifyInputBufferIsCleared()
        {
            string input = "this is an invalid command\n";
            List<ConsoleKeyInfo> keys = GetConsoleKeyInfo(input);

            Shell shell = CreateShell(keys,
                previousCommand: null,
                nextCommand: null,
                out CancellationTokenSource cancellationTokenSource);

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            Assert.Empty(shell.ShellState.InputManager.GetCurrentBuffer());
            Assert.Equal(0, shell.ShellState.InputManager.CaretPosition);
        }

        [Fact]
        public async Task RunAsync_WithTwoInvalidCommandsAndTwoUpArrows_VerifyInputBufferIsCorrect()
        {
            string commandOne = "longer invalid command text";
            string commandTwo = "small invalid cmd";
            string input = $"{commandOne}\n{commandTwo}\n";
            List<ConsoleKeyInfo> keys = GetConsoleKeyInfo(input);
            keys.Add(GetConsoleKeyInfo(ConsoleKey.UpArrow));
            keys.Add(GetConsoleKeyInfo(ConsoleKey.UpArrow));

            Shell shell = CreateShell(keys, out CancellationTokenSource cancellationTokenSource);

            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            Assert.Equal(commandOne,shell.ShellState.InputManager.GetCurrentBuffer());
            Assert.Equal(commandOne.Length, shell.ShellState.InputManager.CaretPosition);
        }

        [Fact]
        public async Task RunAsync_WithNullCharacters_NullCharactersIgnored()
        {
            // Arrange
            string command = "Command With \0 Character";
            string expectedCommand = command.Replace("\0", "");
            List<ConsoleKeyInfo> keys = GetConsoleKeyInfo(command);

            Shell shell = CreateShell(keys, out CancellationTokenSource cancellationTokenSource);

            // Act
            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            // Assert
            Assert.Equal(expectedCommand, shell.ShellState.InputManager.GetCurrentBuffer());
            Assert.Equal(expectedCommand.Length, shell.ShellState.InputManager.CaretPosition);
        }

        [Fact]
        public async Task RunAsync_WithNullCharactersAndBackspaces_NullCharactersIgnored()
        {
            // Arrange
            string command = "Command With \0 ";
            string expectedCommand = "Command Wit";
            List<ConsoleKeyInfo> keys = GetConsoleKeyInfo(command);
            keys.Add(GetConsoleKeyInfo(ConsoleKey.Backspace));
            keys.Add(GetConsoleKeyInfo(ConsoleKey.Backspace));
            keys.Add(GetConsoleKeyInfo(ConsoleKey.Backspace));

            Shell shell = CreateShell(keys, out CancellationTokenSource cancellationTokenSource);

            // Act
            await shell.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            // Assert
            Assert.Equal(expectedCommand, shell.ShellState.InputManager.GetCurrentBuffer());
            Assert.Equal(expectedCommand.Length, shell.ShellState.InputManager.CaretPosition);
        }

        private static Shell CreateShell(IEnumerable<ConsoleKeyInfo> consoleKeyInfo,
            string previousCommand,
            string nextCommand,
            out CancellationTokenSource cancellationTokenSource)
        {
            DefaultCommandDispatcher<object> defaultCommandDispatcher = DefaultCommandDispatcher.Create(x => { }, new object());

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

        private static Shell CreateShell(IEnumerable<ConsoleKeyInfo> consoleKeyInfo, out CancellationTokenSource cancellationTokenSource)
        {
            DefaultCommandDispatcher<object> defaultCommandDispatcher = DefaultCommandDispatcher.Create(x => { }, new object());

            cancellationTokenSource = new CancellationTokenSource();
            MockConsoleManager mockConsoleManager = new MockConsoleManager(consoleKeyInfo, cancellationTokenSource);

            ShellState shellState = new ShellState(defaultCommandDispatcher,
                consoleManager: mockConsoleManager);

            return new Shell(shellState);
        }

        private static Shell CreateShell(ConsoleKeyInfo consoleKeyInfo, string previousCommand, string nextCommand, out CancellationTokenSource cancellationTokenSource)
        {
            return CreateShell(new ConsoleKeyInfo[] { consoleKeyInfo }, previousCommand, nextCommand, out cancellationTokenSource);
        }

        /// <summary>
        /// Converts a string into the series of ConsoleKeyInfo instances that would be used
        /// to type the string in the console.
        /// </summary>
        private static List<ConsoleKeyInfo> GetConsoleKeyInfo(string text)
        {
            List<ConsoleKeyInfo> keys = new();

            text = text.Replace("\r\n", "\n");

            foreach (char c in text)
            {
                ConsoleKeyInfo consoleKeyInfo = GetConsoleKeyInfo(c);
                keys.Add(consoleKeyInfo);
            }

            return keys;
        }

        /// <summary>
        /// Builds a ConsoleKeyInfo object with the specified console key, the null keyChar ('\0') and no modifiers.
        /// Intended for non-printable keystrokes.
        /// </summary>
        private static ConsoleKeyInfo GetConsoleKeyInfo(ConsoleKey key) => new('\0', key, shift: false, alt: false, control: false);

        /// <summary>
        /// Builds a ConsoleKeyInfo that could produce the specified character
        /// </summary>
        private static ConsoleKeyInfo GetConsoleKeyInfo(char keyChar)
        {
            return keyChar switch
            {
                >= 'a' and <= 'z'        => new(keyChar: keyChar, key: (ConsoleKey)(keyChar - 32), shift: false, alt: false, control: false),
                >= 'A' and <= 'Z'        => new(keyChar: keyChar, key: (ConsoleKey)keyChar,        shift: true,  alt: false, control: false),
                >= '0' and <= '9' or ' ' => new(keyChar: keyChar, key: (ConsoleKey)keyChar,        shift: false, alt: false, control: false),
                ':'                      => new(keyChar: keyChar, key: ConsoleKey.Oem1,            shift: true,  alt: false, control: false),
                '/'                      => new(keyChar: keyChar, key: ConsoleKey.Oem2,            shift: false, alt: false, control: false),
                '"'                      => new(keyChar: keyChar, key: ConsoleKey.Oem7,            shift: true,  alt: false, control: false),
                '\n'                     => new(keyChar: '\r',    key: ConsoleKey.Enter,           shift: false, alt: false, control: false),
                '\0'                     => new(keyChar: keyChar, key: ConsoleKey.Oem102,          shift: false, alt: false, control: false),
                _                        => throw new InvalidOperationException($"Test setup does not support '{keyChar}' yet."),
            };
        }
    }
}
