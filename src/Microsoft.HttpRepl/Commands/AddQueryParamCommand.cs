// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Resources;
using Microsoft.Repl;
using Microsoft.Repl.Commanding;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Parsing;

namespace Microsoft.HttpRepl.Commands
{
    public class AddQueryParamCommand : ICommand<HttpState, ICoreParseResult>
    {
        private const string CommandName = "add";
        private const string SubCommand = "query-param";

        public string Name => "addQueryParam";

        public bool? CanHandle(IShellState shellState, HttpState programState, ICoreParseResult parseResult) =>
            parseResult.ContainsAtLeast(minimumLength: 3, CommandName, SubCommand)
                ? (bool?)true
                : null;

        public Task ExecuteAsync(IShellState shellState, HttpState programState, ICoreParseResult parseResult, CancellationToken cancellationToken)
        {
            parseResult = parseResult ?? throw new ArgumentNullException(nameof(parseResult));

            programState = programState ?? throw new ArgumentNullException(nameof(programState));

            int sectionCount = parseResult.Sections.Count;

            if(sectionCount % 2 == 0)
            {               
                for(int i = 2; i < sectionCount; i+=2)
                {
                    if (i + 1 < sectionCount)
                    {
                        if (programState.QueryParam.ContainsKey(parseResult.Sections[i])){
                            IEnumerable<string> updatedParams = programState.QueryParam[parseResult.Sections[i]].Append(parseResult.Sections[i + 1]);
                            programState.QueryParam[parseResult.Sections[i]] = updatedParams;
                        } else
                        {
                            programState.QueryParam[parseResult.Sections[i]] = Enumerable.Repeat(parseResult.Sections[i + 1], 1);
                        }
                            
                    }             
                }
            } 
            else
            {
                shellState.ConsoleManager.WriteLine($"The add query-param command key: {parseResult.Sections[sectionCount - 1]} is missing a value. Please try again with a valid key value pair");           
            }

            return Task.CompletedTask;
        }

        public string GetHelpDetails(IShellState shellState, HttpState programState, ICoreParseResult parseResult)
        {
            if (parseResult.ContainsAtLeast(CommandName, SubCommand))
            {
                StringBuilder helpText = new StringBuilder();
                helpText.Append(Strings.Usage.Bold());
                helpText.AppendLine("add query-param {name} [value]");
                helpText.AppendLine();
                helpText.AppendLine(Strings.AddQueryParamCommand_HelpDetails);
                return helpText.ToString();
            }

            return null;
        }

        public string GetHelpSummary(IShellState shellState, HttpState programState) => Strings.AddQueryParamCommand_HelpSummary;

        public IEnumerable<string> Suggest(IShellState shellState, HttpState programState, ICoreParseResult parseResult) => null;
    }
}
