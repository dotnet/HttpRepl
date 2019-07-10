using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.Repl.ConsoleHandling;
using Moq;

namespace Microsoft.HttpRepl.IntegrationTests.Mocks
{
    public class ConsoleManagerStub : IConsoleManager
    {
        public Point Caret => default;

        public Point CommandStart => default;

        public int CaretPosition => default;

        public IWritable Error => default;

        public bool IsKeyAvailable => default;

        public bool AllowOutputRedirection => default;

        public bool IsCaretVisible { get => default; set { } }

        public IDisposable AddBreakHandler(Action onBreak) => default;


        public void Clear() { }

        public void MoveCaret(int positions) { }

        public ConsoleKeyInfo ReadKey(CancellationToken cancellationToken) => default;

        public void ResetCommandStart() { }

        public void Write(char c) { }

        public void Write(string s) { }

        public void WriteLine() { }

        public void WriteLine(string s) { }
    }
}
