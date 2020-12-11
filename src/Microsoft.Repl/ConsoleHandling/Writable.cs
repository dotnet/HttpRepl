// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Repl.ConsoleHandling
{
    internal class Writable : IWritable
    {
        private readonly Reporter _reporter;

        public Writable(Reporter reporter)
        {
            _reporter = reporter;
        }

        public bool IsCaretVisible
        {
            get => _reporter.IsCaretVisible;
            set => _reporter.IsCaretVisible = value;
        }

        public void Write(char c)
        {
            _reporter.Write(c);
        }

        public void Write(string s)
        {
            _reporter.Write(s);
        }

        public void WriteLine()
        {
            _reporter.WriteLine();
        }

        public void WriteLine(string s)
        {
            _reporter.WriteLine(s);
        }
    }
}
