// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using Microsoft.Repl.Commanding;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Input;
using Microsoft.Repl.Suggestions;

namespace Microsoft.Repl
{
    public interface IShellState
    {
        IInputManager InputManager { get; }

        ICommandHistory CommandHistory { get; }

        IConsoleManager ConsoleManager { get; }

        ICommandDispatcher CommandDispatcher { get; }

        ISuggestionManager SuggestionManager { get; }

        bool IsExiting { get; set; }

        void MoveCarets(int positions);
    }
}
