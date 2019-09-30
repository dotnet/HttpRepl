// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.FileSystem;
using Microsoft.HttpRepl.Resources;
using Microsoft.Repl;
using Microsoft.Repl.Commanding;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Parsing;
using Microsoft.Repl.Scripting;
using Microsoft.Repl.Suggestions;

namespace Microsoft.HttpRepl.Commands
{
    public class RunCommand : ICommand<HttpState, ICoreParseResult>
    {
        private static readonly string Name = "run";

        private IFileSystem _fileSystem;
        public RunCommand(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public bool? CanHandle(IShellState shellState, HttpState programState, ICoreParseResult parseResult)
        {
            parseResult = parseResult ?? throw new ArgumentNullException(nameof(parseResult));

            return parseResult.ContainsAtLeast(minimumLength: 2, Name) && parseResult.Sections.Count < 4
                ? (bool?)true
                : null;
        }

        public async Task ExecuteAsync(IShellState shellState, HttpState programState, ICoreParseResult parseResult, CancellationToken cancellationToken)
        {
            parseResult = parseResult ?? throw new ArgumentNullException(nameof(parseResult));

            shellState = shellState ?? throw new ArgumentNullException(nameof(shellState));

            if (!_fileSystem.FileExists(parseResult.Sections[1]))
            {
                shellState.ConsoleManager.Error.WriteLine(String.Format(Strings.RunCommand_CouldNotFindScriptFile, parseResult.Sections[1]));
                return;
            }

            bool suppressScriptLinesInHistory = true;
            if (parseResult.Sections.Count == 3)
            {
                suppressScriptLinesInHistory = !string.Equals(parseResult.Sections[2], "+history", StringComparison.OrdinalIgnoreCase);
            }

            string[] lines = _fileSystem.ReadAllLinesFromFile(parseResult.Sections[1]);
            IScriptExecutor scriptExecutor = new ScriptExecutor<HttpState, ICoreParseResult>(suppressScriptLinesInHistory);
            await scriptExecutor.ExecuteScriptAsync(shellState, lines, cancellationToken).ConfigureAwait(false);
        }

        public string GetHelpDetails(IShellState shellState, HttpState programState, ICoreParseResult parseResult)
        {
            if (parseResult.ContainsAtLeast(Name))
            {
                var helpText = new StringBuilder();
                helpText.Append(Strings.Usage.Bold());
                helpText.AppendLine(Strings.RunCommand_HelpDetails);
                return helpText.ToString();
            }

            return null;
        }

        public string GetHelpSummary(IShellState shellState, HttpState programState)
        {
            return Resources.Strings.RunCommand_HelpSummary;
        }

        public IEnumerable<string> Suggest(IShellState shellState, HttpState programState, ICoreParseResult parseResult)
        {
            parseResult = parseResult ?? throw new ArgumentNullException(nameof(parseResult));

            if (parseResult.SelectedSection == 0 &&
                (string.IsNullOrEmpty(parseResult.Sections[parseResult.SelectedSection]) || Name.StartsWith(parseResult.Sections[0].Substring(0, parseResult.CaretPositionWithinSelectedSection), StringComparison.OrdinalIgnoreCase)))
            {
                return new[] { Name };
            }

            if (parseResult.SelectedSection == 1 && string.Equals(parseResult.Sections[0], Name, StringComparison.OrdinalIgnoreCase))
            {
                return FileSystemCompletion.GetCompletions(parseResult.Sections[1].Substring(0, parseResult.CaretPositionWithinSelectedSection));
            }

            return null;
        }
    }
}
