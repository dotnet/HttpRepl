// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
