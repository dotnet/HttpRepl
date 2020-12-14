// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Preferences;
using Microsoft.Repl;
using Microsoft.Repl.Commanding;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Parsing;

namespace Microsoft.HttpRepl.Commands
{
    public class ListCommand : CommandWithStructuredInputBase<HttpState, ICoreParseResult>
    {
        private const string RecursiveOption = nameof(RecursiveOption);
        private const string VerboseOption = nameof(VerboseOption);

        private readonly IPreferences _preferences;

        public override string Name => "list";

        public ListCommand(IPreferences preferences)
        {
            _preferences = preferences;
        }

        protected override async Task ExecuteAsync(IShellState shellState, HttpState programState, DefaultCommandInput<ICoreParseResult> commandInput, ICoreParseResult parseResult, CancellationToken cancellationToken)
        {
            programState = programState ?? throw new ArgumentNullException(nameof(programState));

            shellState = shellState ?? throw new ArgumentNullException(nameof(shellState));

            commandInput = commandInput ?? throw new ArgumentNullException(nameof(commandInput));

            if (programState.SwaggerEndpoint != null)
            {
                string swaggerRequeryBehaviorSetting = _preferences.GetValue(WellKnownPreference.SwaggerRequeryBehavior, "auto");

                if (swaggerRequeryBehaviorSetting.StartsWith("auto", StringComparison.OrdinalIgnoreCase))
                {
                    ApiConnection apiConnection = new ApiConnection(programState, _preferences, shellState.ConsoleManager, logVerboseMessages: false)
                    {
                        BaseUri = programState.BaseAddress,
                        SwaggerUri = programState.SwaggerEndpoint,
                        AllowBaseOverrideBySwagger = false
                    };
                    await apiConnection.SetupHttpState(programState, performAutoDetect: false, persistHeaders: true, persistPath: true, cancellationToken).ConfigureAwait(false);
                }
            }

            if (programState.BaseAddress is null)
            {
                shellState.ConsoleManager.WriteLine(Resources.Strings.ListCommand_Error_NoBaseAddress.SetColor(programState.WarningColor));
                return;
            }

            if (programState.SwaggerEndpoint is null || programState.Structure is null)
            {
                shellState.ConsoleManager.WriteLine(Resources.Strings.ListCommand_Error_NoDirectoryStructure.SetColor(programState.WarningColor));
                return;
            }

            string path = commandInput.Arguments.Count > 0 ? commandInput.Arguments[0].Text : string.Empty;

            //If it's an absolute URI, nothing to suggest
            if (Uri.TryCreate(path, UriKind.Absolute, out Uri _))
            {
                return;
            }

            IDirectoryStructure s = programState.Structure.TraverseTo(programState.PathSections.Reverse()).TraverseTo(path);

            string thisDirMethod = s.RequestInfo.GetDirectoryMethodListing();

            List<TreeNode> roots = new List<TreeNode>();
            Formatter formatter = new Formatter();

            roots.Add(new TreeNode(formatter, ".", thisDirMethod));

            if (s.Parent != null)
            {
                string parentDirMethod = s.Parent.RequestInfo.GetDirectoryMethodListing();

                roots.Add(new TreeNode(formatter, "..", parentDirMethod));
            }

            int recursionDepth = 1;

            if (commandInput.Options[RecursiveOption].Count > 0)
            {
                if (string.IsNullOrEmpty(commandInput.Options[RecursiveOption][0]?.Text))
                {
                    recursionDepth = int.MaxValue;
                }
                else if (int.TryParse(commandInput.Options[RecursiveOption][0].Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int rd) && rd > 1)
                {
                    recursionDepth = rd;
                }
            }

            foreach (string child in s.DirectoryNames)
            {
                IDirectoryStructure dir = s.GetChildDirectory(child);

                string methods = dir.RequestInfo.GetDirectoryMethodListing();

                TreeNode dirNode = new TreeNode(formatter, child, methods);
                roots.Add(dirNode);
                Recurse(dirNode, dir, recursionDepth - 1);
            }

            foreach (TreeNode node in roots)
            {
                shellState.ConsoleManager.WriteLine(node.ToString());
            }

            bool hasVerboseOption = commandInput.Options[VerboseOption].Count > 0;
            bool hasRequestMethods = s.RequestInfo?.Methods?.Count > 0;

            if (hasVerboseOption && hasRequestMethods)
            {
                shellState.ConsoleManager.WriteLine();
                shellState.ConsoleManager.WriteLine(Resources.Strings.ListCommand_AvailableMethods);

                foreach (string method in s.RequestInfo.Methods)
                {
                    shellState.ConsoleManager.WriteLine("  " + method.ToUpperInvariant());
                    IReadOnlyList<string> accepts = s.RequestInfo.ContentTypesByMethod[method];
                    string acceptsString = string.Join(", ", accepts.Where(x => !string.IsNullOrEmpty(x)));
                    if (!string.IsNullOrEmpty(acceptsString))
                    {
                        shellState.ConsoleManager.WriteLine($"    {Resources.Strings.ListCommand_Accepts} " + acceptsString);
                    }
                }
            }
        }

        private static void Recurse(TreeNode parentNode, IDirectoryStructure parent, int remainingDepth)
        {
            if (remainingDepth <= 0)
            {
                return;
            }

            foreach (string child in parent.DirectoryNames)
            {
                IDirectoryStructure dir = parent.GetChildDirectory(child);

                string methods = dir.RequestInfo?.GetDirectoryMethodListing();

                TreeNode node = parentNode.AddChild(child, methods);
                Recurse(node, dir, remainingDepth - 1);
            }
        }



        public override CommandInputSpecification InputSpec { get; } = CommandInputSpecification.Create("ls").AlternateName("dir")
            .MaximumArgCount(1)
            .WithOption(new CommandOptionSpecification(RecursiveOption, acceptsValue: true, maximumOccurrences: 1, forms: new[] {"-r", "--recursive"}))
            .WithOption(new CommandOptionSpecification(VerboseOption, acceptsValue: false, maximumOccurrences: 1, forms: new[] { "-v", "--verbose"}))
            .Finish();

        protected override string GetHelpDetails(IShellState shellState, HttpState programState, DefaultCommandInput<ICoreParseResult> commandInput, ICoreParseResult parseResult)
        {
            var helpText = new StringBuilder();
            helpText.Append(Resources.Strings.Usage.Bold());
            helpText.AppendLine($"ls [Options]");
            helpText.AppendLine();
            helpText.AppendLine($"Displays the known routes at the current location. Requires a Swagger document to be set.");
            return helpText.ToString();
        }

        public override string GetHelpSummary(IShellState shellState, HttpState programState)
        {
            return Resources.Strings.ListCommand_HelpSummary;
        }

        protected override IEnumerable<string> GetArgumentSuggestionsForText(IShellState shellState, HttpState programState, ICoreParseResult parseResult, DefaultCommandInput<ICoreParseResult> commandInput, string normalCompletionString)
        {
            programState = programState ?? throw new ArgumentNullException(nameof(programState));

            if (programState.Structure == null || programState.BaseAddress == null)
            {
                return null;
            }

            //If it's an absolute URI, nothing to suggest
            if (Uri.TryCreate(normalCompletionString, UriKind.Absolute, out Uri _))
            {
                return null;
            }

            normalCompletionString = normalCompletionString ?? throw new ArgumentNullException(nameof(normalCompletionString));

            string path = normalCompletionString.Replace('\\', '/');
            int searchFrom = normalCompletionString.Length - 1;
            int lastSlash = path.LastIndexOf('/', searchFrom);
            string prefix;

            if (lastSlash < 0)
            {
                path = string.Empty;
                prefix = normalCompletionString;
            }
            else
            {
                path = path.Substring(0, lastSlash + 1);
                prefix = normalCompletionString.Substring(lastSlash + 1);
            }

            IDirectoryStructure s = programState.Structure.TraverseTo(programState.PathSections.Reverse()).TraverseTo(path);

            List<string> results = new List<string>();

            foreach (string child in s.DirectoryNames)
            {
                if (child.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(path + child);
                }
            }

            return results;
        }
    }
}
