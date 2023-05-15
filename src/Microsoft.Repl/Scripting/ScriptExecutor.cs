// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Repl.Commanding;
using Microsoft.Repl.Parsing;

namespace Microsoft.Repl.Scripting
{
    public class ScriptExecutor<TProgramState, TParseResult> : IScriptExecutor
        where TParseResult : ICoreParseResult
    {
        private readonly bool _hideScriptLinesFromHistory;

        public ScriptExecutor(bool hideScriptLinesFromHistory = true)
        {
            _hideScriptLinesFromHistory = hideScriptLinesFromHistory;
        }

        public async Task ExecuteScriptAsync(IShellState shellState, IEnumerable<string> commandTexts, CancellationToken cancellationToken)
        {
            shellState = shellState ?? throw new ArgumentNullException(nameof(shellState));

            commandTexts = commandTexts ?? throw new ArgumentNullException(nameof(commandTexts));

            if (shellState.CommandDispatcher is ICommandDispatcher<TProgramState, TParseResult> dispatcher)
            {
                IDisposable suppressor = _hideScriptLinesFromHistory ? shellState.CommandHistory.SuspendHistory() : null;

                using (suppressor)
                {

                    shellState.ScriptManager.NumberOfRequests = commandTexts.Count();
                    foreach (string commandText in commandTexts)
                    {
                        if (string.IsNullOrWhiteSpace(commandText))
                        {
                            continue;
                        }

                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        dispatcher.OnReady(shellState);
                        shellState.InputManager.SetInput(shellState, commandText);
                        await dispatcher.ExecuteCommandAsync(shellState, cancellationToken).ConfigureAwait(false);
                    }
                    if(shellState.ScriptManager.IsActive)
                    {
                        //shellState.ScriptManager.Statuses;
                    }
                }
            }
        }
    }
}
