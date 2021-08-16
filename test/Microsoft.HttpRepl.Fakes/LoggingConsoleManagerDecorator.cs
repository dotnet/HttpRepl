// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System;
using System.Text;
using System.Threading;
using Microsoft.Repl.ConsoleHandling;

namespace Microsoft.HttpRepl.Fakes
{
    public class LoggingConsoleManagerDecorator : IConsoleManager
    {
        private readonly IConsoleManager _baseConsole;
        private readonly StringBuilder _log;

        public LoggingConsoleManagerDecorator(IConsoleManager console)
        {
            _baseConsole = console;
            _log = new StringBuilder();
        }

        public string LoggedOutput => _log.ToString();
        public bool WasClearCalled { get; private set; }

        #region IConsoleManager
        public Point Caret => _baseConsole.Caret;

        public IWritable Error => _baseConsole.Error;

        public bool IsKeyAvailable => _baseConsole.IsKeyAvailable;

        public bool IsCaretVisible { get => _baseConsole.IsCaretVisible; set => _baseConsole.IsCaretVisible = value; }

        public bool AllowOutputRedirection => _baseConsole.AllowOutputRedirection;

        public IDisposable AddBreakHandler(Action onBreak)
        {
            return _baseConsole.AddBreakHandler(onBreak);
        }

        public void Clear()
        {
            _baseConsole.Clear();
            WasClearCalled = true;
        }

        public void MoveCaret(int positions)
        {
            _baseConsole.MoveCaret(positions);
        }

        public ConsoleKeyInfo ReadKey(CancellationToken cancellationToken)
        {
            return _baseConsole.ReadKey(cancellationToken);
        }

        public void Write(char c)
        {
            _log.Append(c);
            _baseConsole.Write(c);
        }

        public void Write(string s)
        {
            _log.Append(s);
            _baseConsole.Write(s);
        }

        public void WriteLine()
        {
            _log.AppendLine();
            _baseConsole.WriteLine();
        }

        public void WriteLine(string s)
        {
            _log.AppendLine(s);
            _baseConsole.WriteLine(s);
        }
        #endregion
    }
}
