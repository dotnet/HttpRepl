// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Resources;
using Microsoft.Repl;
using Microsoft.Repl.Commanding;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Parsing;

namespace Microsoft.HttpRepl.Commands
{
    public class UICommand : ICommand<HttpState, ICoreParseResult>
    {
        private static readonly string Name = "ui";
        private IUriLauncher UriLauncher;

        public UICommand(IUriLauncher uriLauncher)
        {
            UriLauncher = uriLauncher ?? throw new ArgumentNullException(nameof(uriLauncher));
        }

        public bool? CanHandle(IShellState shellState, HttpState programState, ICoreParseResult parseResult)
        {
            return parseResult.Sections.Count == 1 && string.Equals(parseResult.Sections[0], Name)
                ? (bool?)true
                : null;
        }

        public Task ExecuteAsync(IShellState shellState, HttpState programState, ICoreParseResult parseResult, CancellationToken cancellationToken)
        {
            if (programState.BaseAddress == null)
            {
                shellState.ConsoleManager.Error.WriteLine(Strings.UICommand_NotConnectedToServerError.SetColor(programState.ErrorColor));
                return Task.CompletedTask;
            }

            Uri uri = new Uri(programState.BaseAddress, "swagger");

            return UriLauncher.LaunchUriAsync(uri);
        }

        public string GetHelpDetails(IShellState shellState, HttpState programState, ICoreParseResult parseResult)
        {
            if (parseResult.Sections.Count == 1 && string.Equals(parseResult.Sections[0], Name))
            {
                return Strings.UICommand_Description;
            }

            return null;
        }

        public string GetHelpSummary(IShellState shellState, HttpState programState)
        {
            return Resources.Strings.UICommand_HelpSummary;
        }

        public IEnumerable<string> Suggest(IShellState shellState, HttpState programState, ICoreParseResult parseResult)
        {
            if (parseResult.SelectedSection == 0 &&
                (string.IsNullOrEmpty(parseResult.Sections[parseResult.SelectedSection]) || Name.StartsWith(parseResult.Sections[0].Substring(0, parseResult.CaretPositionWithinSelectedSection), StringComparison.OrdinalIgnoreCase)))
            {
                return new[] { Name };
            }

            return null;
        }
    }
}
