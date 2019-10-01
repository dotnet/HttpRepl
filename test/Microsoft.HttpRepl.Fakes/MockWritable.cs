// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Repl.ConsoleHandling;

namespace Microsoft.HttpRepl.Fakes
{
    internal class MockWritable : IWritable
    {
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

        public bool IsCaretVisible { get => true; set => value = true; }
    }
}
