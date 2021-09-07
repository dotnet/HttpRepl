// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Repl;
using Microsoft.Repl.Commanding;
using Microsoft.Repl.Parsing;

namespace Microsoft.HttpRepl.Fakes
{
    public class MockCommand : ICommand<object, ICoreParseResult>
    {
        public string Name { get; }

        public MockCommand(string commandName)
        {
            Name = commandName;
        }

        public bool? CanHandle(IShellState shellState, object programState, ICoreParseResult parseResult)
        {
            return (bool?)true;
        }

        public Task ExecuteAsync(IShellState shellState, object programState, ICoreParseResult parseResult, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public string GetHelpDetails(IShellState shellState, object programState, ICoreParseResult parseResult)
        {
            return null;
        }

        public string GetHelpSummary(IShellState shellState, object programState)
        {
            return null;
        }

        public IEnumerable<string> Suggest(IShellState shellState, object programState, ICoreParseResult parseResult)
        {
            return new[] { Name };
        }
    }
}
