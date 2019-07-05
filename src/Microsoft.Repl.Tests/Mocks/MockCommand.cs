using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Repl.Commanding;
using Microsoft.Repl.Parsing;

namespace Microsoft.Repl.Tests.Mocks
{
    public class MockCommand : ICommand<object, ICoreParseResult>
    {
        private string _commandName;

        public MockCommand(string commandName)
        {
            _commandName = commandName;
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

        public IEnumerable<string> Suggest(IShellState shellState, object programState, ICoreParseResult parseResult)
        {
            return new[] { _commandName };
        }
    }
}
