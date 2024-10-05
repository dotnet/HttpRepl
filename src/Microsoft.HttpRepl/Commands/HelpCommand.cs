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
using Microsoft.HttpRepl.Suggestions;
using Microsoft.Repl;
using Microsoft.Repl.Commanding;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Parsing;

namespace Microsoft.HttpRepl.Commands
{
    public class HelpCommand : ICommand<HttpState, ICoreParseResult>
    {
        public string Name => "help";

        public HelpCommand()
        {

        }

        public bool? CanHandle(IShellState shellState, HttpState programState, ICoreParseResult parseResult)
        {
            return parseResult.ContainsAtLeast(Name)
                ? (bool?)true
                : null;
        }

        public Task ExecuteAsync(IShellState shellState, HttpState programState, ICoreParseResult parseResult, CancellationToken cancellationToken)
        {
            shellState = shellState ?? throw new ArgumentNullException(nameof(shellState));

            parseResult = parseResult ?? throw new ArgumentNullException(nameof(parseResult));

            programState = programState ?? throw new ArgumentNullException(nameof(programState));

            if (shellState.CommandDispatcher is ICommandDispatcher<HttpState, ICoreParseResult> dispatcher)
            {
                if (parseResult.Sections.Count == 1)
                {
                    CoreGetHelp(shellState, dispatcher, programState);
                }
                else
                {
                    bool anyHelp = false;
                    StringBuilder output = new StringBuilder();

                    if (parseResult.Slice(1) is ICoreParseResult continuationParseResult)
                    {
                        foreach (ICommand<HttpState, ICoreParseResult> command in dispatcher.Commands)
                        {
                            string help = command.GetHelpDetails(shellState, programState, continuationParseResult);

                            if (!string.IsNullOrEmpty(help))
                            {
                                anyHelp = true;
                                output.AppendLine();
                                output.AppendLine(help);

                                CommandWithStructuredInputBase<HttpState, ICoreParseResult> structuredCommand = GetStructuredCommand(command);
                                if (structuredCommand is not null && structuredCommand.InputSpec.Options.Any())
                                {
                                    output.AppendLine();
                                    output.AppendLine(Strings.Options.Bold());
                                    foreach (CommandOptionSpecification option in structuredCommand.InputSpec.Options)
                                    {
                                        string optionText = string.Empty;
                                        foreach (string form in option.Forms)
                                        {
                                            if (!string.IsNullOrEmpty(optionText))
                                            {
                                                optionText += "|";
                                            }
                                            optionText += form;
                                        }
                                        output.AppendLine($"    {optionText}");
                                    }
                                }

                                break;
                            }
                        }
                    }

                    if (!anyHelp)
                    {
                        output.AppendLine(Strings.HelpCommand_Error_UnableToLocateHelpInfo);
                    }

                    shellState.ConsoleManager.Write(output.ToString());
                }
            }

            return Task.CompletedTask;
        }

        public string GetHelpDetails(IShellState shellState, HttpState programState, ICoreParseResult parseResult)
        {
            parseResult = parseResult ?? throw new ArgumentNullException(nameof(parseResult));

            if (parseResult.ContainsAtLeast(Name))
            {
                if (parseResult.Sections.Count > 1)
                {
                    return "Gets help about " + parseResult.Slice(1).CommandText;
                }
                else
                {
                    return "Gets help";
                }
            }

            return null;
        }

        public string GetHelpSummary(IShellState shellState, HttpState programState)
        {
            return "help - Gets help";
        }

        public IEnumerable<string> Suggest(IShellState shellState, HttpState programState, ICoreParseResult parseResult)
        {
            parseResult = parseResult ?? throw new ArgumentNullException(nameof(parseResult));

            shellState = shellState ?? throw new ArgumentNullException(nameof(shellState));

            if (parseResult.SelectedSection == 0 &&
                (string.IsNullOrEmpty(parseResult.Sections[parseResult.SelectedSection]) || Name.StartsWith(parseResult.Sections[0].Substring(0, parseResult.CaretPositionWithinSelectedSection), StringComparison.OrdinalIgnoreCase)))
            {
                return new[] { Name };
            }
            else if (parseResult.ContainsAtLeast(minimumLength: 2, Name))
            {
                if (shellState.CommandDispatcher is ICommandDispatcher<HttpState, ICoreParseResult> dispatcher
                    && parseResult.Slice(1) is ICoreParseResult continuationParseResult)
                {
                    HashSet<string> suggestions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    foreach (ICommand<HttpState, ICoreParseResult> command in dispatcher.Commands)
                    {
                        IEnumerable<string> commandSuggestions = command.Suggest(shellState, programState, continuationParseResult);

                        if (commandSuggestions != null)
                        {
                            suggestions.UnionWith(commandSuggestions);
                        }
                    }

                    if (continuationParseResult.SelectedSection == 0)
                    {
                        string normalizedCompletionText = continuationParseResult.Sections[0].Substring(0, continuationParseResult.CaretPositionWithinSelectedSection);
                        IEnumerable<string> completions = ServerPathCompletion.GetCompletions(programState, normalizedCompletionText);

                        if (completions != null)
                        {
                            suggestions.UnionWith(completions);
                        }
                    }

                    return suggestions.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();
                }
            }

            return null;
        }

        public static void CoreGetHelp(IShellState shellState, ICommandDispatcher<HttpState, ICoreParseResult> dispatcher, HttpState programState)
        {
            shellState = shellState ?? throw new ArgumentNullException(nameof(shellState));

            dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));

            const int navCommandColumn = -15;
            StringBuilder output = new StringBuilder();

            output.AppendLine();
            output.AppendLine(Strings.HelpCommand_Core_SetupCommands.Bold().Cyan());
            output.AppendLine(Strings.HelpCommand_Core_SetupCommands_Description);
            output.AppendLine();
            output.AppendLine($"{"connect",navCommandColumn}{GetCommand<ConnectCommand>(dispatcher).GetHelpSummary(shellState, programState)}");
            output.AppendLine($"{"set header",navCommandColumn}{GetCommand<SetHeaderCommand>(dispatcher).GetHelpSummary(shellState, programState)}");
            output.AppendLine($"{"add query-param",navCommandColumn}{GetCommand<AddQueryParamCommand>(dispatcher).GetHelpSummary(shellState, programState)}");
            output.AppendLine($"{"clear query-param",navCommandColumn}{GetCommand<ClearQueryParamCommand>(dispatcher).GetHelpSummary(shellState, programState)}");
            output.AppendLine();
            output.AppendLine(Strings.HelpCommand_Core_HttpCommands.Bold().Cyan());
            output.AppendLine(Strings.HelpCommand_Core_HttpCommands_Description);
            output.AppendLine();
            output.AppendLine($"{"GET",navCommandColumn}{GetCommand<GetCommand>(dispatcher).GetHelpSummary(shellState, programState)}");
            output.AppendLine($"{"POST",navCommandColumn}{GetCommand<PostCommand>(dispatcher).GetHelpSummary(shellState, programState)}");
            output.AppendLine($"{"PUT",navCommandColumn}{GetCommand<PutCommand>(dispatcher).GetHelpSummary(shellState, programState)}");
            output.AppendLine($"{"DELETE",navCommandColumn}{GetCommand<DeleteCommand>(dispatcher).GetHelpSummary(shellState, programState)}");
            output.AppendLine($"{"PATCH",navCommandColumn}{GetCommand<PatchCommand>(dispatcher).GetHelpSummary(shellState, programState)}");
            output.AppendLine($"{"HEAD",navCommandColumn}{GetCommand<HeadCommand>(dispatcher).GetHelpSummary(shellState, programState)}");
            output.AppendLine($"{"OPTIONS",navCommandColumn}{GetCommand<OptionsCommand>(dispatcher).GetHelpSummary(shellState, programState)}");

            output.AppendLine();
            output.AppendLine(Strings.HelpCommand_Core_NavigationCommands.Bold().Cyan());
            output.AppendLine(Strings.HelpCommand_Core_NavigationCommands_Description);
            output.AppendLine();

            output.AppendLine($"{"ls",navCommandColumn}{GetCommand<ListCommand>(dispatcher).GetHelpSummary(shellState, programState)}");
            output.AppendLine($"{"cd",navCommandColumn}{GetCommand<ChangeDirectoryCommand>(dispatcher).GetHelpSummary(shellState, programState)}");

            output.AppendLine();
            output.AppendLine(Strings.HelpCommand_Core_ShellCommands.Bold().Cyan());
            output.AppendLine(Strings.HelpCommand_Core_ShellCommands_Description);
            output.AppendLine();

            output.AppendLine($"{"clear",navCommandColumn}{GetCommand<ClearCommand>(dispatcher).GetHelpSummary(shellState, programState)}");
            output.AppendLine($"{"echo [on/off]",navCommandColumn}{GetCommand<EchoCommand>(dispatcher).GetHelpSummary(shellState, programState)}");
            output.AppendLine($"{"exit",navCommandColumn}{GetCommand<ExitCommand>(dispatcher).GetHelpSummary(shellState, programState)}");

            output.AppendLine();
            output.AppendLine(Strings.HelpCommand_Core_CustomizationCommands.Bold().Cyan());
            output.AppendLine(Strings.HelpCommand_Core_CustomizationCommands_Description);
            output.AppendLine();

            output.AppendLine($"{"pref [get/set]",navCommandColumn}{GetCommand<PrefCommand>(dispatcher).GetHelpSummary(shellState, programState)}");
            output.AppendLine($"{"run",navCommandColumn}{GetCommand<RunCommand>(dispatcher).GetHelpSummary(shellState, programState)}");
            output.AppendLine($"{"ui",navCommandColumn}{GetCommand<UICommand>(dispatcher).GetHelpSummary(shellState, programState)}");
            output.AppendLine();
            output.AppendLine(Strings.HelpCommand_Core_Details_Line1.Bold().Cyan());
            output.AppendLine(Strings.HelpCommand_Core_Details_Line2.Bold().Cyan());
            output.AppendLine();

            shellState.ConsoleManager.Write(output.ToString());
        }

        private static CommandWithStructuredInputBase<HttpState, ICoreParseResult> GetStructuredCommand(ICommand<HttpState, ICoreParseResult> rawCommand)
        {
            return rawCommand switch
            {
                CommandWithStructuredInputBase<HttpState, ICoreParseResult> structuredCommand => structuredCommand,
                _ => null
            };
        }

        private static ICommand<HttpState, ICoreParseResult> GetCommand<T>(ICommandDispatcher<HttpState, ICoreParseResult> dispatcher) where T : ICommand<HttpState, ICoreParseResult>
        {
            foreach (ICommand<HttpState, ICoreParseResult> command in dispatcher.Commands)
            {
                switch (command)
                {
                    case T directMatch:
                        return directMatch;
                }
            }

            return null;
        }
    }
}
