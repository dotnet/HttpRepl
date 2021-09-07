// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Repl.Scripting
{
    public interface IScriptExecutor
    {
        Task ExecuteScriptAsync(IShellState shellState, IEnumerable<string> commandTexts, CancellationToken cancellationToken);
    }
}
