// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Repl.Parsing;

namespace Microsoft.Repl.Commanding
{
    public abstract class CommandWithStructuredInputBase<TProgramState, TParseResult> : ICommand<TProgramState, TParseResult>
        where TParseResult : ICoreParseResult
    {
        public abstract string Name { get; }

        public abstract string GetHelpSummary(IShellState shellState, TProgramState programState);

        public string GetHelpDetails(IShellState shellState, TProgramState programState, TParseResult parseResult)
        {
            if (!DefaultCommandInput<TParseResult>.TryProcess(InputSpec, parseResult, out DefaultCommandInput<TParseResult> commandInput, out IReadOnlyList<CommandInputProcessingIssue> processingIssues)
                && processingIssues.Any(x => x.Kind == CommandInputProcessingIssueKind.CommandMismatch))
            {
                //If this is the right command, just not the right syntax, report the usage errors
                return null;
            }

            return GetHelpDetails(shellState, programState, commandInput, parseResult);
        }

        protected abstract string GetHelpDetails(IShellState shellState, TProgramState programState, DefaultCommandInput<TParseResult> commandInput, TParseResult parseResult);

        public IEnumerable<string> Suggest(IShellState shellState, TProgramState programState, TParseResult parseResult)
        {
            DefaultCommandInput<TParseResult>.TryProcess(InputSpec, parseResult, out DefaultCommandInput<TParseResult> commandInput, out IReadOnlyList<CommandInputProcessingIssue> _);

            string normalCompletionString = parseResult.SelectedSection == parseResult.Sections.Count
                ? string.Empty
                : parseResult.Sections[parseResult.SelectedSection].Substring(0, parseResult.CaretPositionWithinSelectedSection);


            // Check the various command name permutations to see if we might be completing one of them
            IReadOnlyList<string> commandName = null;
            for (int j = 0; j < InputSpec.CommandName.Count; ++j)
            {
                IReadOnlyList<string> currentCommandNameParts = InputSpec.CommandName[j];

                //If we're completing in a name position, offer completion for the command name
                if (parseResult.SelectedSection < currentCommandNameParts.Count)
                {
                    bool success = true;
                    for (int i = 0; i < parseResult.SelectedSection; ++i)
                    {
                        string currentCommandNamePart = currentCommandNameParts[i];
                        string currentParseSection = parseResult.Sections[i];
                        if (!string.Equals(currentCommandNamePart, currentParseSection, StringComparison.OrdinalIgnoreCase))
                        {
                            success = false;
                            break;
                        }
                    }

                    if (success)
                    {
                        commandName = currentCommandNameParts;
                        break;
                    }
                }
            }

            if (commandName?.Count > parseResult.SelectedSection && commandName[parseResult.SelectedSection].StartsWith(normalCompletionString, StringComparison.OrdinalIgnoreCase))
            {
                return new[] { commandName[parseResult.SelectedSection] };
            }

            if (commandInput is null)
            {
                return null;
            }

            if (normalCompletionString.StartsWith(InputSpec.OptionPreamble.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return GetOptionCompletions(commandInput, normalCompletionString);
            }

            IEnumerable<string> completions = Enumerable.Empty<string>();
            CommandInputLocation? inputLocation = commandInput.SelectedElement?.Location;

            if (inputLocation != CommandInputLocation.OptionValue && commandInput.Arguments.Count < InputSpec.MaximumArguments)
            {
                IEnumerable<string> results = GetArgumentSuggestionsForText(shellState, programState, parseResult, commandInput, normalCompletionString);

                if (results != null)
                {
                    completions = results;
                }
            }

            switch (inputLocation)
            {
                case CommandInputLocation.OptionName:
                {
                    IEnumerable<string> results = GetOptionCompletions(commandInput, normalCompletionString);

                    if (results != null)
                    {
                        completions = completions.Union(results);
                    }

                    break;
                }
                case CommandInputLocation.OptionValue:
                {
                    IEnumerable<string> results = GetOptionValueCompletions(shellState, programState, commandInput.SelectedElement.Owner.NormalizedText, commandInput, parseResult, normalCompletionString);

                    if (results != null)
                    {
                        completions = completions.Union(results);
                    }

                    break;
                }
                case CommandInputLocation.Argument:
                {
                    IEnumerable<string> argumentResults = GetArgumentSuggestionsForText(shellState, programState, parseResult, commandInput, normalCompletionString);

                    if (argumentResults != null)
                    {
                        completions = completions.Union(argumentResults);
                    }

                    if (string.IsNullOrEmpty(normalCompletionString))
                    {
                        IEnumerable<string> results = GetOptionCompletions(commandInput, normalCompletionString);

                        if (results != null)
                        {
                            completions = completions.Union(results);
                        }
                    }

                    break;
                }
            }

            return completions;
        }

        protected virtual IEnumerable<string> GetOptionValueCompletions(IShellState shellState, TProgramState programState, string optionId, DefaultCommandInput<TParseResult> commandInput, TParseResult parseResult, string normalizedCompletionText)
        {
            return null;
        }

        protected virtual IEnumerable<string> GetArgumentSuggestionsForText(IShellState shellState, TProgramState programState, TParseResult parseResult, DefaultCommandInput<TParseResult> commandInput, string normalCompletionString)
        {
            return null;
        }

        private IEnumerable<string> GetOptionCompletions(DefaultCommandInput<TParseResult> commandInput, string normalCompletionString)
        {
            return InputSpec.Options.Where(x => commandInput.Options[x.Id].Count < x.MaximumOccurrences)
                .SelectMany(x => x.Forms)
                .Where(x => x.StartsWith(normalCompletionString, StringComparison.OrdinalIgnoreCase));
        }

        public bool? CanHandle(IShellState shellState, TProgramState programState, TParseResult parseResult)
        {
            shellState = shellState ?? throw new ArgumentNullException(nameof(shellState));

            if (!DefaultCommandInput<TParseResult>.TryProcess(InputSpec, parseResult, out DefaultCommandInput<TParseResult> commandInput, out IReadOnlyList<CommandInputProcessingIssue> processingIssues))
            {
                //If this is the right command, just not the right syntax, report the usage errors
                if (processingIssues.All(x => x.Kind != CommandInputProcessingIssueKind.CommandMismatch))
                {
                    foreach (CommandInputProcessingIssue issue in processingIssues)
                    {
                        shellState.ConsoleManager.Error.WriteLine(GetStringForIssue(issue));
                    }

                    string help = GetHelpDetails(shellState, programState, parseResult);
                    shellState.ConsoleManager.WriteLine(help);
                    return false;
                }

                //If there was a mismatch in the command name, this isn't our input to handle
                return null;
            }

            return CanHandle(shellState, programState, commandInput);
        }

        protected virtual bool CanHandle(IShellState shellState, TProgramState programState, DefaultCommandInput<TParseResult> commandInput)
        {
            return true;
        }

        protected virtual string GetStringForIssue(CommandInputProcessingIssue issue)
        {
            issue = issue ?? throw new ArgumentNullException(nameof(issue));

            //TODO: Make this nicer
            return issue.Kind + " -- " + issue.Text;
        }

        public Task ExecuteAsync(IShellState shellState, TProgramState programState, TParseResult parseResult, CancellationToken cancellationToken)
        {
            if (!DefaultCommandInput<TParseResult>.TryProcess(InputSpec, parseResult, out DefaultCommandInput<TParseResult> commandInput, out IReadOnlyList<CommandInputProcessingIssue> _))
            {
                return Task.CompletedTask;
            }

            return ExecuteAsync(shellState, programState, commandInput, parseResult, cancellationToken);
        }

        protected abstract Task ExecuteAsync(IShellState shellState, TProgramState programState, DefaultCommandInput<TParseResult> commandInput, TParseResult parseResult, CancellationToken cancellationToken);

        public abstract CommandInputSpecification InputSpec { get; }
    }
}
