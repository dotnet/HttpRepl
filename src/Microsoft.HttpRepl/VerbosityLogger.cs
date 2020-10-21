// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#nullable enable

using Microsoft.Repl.ConsoleHandling;

namespace Microsoft.HttpRepl
{
    internal class VerbosityLogger
    {
        private readonly bool _isVerbosityEnabled;
        private readonly IConsoleManager? _consoleManager;

        private VerbosityLogger(IConsoleManager? consoleManager, bool isVerbosityEnabled)
        {
            _consoleManager = consoleManager;
            _isVerbosityEnabled = isVerbosityEnabled;
        }

        public static VerbosityLogger FromConsoleManager(IConsoleManager? consoleManager, bool isVerbosityEnabled = false)
        {
            return new VerbosityLogger(consoleManager, isVerbosityEnabled);
        }

        public void Write(string s) => _consoleManager?.Write(s);
        public void Write(string s, params object[] args) => _consoleManager?.Write(string.Format(s, args));
        public void WriteLine(string s) => _consoleManager?.WriteLine(s);
        public void WriteLine(string s, params object[] args) => _consoleManager?.WriteLine(string.Format(s, args));
        public void WriteLine() => _consoleManager?.WriteLine();

        public void WriteVerbose(string s)
        {
            if (_isVerbosityEnabled)
            {
                Write(s);
            }
        }

        public void WriteVerbose(string s, params object[] args)
        {
            if (_isVerbosityEnabled)
            {
                Write(s, args);
            }
        }

        public void WriteLineVerbose(string s)
        {
            if (_isVerbosityEnabled)
            {
                WriteLine(s);
            }
        }

        public void WriteLineVerbose(string s, params object[] args)
        {
            if (_isVerbosityEnabled)
            {
                WriteLine(s, args);
            }
        }

        public void WriteLineVerbose()
        {
            if (_isVerbosityEnabled)
            {
                WriteLine();
            }
        }
    }
}
