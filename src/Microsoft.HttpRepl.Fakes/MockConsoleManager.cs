// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using Microsoft.Repl.ConsoleHandling;

namespace Microsoft.HttpRepl.Fakes
{
    public class MockConsoleManager : IConsoleManager
    {
        private CancellationTokenSource _cancellationTokenSource;
        private ConsoleKeyInfo _consoleKeyInfo;

        public MockConsoleManager(ConsoleKeyInfo consoleKeyInfo, CancellationTokenSource cancellationTokenSource)
        {
            _cancellationTokenSource = cancellationTokenSource;
            _consoleKeyInfo = consoleKeyInfo;
        }

        public Point Caret => throw new NotImplementedException();

        public Point CommandStart => throw new NotImplementedException();

        public int CaretPosition { get; set; }

        public IWritable Error => new MockWritable();

        public bool IsKeyAvailable => throw new NotImplementedException();

        public void Clear()
        {
        }

        public void MoveCaret(int offset)
        {
            CaretPosition += offset;
        }

        public ConsoleKeyInfo ReadKey(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();
            return _consoleKeyInfo;
        }

        public void ResetCommandStart()
        {
        }

        public IDisposable AddBreakHandler(Action onBreak)
        {
            return null;
        }

        public void Write(char c)
        {
        }

        public void Write(string s)
        {
        }

        public void WriteLine()
        {
        }

        public void WriteLine(string s)
        {
        }

        public bool IsCaretVisible { get => true; }

        bool IWritable.IsCaretVisible { get => true; set => value = true; }

        public bool AllowOutputRedirection => true;
    }
}
