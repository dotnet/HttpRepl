// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Repl.Input
{
    public interface IInputManager
    {
        bool IsOverwriteMode { get; set; }

        int CaretPosition { get; }

        IInputManager RegisterKeyHandler(ConsoleKey key, AsyncKeyPressHandler handler);

        IInputManager RegisterKeyHandler(ConsoleKey key, ConsoleModifiers modifiers, AsyncKeyPressHandler handler);

        void ResetInput();

        Task StartAsync(IShellState state, CancellationToken cancellationToken);

        void SetInput(IShellState state, string input);

        string GetCurrentBuffer();

        void RemovePreviousCharacter(IShellState state);

        void RemoveCurrentCharacter(IShellState state);

        void Clear(IShellState state);

        void MoveCaret(int positions);
    }
}
