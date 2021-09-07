// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System;

namespace Microsoft.Repl.ConsoleHandling
{
    public class Reporter : IWritable
    {
        private static readonly Reporter NullReporter = new Reporter(null);
        private static readonly object Sync = new object();

        private readonly AnsiConsole _console;

        static Reporter()
        {
            Reset();
        }

        private Reporter(AnsiConsole console)
        {
            _console = console;
        }

        public static Reporter Output { get; private set; }
        public static Reporter Error { get; private set; }
        public static Reporter Verbose { get; private set; }

        /// <summary>
        /// Resets the Reporters to write to the current Console Out/Error.
        /// </summary>
        public static void Reset()
        {
            lock (Sync)
            {
                Output = new Reporter(AnsiConsole.GetOutput());
                Error = new Reporter(AnsiConsole.GetError());
                Verbose = IsVerbose ?
                    new Reporter(AnsiConsole.GetOutput()) :
                    NullReporter;
            }
        }

        public void WriteLine(string s)
        {
            if (s is null)
            {
                return;
            }

            lock (Sync)
            {
                if (ShouldPassAnsiCodesThrough)
                {
                    _console?.Writer?.WriteLine(s);
                }
                else
                {
                    _console?.WriteLine(s);
                }
            }
        }

        public void WriteLine()
        {
            lock (Sync)
            {
                _console?.Writer?.WriteLine();
            }
        }

        public void Write(char c)
        {
            lock (Sync)
            {
                if (ShouldPassAnsiCodesThrough)
                {
                    _console?.Writer?.Write(c);
                }
                else
                {
                    _console?.Write(c);
                }
            }
        }

        public void Write(string s)
        {
            lock (Sync)
            {
                if (ShouldPassAnsiCodesThrough)
                {
                    _console?.Writer?.Write(s);
                }
                else
                {
                    _console?.Write(s);
                }
            }
        }

        private static bool IsVerbose => bool.TryParse(Environment.GetEnvironmentVariable("DOTNET_CLI_CONTEXT_VERBOSE") ?? "false", out bool value) && value;

        private static bool ShouldPassAnsiCodesThrough => bool.TryParse(Environment.GetEnvironmentVariable("DOTNET_CLI_CONTEXT_ANSI_PASS_THRU") ?? "false", out bool value) && value;

        private bool _isCaretVisible = true;

        public bool IsCaretVisible
        {
            get => _isCaretVisible;
            set
            {
                Console.CursorVisible = value;
                _isCaretVisible = value;
            }
        }
    }
}
