// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Repl;
using Microsoft.Repl.Commanding;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Parsing;

namespace Microsoft.HttpRepl.Commands
{
    public class ExitCommand : CommandWithStructuredInputBase<object, ICoreParseResult>
    {
        public override string Name => "exit";

        protected override Task ExecuteAsync(IShellState shellState, object programState, DefaultCommandInput<ICoreParseResult> commandInput, ICoreParseResult parseResult, CancellationToken cancellationToken)
        {
            shellState = shellState ?? throw new ArgumentNullException(nameof(shellState));

            shellState.IsExiting = true;
            return Task.CompletedTask;
        }

        public override CommandInputSpecification InputSpec { get; } = CommandInputSpecification.Create("exit").ExactArgCount(0).Finish();

        protected override string GetHelpDetails(IShellState shellState, object programState, DefaultCommandInput<ICoreParseResult> commandInput, ICoreParseResult parseResult)
        {
            var helpText = new StringBuilder();
            helpText.Append(Resources.Strings.Usage.Bold());
            helpText.AppendLine($"exit");
            helpText.AppendLine();
            helpText.AppendLine($"Exits the shell");
            return helpText.ToString();
        }

        public override string GetHelpSummary(IShellState shellState, object programState)
        {
            return Resources.Strings.ExitCommand_HelpSummary;
        }
    }
}
