// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Repl;
using Microsoft.Repl.Commanding;
using Microsoft.Repl.Parsing;

namespace Microsoft.HttpRepl.Commands
{
    public class ClearCommand : ICommand<object, ICoreParseResult>
    {
        private static readonly string Name = "clear";
        private static readonly string AlternateName = "cls";

        public bool? CanHandle(IShellState shellState, object programState, ICoreParseResult parseResult)
        {
            return parseResult.ContainsExactly(Name) || parseResult.ContainsExactly(AlternateName)
                ? (bool?) true
                : null;
        }

        public Task ExecuteAsync(IShellState shellState, object programState, ICoreParseResult parseResult, CancellationToken cancellationToken)
        {
            shellState = shellState ?? throw new ArgumentNullException(nameof(shellState));

            shellState.ConsoleManager.Clear();
            shellState.CommandDispatcher.OnReady(shellState);
            return Task.CompletedTask;
        }

        public string GetHelpDetails(IShellState shellState, object programState, ICoreParseResult parseResult)
        {
            if (parseResult.ContainsExactly(Name) || parseResult.ContainsExactly(AlternateName))
            {
                return "Clears the shell";
            }

            return null;
        }

        public string GetHelpSummary(IShellState shellState, object programState)
        {
            return Resources.Strings.ClearCommand_HelpSummary;
        }

        public IEnumerable<string> Suggest(IShellState shellState, object programState, ICoreParseResult parseResult)
        {
            parseResult = parseResult ?? throw new ArgumentNullException(nameof(parseResult));

            if (parseResult.SelectedSection == 0)
            {
                bool nameMatch = string.IsNullOrEmpty(parseResult.Sections[parseResult.SelectedSection]) || Name.StartsWith(parseResult.Sections[0].Substring(0, parseResult.CaretPositionWithinSelectedSection), StringComparison.OrdinalIgnoreCase);
                bool alternateNameMatch = string.IsNullOrEmpty(parseResult.Sections[parseResult.SelectedSection]) || AlternateName.StartsWith(parseResult.Sections[0].Substring(0, parseResult.CaretPositionWithinSelectedSection), StringComparison.OrdinalIgnoreCase);

                if (nameMatch && alternateNameMatch)
                {
                    return new[] { Name, AlternateName };
                }
                else if (nameMatch)
                {
                    return new[] { Name };
                }
                else if (alternateNameMatch)
                {
                    return new[] { AlternateName };
                }
            }

            return null;
        }
    }
}
