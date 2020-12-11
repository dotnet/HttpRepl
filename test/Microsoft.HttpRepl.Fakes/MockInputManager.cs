// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
