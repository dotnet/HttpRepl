// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Preferences;
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
        private static readonly string Name = "help";

        private readonly IPreferences _preferences;

        public HelpCommand(IPreferences preferences)
        {
            _preferences = preferences;
        }

        public bool? CanHandle(IShellState shellState, HttpState programState, ICoreParseResult parseResult)
        {
            return parseResult.ContainsAtLeast(Name)
                ? (bool?)true
                : null;
        }

        public async Task ExecuteAsync(IShellState shellState, HttpState programState, ICoreParseResult parseResult, CancellationToken cancellationToken)
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
                    var output = new StringBuilder();

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

                                var structuredCommand = command as CommandWithStructuredInputBase<HttpState, ICoreParseResult>;
                                if (structuredCommand != null && structuredCommand.InputSpec.Options.Any())
                                {
                                    output.AppendLine();
                                    output.AppendLine(Resources.Strings.Options.Bold());
                                    foreach (var option in structuredCommand.InputSpec.Options)
                                    {
                                        var optionText = string.Empty;
                                        foreach (var form in option.Forms)
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
                        //Maybe the input is an URL
                        if (parseResult.Sections.Count == 2)
                        {

                            if (programState.SwaggerEndpoint != null)
                            {
                                string swaggerRequeryBehaviorSetting = _preferences.GetValue(WellKnownPreference.SwaggerRequeryBehavior, "auto");

                                if (swaggerRequeryBehaviorSetting.StartsWith("auto", StringComparison.OrdinalIgnoreCase))
                                {
                                    ApiConnection apiConnection = new ApiConnection(_preferences)
                                    {
                                        BaseUri = programState.BaseAddress,
                                        SwaggerUri = programState.SwaggerEndpoint,
                                        AllowBaseOverrideBySwagger = false
                                    };
                                    await apiConnection.SetupHttpState(programState, performAutoDetect: false, cancellationToken).ConfigureAwait(false);
                                }
                            }

                            //Structure is null because, for example, SwaggerEndpoint exists but is not reachable.
                            if (programState.Structure != null)
                            {
                                IDirectoryStructure structure = programState.Structure.TraverseTo(parseResult.Sections[1]);
                                if (structure.DirectoryNames.Any())
                                {
                                    output.AppendLine("Child directories:");

                                    foreach (string name in structure.DirectoryNames)
                                    {
                                        output.AppendLine("  " + name + "/");
                                    }
                                    anyHelp = true;
                                }

                                if (structure.RequestInfo != null)
                                {
                                    if (structure.RequestInfo.Methods.Count > 0)
                                    {
                                        if (anyHelp)
                                        {
                                            output.AppendLine();
                                        }

                                        anyHelp = true;
                                        output.AppendLine("Available methods:");

                                        foreach (string method in structure.RequestInfo.Methods)
                                        {
                                            output.AppendLine("  " + method.ToUpperInvariant());
                                            IReadOnlyList<string> accepts = structure.RequestInfo.ContentTypesByMethod[method];
                                            string acceptsString = string.Join(", ", accepts.Where(x => !string.IsNullOrEmpty(x)));
                                            if (!string.IsNullOrEmpty(acceptsString))
                                            {
                                                output.AppendLine("    Accepts: " + acceptsString);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (!anyHelp)
                        {
                            output.AppendLine("Unable to locate any help information for the specified command");
                        }
                    }

                    shellState.ConsoleManager.Write(output.ToString());
                }
            }
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

        public void CoreGetHelp(IShellState shellState, ICommandDispatcher<HttpState, ICoreParseResult> dispatcher, HttpState programState)
        {
            shellState = shellState ?? throw new ArgumentNullException(nameof(shellState));

            dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));

            const int navCommandColumn = -15;
            var output = new StringBuilder();

            output.AppendLine();
            output.AppendLine(Strings.HelpCommand_Core_SetupCommands.Bold().Cyan());
            output.AppendLine(Strings.HelpCommand_Core_SetupCommands_Description);
            output.AppendLine();
            output.AppendLine($"{"connect",navCommandColumn}{dispatcher.GetCommand<ConnectCommand>().GetHelpSummary(shellState, programState)}");
            output.AppendLine($"{"set header",navCommandColumn}{dispatcher.GetCommand<SetHeaderCommand>().GetHelpSummary(shellState, programState)}");

            output.AppendLine();
            output.AppendLine(Strings.HelpCommand_Core_HttpCommands.Bold().Cyan());
            output.AppendLine(Strings.HelpCommand_Core_HttpCommands_Description);
            output.AppendLine();
            output.AppendLine($"{"GET",navCommandColumn}{dispatcher.GetCommand<GetCommand>().GetHelpSummary(shellState, programState)}");
            output.AppendLine($"{"POST",navCommandColumn}{dispatcher.GetCommand<PostCommand>().GetHelpSummary(shellState, programState)}");
            output.AppendLine($"{"PUT",navCommandColumn}{dispatcher.GetCommand<PutCommand>().GetHelpSummary(shellState, programState)}");
            output.AppendLine($"{"DELETE",navCommandColumn}{dispatcher.GetCommand<DeleteCommand>().GetHelpSummary(shellState, programState)}");
            output.AppendLine($"{"PATCH",navCommandColumn}{dispatcher.GetCommand<PatchCommand>().GetHelpSummary(shellState, programState)}");
            output.AppendLine($"{"HEAD",navCommandColumn}{dispatcher.GetCommand<HeadCommand>().GetHelpSummary(shellState, programState)}");
            output.AppendLine($"{"OPTIONS",navCommandColumn}{dispatcher.GetCommand<OptionsCommand>().GetHelpSummary(shellState, programState)}");          

            output.AppendLine();
            output.AppendLine(Strings.HelpCommand_Core_NavigationCommands.Bold().Cyan());
            output.AppendLine(Strings.HelpCommand_Core_NavigationCommands_Description);
            output.AppendLine();

            output.AppendLine($"{"set base",navCommandColumn}{dispatcher.GetCommand<SetBaseCommand>().GetHelpSummary(shellState, programState)}");
            output.AppendLine($"{"ls",navCommandColumn}{dispatcher.GetCommand<ListCommand>().GetHelpSummary(shellState, programState)}");
            output.AppendLine($"{"cd",navCommandColumn}{dispatcher.GetCommand<ChangeDirectoryCommand>().GetHelpSummary(shellState, programState)}");

            output.AppendLine();
            output.AppendLine(Strings.HelpCommand_Core_ShellCommands.Bold().Cyan());
            output.AppendLine(Strings.HelpCommand_Core_ShellCommands_Description);
            output.AppendLine();

            output.AppendLine($"{"clear",navCommandColumn}{dispatcher.GetCommand<ClearCommand>().GetHelpSummary(shellState, programState)}");
            output.AppendLine($"{"echo [on/off]",navCommandColumn}{dispatcher.GetCommand<EchoCommand>().GetHelpSummary(shellState, programState)}");
            output.AppendLine($"{"exit",navCommandColumn}{dispatcher.GetCommand<ExitCommand>().GetHelpSummary(shellState, programState)}");

            output.AppendLine();
            output.AppendLine(Strings.HelpCommand_Core_CustomizationCommands.Bold().Cyan());
            output.AppendLine(Strings.HelpCommand_Core_CustomizationCommands_Description);
            output.AppendLine();

            output.AppendLine($"{"pref [get/set]",navCommandColumn}{dispatcher.GetCommand<PrefCommand>().GetHelpSummary(shellState, programState)}");
            output.AppendLine($"{"run",navCommandColumn}{dispatcher.GetCommand<RunCommand>().GetHelpSummary(shellState, programState)}");
            output.AppendLine($"{"ui",navCommandColumn}{dispatcher.GetCommand<UICommand>().GetHelpSummary(shellState, programState)}");
            output.AppendLine();
            output.AppendLine(Strings.HelpCommand_Core_Details_Line1.Bold().Cyan());
            output.AppendLine(Strings.HelpCommand_Core_Details_Line2.Bold().Cyan());
            output.AppendLine();

            shellState.ConsoleManager.Write(output.ToString());
        }
    }
}
