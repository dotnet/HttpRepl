// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Resources;
using Microsoft.HttpRepl.Suggestions;
using Microsoft.HttpRepl.Telemetry;
using Microsoft.HttpRepl.Telemetry.Events;
using Microsoft.Repl;
using Microsoft.Repl.Commanding;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Parsing;

namespace Microsoft.HttpRepl.Commands
{
    public class SetQueryStringCommand : ICommand<HttpState, ICoreParseResult>
    {
        private const string CommandName = "set";
        private const string SubCommand = "queryString";

        private readonly ITelemetry _telemetry;

        public string Name => "setQueryString";
        public static string Description => Strings.SetQueryStringCommand_HelpDetails;

        public SetQueryStringCommand(ITelemetry telemetry)
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
                programState.QueryString.Remove(parseResult.Sections[2]);
                isValueEmpty = true;
            }
            else
            {
                programState.QueryString[parseResult.Sections[2]] = parseResult.Sections.Skip(3);
                isValueEmpty = false;
            }

            _telemetry.TrackEvent(new SetQueryStringEvent(parseResult.Sections[2], isValueEmpty));

            return Task.CompletedTask;
        }

        public string GetHelpDetails(IShellState shellState, HttpState programState, ICoreParseResult parseResult)
        {
            if (parseResult.ContainsAtLeast(CommandName, SubCommand))
            {
                StringBuilder helpText = new StringBuilder();
                helpText.Append(Strings.Usage.Bold());
                helpText.AppendLine("set queryString {name} [value]");
                helpText.AppendLine();
                helpText.AppendLine(Strings.SetQueryStringCommand_HelpDetails);
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
