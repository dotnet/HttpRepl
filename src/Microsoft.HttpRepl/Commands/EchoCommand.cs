// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Repl;
using Microsoft.Repl.Commanding;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Parsing;

namespace Microsoft.HttpRepl.Commands
{
    public class EchoCommand : CommandWithStructuredInputBase<HttpState, ICoreParseResult>
    {
        public override string Name => "echo";

        private readonly HashSet<string> _allowedModes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {"on", "off"};

        protected override bool CanHandle(IShellState shellState, HttpState programState, DefaultCommandInput<ICoreParseResult> commandInput)
        {
            commandInput = commandInput ?? throw new ArgumentNullException(nameof(commandInput));

            shellState = shellState ?? throw new ArgumentNullException(nameof(shellState));

            programState = programState ?? throw new ArgumentNullException(nameof(programState));

            if (commandInput.Arguments.Count == 0 || !_allowedModes.Contains(commandInput.Arguments[0]?.Text))
            {
                shellState.ConsoleManager.Error.WriteLine(Resources.Strings.EchoCommand_Error_AllowedModes.SetColor(programState.ErrorColor));
                return false;
            }

            return true;
        }

        protected override Task ExecuteAsync(IShellState shellState, HttpState programState, DefaultCommandInput<ICoreParseResult> commandInput, ICoreParseResult parseResult, CancellationToken cancellationToken)
        {
            shellState = shellState ?? throw new ArgumentNullException(nameof(shellState));

            commandInput = commandInput ?? throw new ArgumentNullException(nameof(commandInput));

            programState = programState ?? throw new ArgumentNullException(nameof(programState));

            bool turnOn = string.Equals(commandInput.Arguments[0].Text, "on", StringComparison.OrdinalIgnoreCase);
            programState.EchoRequest = turnOn;

            shellState.ConsoleManager.WriteLine("Request echoing is " + (turnOn ? "on" : "off"));
            return Task.CompletedTask;
        }

        public override CommandInputSpecification InputSpec { get; } = CommandInputSpecification.Create("echo").ExactArgCount(1).Finish();

        protected override string GetHelpDetails(IShellState shellState, HttpState programState, DefaultCommandInput<ICoreParseResult> commandInput, ICoreParseResult parseResult)
        {
            var helpText = new StringBuilder();
            helpText.Append(Resources.Strings.Usage.Bold());
            helpText.AppendLine($"echo [on|off]");
            helpText.AppendLine();
            helpText.AppendLine($"Turns request echoing on or off. When request echoing is on we will display a text representation of requests made by the CLI.");
            return helpText.ToString();
        }

        public override string GetHelpSummary(IShellState shellState, HttpState programState)
        {
            return Resources.Strings.EchoCommand_HelpSummary;
        }

        protected override IEnumerable<string> GetArgumentSuggestionsForText(IShellState shellState, HttpState programState, ICoreParseResult parseResult, DefaultCommandInput<ICoreParseResult> commandInput, string normalCompletionString)
        {
            List<string> result = _allowedModes.Where(x => x.StartsWith(normalCompletionString, StringComparison.OrdinalIgnoreCase)).ToList();
            return result.Count > 0 ? result : null;
        }
    }
}
