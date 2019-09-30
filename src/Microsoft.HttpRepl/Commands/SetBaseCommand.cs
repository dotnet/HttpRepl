// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Sockets;
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
    public class SetBaseCommand : ICommand<HttpState, ICoreParseResult>
    {
        private const string Name = "set";
        private const string SubCommand = "base";

        public string Description => Strings.SetBaseCommand_HelpSummary;

        public bool? CanHandle(IShellState shellState, HttpState programState, ICoreParseResult parseResult)
        {
            return parseResult.ContainsAtLeast(Name, SubCommand)
                ? (bool?)true
                : null;
        }

        public async Task ExecuteAsync(IShellState shellState, HttpState programState, ICoreParseResult parseResult, CancellationToken cancellationToken)
        {
            shellState = shellState ?? throw new ArgumentNullException(nameof(shellState));

            programState = programState ?? throw new ArgumentNullException(nameof(programState));

            parseResult = parseResult ?? throw new ArgumentNullException(nameof(parseResult));

            if (parseResult.Sections.Count == 2)
            {
                programState.BaseAddress = null;
            }
            else if (parseResult.Sections.Count != 3 || string.IsNullOrEmpty(parseResult.Sections[2]) || !Uri.TryCreate(parseResult.Sections[2].EnsureTrailingSlash(), UriKind.Absolute, out Uri serverUri))
            {
                shellState.ConsoleManager.Error.WriteLine(Strings.SetBaseCommand_MustSpecifyServerError.SetColor(programState.ErrorColor));
            }
            else
            {
                programState.BaseAddress = serverUri;
                try
                {
                    using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, serverUri))
                    {
                        await programState.Client.SendAsync(request).ConfigureAwait(false);
                    }
                }
                catch (Exception ex) when (ex.InnerException is SocketException se)
                {
                    shellState.ConsoleManager.Error.WriteLine(String.Format(Strings.SetBaseCommand_HEADRequestUnSuccessful, se.Message).SetColor(programState.WarningColor));
                }
                catch { }
            }
        }

        public string GetHelpDetails(IShellState shellState, HttpState programState, ICoreParseResult parseResult)
        {
            if (parseResult.ContainsAtLeast(Name, SubCommand))
            {
                var helpText = new StringBuilder();
                helpText.Append(Strings.Usage.Bold());
                helpText.AppendLine($"set base [uri]");
                helpText.AppendLine();
                helpText.AppendLine(Description);
                return helpText.ToString();
            }

            return null;
        }

        public string GetHelpSummary(IShellState shellState, HttpState programState)
        {
            return Description;
        }

        public IEnumerable<string> Suggest(IShellState shellState, HttpState programState, ICoreParseResult parseResult)
        {
            parseResult = parseResult ?? throw new ArgumentNullException(nameof(parseResult));

            if (parseResult.Sections.Count == 0)
            {
                return new[] { Name };
            }

            if (parseResult.Sections.Count > 0 && parseResult.SelectedSection == 0 && Name.StartsWith(parseResult.Sections[0].Substring(0, parseResult.CaretPositionWithinSelectedSection), StringComparison.OrdinalIgnoreCase))
            {
                return new[] { Name };
            }

            if (string.Equals(Name, parseResult.Sections[0], StringComparison.OrdinalIgnoreCase) && parseResult.SelectedSection == 1 && (parseResult.Sections.Count < 2 || SubCommand.StartsWith(parseResult.Sections[1].Substring(0, parseResult.CaretPositionWithinSelectedSection), StringComparison.OrdinalIgnoreCase)))
            {
                return new[] { SubCommand };
            }

            return null;
        }
    }
}
