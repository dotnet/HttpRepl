// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using System.Threading;
using Microsoft.Repl.ConsoleHandling;

namespace Microsoft.HttpRepl.Fakes
{
    public class MockConsoleManager : IConsoleManager
    {
        private CancellationTokenSource _cancellationTokenSource;
        private ConsoleKeyInfo _consoleKeyInfo;
        private StringBuilder _outputTracking = new StringBuilder();

        public MockConsoleManager(ConsoleKeyInfo consoleKeyInfo, CancellationTokenSource cancellationTokenSource)
        {
            _cancellationTokenSource = cancellationTokenSource;
            _consoleKeyInfo = consoleKeyInfo;
        }

        public MockConsoleManager()
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public string Output => _outputTracking.ToString();

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

        public void Write(char c) => _outputTracking.Append(c);

        public void Write(string s) => _outputTracking.Append(s);

        public void WriteLine() => _outputTracking.AppendLine();

        public void WriteLine(string s) => _outputTracking.AppendLine(s);

        public bool IsCaretVisible { get => true; }

        bool IWritable.IsCaretVisible { get => true; set => value = true; }

        public bool AllowOutputRedirection => true;
    }
}
