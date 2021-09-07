// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

namespace Microsoft.Repl.ConsoleHandling
{
    public interface IWritable
    {
        void Write(char c);

        void Write(string s);

        void WriteLine();

        void WriteLine(string s);

        bool IsCaretVisible { get; set; }
    }
}
