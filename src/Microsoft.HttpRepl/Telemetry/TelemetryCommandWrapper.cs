// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Telemetry.Events;
using Microsoft.Repl;
using Microsoft.Repl.Commanding;
using Microsoft.Repl.Parsing;

namespace Microsoft.HttpRepl.Telemetry
{
    internal class TelemetryCommandWrapper : ICommand<HttpState, ICoreParseResult>
    {
        private readonly ICommand<HttpState, ICoreParseResult> _command;
        private readonly ITelemetry _telemetry;

        public string Name => _command.Name;

        public ICommand<HttpState, ICoreParseResult> Command => _command;

        public TelemetryCommandWrapper(ITelemetry telemetry, ICommand<HttpState, ICoreParseResult> command)
        {
            _telemetry = telemetry;
            _command = command;
        }

        public bool? CanHandle(IShellState shellState, HttpState programState, ICoreParseResult parseResult)
        {
            return _command.CanHandle(shellState, programState, parseResult);
        }

        public Task ExecuteAsync(IShellState shellState, HttpState programState, ICoreParseResult parseResult, CancellationToken cancellationToken)
        {
            bool wasSuccessful = true;
            try
            {
                return _command.ExecuteAsync(shellState, programState, parseResult, cancellationToken);
            }
            catch
            {
                wasSuccessful = false;
                throw;
            }
            finally
            {
                _telemetry.TrackEvent(new CommandExecutedEvent(_command.Name, wasSuccessful));
            }
        }

        public string GetHelpDetails(IShellState shellState, HttpState programState, ICoreParseResult parseResult)
        {
            return _command.GetHelpDetails(shellState, programState, parseResult);
        }

        public string GetHelpSummary(IShellState shellState, HttpState programState)
        {
            return _command.GetHelpSummary(shellState, programState);
        }

        public IEnumerable<string> Suggest(IShellState shellState, HttpState programState, ICoreParseResult parseResult)
        {
            return _command.Suggest(shellState, programState, parseResult);
        }
    }
}
