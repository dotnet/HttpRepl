// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Repl.Commanding;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Input;
using Microsoft.Repl.Suggestions;

namespace Microsoft.Repl
{
    public class Shell
    {
        public Shell(IShellState shellState)
        {
            shellState = shellState ?? throw new ArgumentNullException(nameof(shellState));

            KeyHandlers.RegisterDefaultKeyHandlers(shellState.InputManager);
            ShellState = shellState;
        }

        public Shell(ICommandDispatcher dispatcher, ISuggestionManager suggestionManager = null, IConsoleManager consoleManager = null)
            : this(new ShellState(dispatcher, suggestionManager, consoleManager: consoleManager))
        {
        }

        public IShellState ShellState { get; }

        public Task RunAsync(CancellationToken cancellationToken)
        {
            ShellState.CommandDispatcher.OnReady(ShellState);
            return ShellState.InputManager.StartAsync(ShellState, cancellationToken);
        }
    }
}
