// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using Microsoft.Repl.ConsoleHandling;

namespace Microsoft.HttpRepl.Fakes
{
    public class NullConsoleManager : IConsoleManager
    {
        public Point Caret => default(Point);

        public Point CommandStart => default(Point);

        public int CaretPosition => default(int);

        public IWritable Error => default(IWritable);

        public bool IsKeyAvailable => default;

        public bool IsCaretVisible { get => default; set => _ = value; }

        public bool AllowOutputRedirection => true;

        public IDisposable AddBreakHandler(Action onBreak)
        {
            return default;
        }

        public void Clear()
        {
        }

        public void MoveCaret(int positions)
        {
        }

        public ConsoleKeyInfo ReadKey(CancellationToken cancellationToken)
        {
            return default;
        }

        public void ResetCommandStart()
        {
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
    }
}
