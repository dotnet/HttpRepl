// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Repl.Parsing;

namespace Microsoft.Repl.Commanding
{
    public interface ICommandDispatcher
    {
        IParser Parser { get; }

        IReadOnlyList<string> CollectSuggestions(IShellState shellState);

        void OnReady(IShellState shellState);

        Task ExecuteCommandAsync(IShellState shellState, CancellationToken cancellationToken);
    }

    public interface ICommandDispatcher<in TProgramState, in TParseResult> : ICommandDispatcher
        where TParseResult : ICoreParseResult
    {
        IEnumerable<ICommand<TProgramState, TParseResult>> Commands { get; }
    }
}
