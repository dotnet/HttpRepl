// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

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
