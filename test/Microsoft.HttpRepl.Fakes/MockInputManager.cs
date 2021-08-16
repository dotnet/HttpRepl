// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Repl;
using Microsoft.Repl.Input;

namespace Microsoft.HttpRepl.Fakes
{
    public class MockInputManager : IInputManager
    {
        private string _inputBuffer;

        public int CaretPosition { get; private set; }

        public MockInputManager(string inputBuffer)
        {
            _inputBuffer = inputBuffer;
        }

        public void MoveCaret(int positions)
        {

        }

        public bool IsOverwriteMode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IInputManager RegisterKeyHandler(ConsoleKey key, AsyncKeyPressHandler handler)
        {
            return new InputManager();
        }

        public IInputManager RegisterKeyHandler(ConsoleKey key, ConsoleModifiers modifiers, AsyncKeyPressHandler handler)
        {
            return new InputManager();
        }

        public void ResetInput()
        {
        }

        public Task StartAsync(IShellState state, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void SetInput(IShellState state, string input)
        {
            _inputBuffer = input;
        }

        public string GetCurrentBuffer()
        {
            return _inputBuffer;
        }

        public void RemovePreviousCharacter(IShellState state)
        {
        }

        public void RemoveCurrentCharacter(IShellState state)
        {
        }

        public void Clear(IShellState state)
        {
        }
    }
}
