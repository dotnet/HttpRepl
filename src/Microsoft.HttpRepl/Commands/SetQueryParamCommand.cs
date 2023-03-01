// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Resources;
using Microsoft.HttpRepl.Telemetry;
using Microsoft.HttpRepl.Telemetry.Events;
using Microsoft.Repl;
using Microsoft.Repl.Commanding;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Parsing;

namespace Microsoft.HttpRepl.Commands
{
    public class SetQueryParamCommand : ICommand<HttpState, ICoreParseResult>
    {
        private const string CommandName = "set";
        private const string SubCommand = "query-param";

        private readonly ITelemetry _telemetry;

        public string Name => "setQueryParam";
        public static string Description => Strings.SetQueryParamCommand_HelpDetails;

        public SetQueryParamCommand(ITelemetry telemetry)
        {
            _telemetry = telemetry;
        }

        public bool? CanHandle(IShellState shellState, HttpState programState, ICoreParseResult parseResult)
        {
            return parseResult.ContainsAtLeast(minimumLength: 3, CommandName, SubCommand)
                ? (bool?)true
                : null;
        }

        public Task ExecuteAsync(IShellState shellState, HttpState programState, ICoreParseResult parseResult, CancellationToken cancellationToken)
        {
            parseResult = parseResult ?? throw new ArgumentNullException(nameof(parseResult));

            programState = programState ?? throw new ArgumentNullException(nameof(programState));

            bool isValueEmpty;
            if (parseResult.Sections.Count == 3)
            {
                programState.QueryParam.Remove(parseResult.Sections[2]);
                isValueEmpty = true;
            }
            else
            {
                programState.QueryParam[parseResult.Sections[2]] = parseResult.Sections.Skip(3);
                isValueEmpty = false;
            }

            _telemetry.TrackEvent(new SetQueryParamEvent(parseResult.Sections[2], isValueEmpty));

            return Task.CompletedTask;
        }

        public string GetHelpDetails(IShellState shellState, HttpState programState, ICoreParseResult parseResult)
        {
            if (parseResult.ContainsAtLeast(CommandName, SubCommand))
            {
                StringBuilder helpText = new StringBuilder();
                helpText.Append(Strings.Usage.Bold());
                helpText.AppendLine("set query-param {name} [value]");
                helpText.AppendLine();
                helpText.AppendLine(Strings.SetQueryParamCommand_HelpDetails);
                return helpText.ToString();
            }

            return null;
        }

        public string GetHelpSummary(IShellState shellState, HttpState programState)
        {
            return Description;
        }

        public IEnumerable<string> Suggest(IShellState shellState, HttpState programState, ICoreParseResult parseResult)
        {
            return null;
        }
    }
}
