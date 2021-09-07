// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System;
using System.Threading;

namespace Microsoft.Repl.ConsoleHandling
{
    public interface IConsoleManager : IWritable
    {
        Point Caret { get; }

#pragma warning disable CA1716 // Identifiers should not match keywords
        IWritable Error { get; }
#pragma warning restore CA1716 // Identifiers should not match keywords

        bool IsKeyAvailable { get; }

        void Clear();

        void MoveCaret(int positions);

        ConsoleKeyInfo ReadKey(CancellationToken cancellationToken);

        IDisposable AddBreakHandler(Action onBreak);

        bool AllowOutputRedirection { get; }
    }
}
