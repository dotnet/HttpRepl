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
    public class ClearQueryParamCommand : ICommand<HttpState, ICoreParseResult>
    {
        public string Name => "clearQueryParam";
        private const string CommandName = "clear";
        private const string SubCommand = "query-param";

        public static string Description => Strings.ClearQueryParamCommand_HelpDetails;

        private readonly ITelemetry _telemetry;

        public ClearQueryParamCommand(ITelemetry telemetry) {
            _telemetry = telemetry;
        }

        public bool? CanHandle(IShellState shellState, HttpState programState, ICoreParseResult parseResult) =>
            parseResult.ContainsAtLeast(minimumLength: 2, CommandName, SubCommand)
                ? (bool?)true
                : null;

        public Task ExecuteAsync(IShellState shellState, HttpState programState, ICoreParseResult parseResult, CancellationToken cancellationToken)
        {
            parseResult = parseResult ?? throw new ArgumentNullException(nameof(parseResult));

            programState = programState ?? throw new ArgumentNullException(nameof(programState));

            int sectionCount = parseResult.Sections.Count;
            bool isValueEmpty;
            if (sectionCount == 2)
            {
                programState.QueryParam.Clear();
                isValueEmpty = true;
            } else
            {
                isValueEmpty = false;
            }

            _telemetry.TrackEvent(new ClearQueryParamEvent(parseResult.Sections[1], isValueEmpty));
            return Task.CompletedTask;
        }

        public string GetHelpDetails(IShellState shellState, HttpState programState, ICoreParseResult parseResult)
        {
            if (parseResult.ContainsAtLeast(CommandName, SubCommand))
            {
                StringBuilder helpText = new StringBuilder();
                helpText.Append(Strings.Usage.Bold());
                helpText.AppendLine("clear query-param");
                helpText.AppendLine();
                helpText.AppendLine(Strings.ClearQueryParamCommand_HelpDetails);
                return helpText.ToString();
            }

            return null;
        }

        public string GetHelpSummary(IShellState shellState, HttpState programState) => Description;

        public IEnumerable<string> Suggest(IShellState shellState, HttpState programState, ICoreParseResult parseResult) => null;
    }
}
