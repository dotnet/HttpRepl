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
using Microsoft.Repl;
using Microsoft.Repl.Commanding;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Parsing;

namespace Microsoft.HttpRepl.Commands
{
    public class ChangeDirectoryCommand : CommandWithStructuredInputBase<HttpState, ICoreParseResult>
    {
        protected override Task ExecuteAsync(IShellState shellState, HttpState programState, DefaultCommandInput<ICoreParseResult> commandInput, ICoreParseResult parseResult, CancellationToken cancellationToken)
        {
            commandInput = commandInput ?? throw new ArgumentNullException(nameof(commandInput));

            shellState = shellState ?? throw new ArgumentNullException(nameof(shellState));

            programState = programState ?? throw new ArgumentNullException(nameof(programState));

            if (commandInput.Arguments.Count == 0 || string.IsNullOrEmpty(commandInput.Arguments[0]?.Text))
            {
                shellState.ConsoleManager.WriteLine(programState.GetRelativePathString());
            }
            else
            {
                string[] parts = commandInput.Arguments[0].Text.Replace('\\', '/').Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                if (commandInput.Arguments[0].Text.StartsWith("/", StringComparison.Ordinal))
                {
                    programState.PathSections.Clear();
                }

                foreach (string part in parts)
                {
                    switch (part)
                    {
                        case ".":
                            break;
                        case "..":
                            if (programState.PathSections.Count > 0)
                            {
                                programState.PathSections.Pop();
                            }
                            break;
                        default:
                            programState.PathSections.Push(part);
                            break;
                    }
                }

                // If there's no directory structure, we can't traverse it to find the relevant
                // metadata and display it. The command still succeeded as far as its impact on
                // future commands, so we can safely just skip this part.
                if (programState.Structure != null)
                {
                    IDirectoryStructure s = programState.Structure.TraverseTo(programState.PathSections.Reverse());

                    string thisDirMethod = "[]";

                    bool hasRequestMethods = s.RequestInfo?.Methods?.Count > 0;
                    bool hasDirectoryNames = s.DirectoryNames?.Any() == true;

                    // If there's no RequestInfo/Methods AND there's no (sub)DirectoryNames, we currently
                    // assume this must be an auto-generated directory and not one from a swagger definition
                    if (!hasRequestMethods && !hasDirectoryNames)
                    {
                        string warningMessage = string.Format(Resources.Strings.ChangeDirectoryCommand_Warning_UnknownEndpoint, programState.GetRelativePathString()).SetColor(programState.WarningColor);
                        shellState.ConsoleManager.WriteLine(warningMessage);
                    }
                    else if (hasRequestMethods)
                    {
                        thisDirMethod = s.RequestInfo.GetDirectoryMethodListing();
                    }

                    shellState.ConsoleManager.WriteLine($"{programState.GetRelativePathString()}    {thisDirMethod}");
                }
            }

            return Task.CompletedTask;
        }

        public override CommandInputSpecification InputSpec { get; } = CommandInputSpecification.Create("cd")
            .MaximumArgCount(1)
            .Finish();

        protected override string GetHelpDetails(IShellState shellState, HttpState programState, DefaultCommandInput<ICoreParseResult> commandInput, ICoreParseResult parseResult)
        {
            var help = new StringBuilder();
            help.Append(Strings.Usage.Bold());
            help.AppendLine("cd [directory]");
            help.AppendLine();
            help.AppendLine("Prints the current directory if no argument is specified, otherwise changes to the specified directory");

            return help.ToString();
        }

        public override string GetHelpSummary(IShellState shellState, HttpState programState)
        {
            return Resources.Strings.ChangeDirectoryCommand_HelpSummary;
        }

        protected override IEnumerable<string> GetArgumentSuggestionsForText(IShellState shellState, HttpState programState, ICoreParseResult parseResult, DefaultCommandInput<ICoreParseResult> commandInput, string normalCompletionString)
        {
            return ServerPathCompletion.GetCompletions(programState, normalCompletionString);
        }
    }
}
