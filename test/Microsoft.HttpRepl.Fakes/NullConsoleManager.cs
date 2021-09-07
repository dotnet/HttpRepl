// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

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
