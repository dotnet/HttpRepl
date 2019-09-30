// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Preferences;
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
        private IUriLauncher _uriLauncher;
        private IPreferences _preferences;

        public UICommand(IUriLauncher uriLauncher, IPreferences preferences)
        {
            _uriLauncher = uriLauncher ?? throw new ArgumentNullException(nameof(uriLauncher));
            _preferences = preferences ?? throw new ArgumentNullException(nameof(preferences));
        }

        public bool? CanHandle(IShellState shellState, HttpState programState, ICoreParseResult parseResult)
        {
            return parseResult.ContainsAtLeast(Name)
                ? (bool?)true
                : null;
        }

        public Task ExecuteAsync(IShellState shellState, HttpState programState, ICoreParseResult parseResult, CancellationToken cancellationToken)
        {
            parseResult = parseResult ?? throw new ArgumentNullException(nameof(parseResult));

            programState = programState ?? throw new ArgumentNullException(nameof(programState));

            shellState = shellState ?? throw new ArgumentNullException(nameof(shellState));

            if (programState.BaseAddress == null)
            {
                shellState.ConsoleManager.Error.WriteLine(Strings.UICommand_NotConnectedToServerError.SetColor(programState.ErrorColor));
                return Task.CompletedTask;
            }

            Uri uri = null;

            // Try to use the parameter first, if there was one.
            if (parseResult.Sections.Count > 1)
            {
                string parameter = parseResult.Sections[1];

                if (Uri.IsWellFormedUriString(parameter, UriKind.Absolute))
                {
                    uri = new Uri(parameter, UriKind.Absolute);
                }
                else if (!Uri.TryCreate(programState.BaseAddress, parameter, out uri))
                {
                    uri = null;
                }

                // If they specified a parameter and it failed, bail out
                if (uri is null)
                {
                    shellState.ConsoleManager.Error.WriteLine(string.Format(Strings.UICommand_InvalidParameter, parameter).SetColor(programState.ErrorColor));
                    return Task.CompletedTask;
                }
            }

            // If no parameter specified, check the preferences or use the default
            if (uri is null)
            {
                string uiEndpoint = _preferences.GetValue(WellKnownPreference.SwaggerUIEndpoint, "swagger");
                if (Uri.IsWellFormedUriString(uiEndpoint, UriKind.Absolute))
                {
                    uri = new Uri(uiEndpoint, UriKind.Absolute);
                }
                else
                {
                    uri = new Uri(programState.BaseAddress, uiEndpoint);
                }
            }

            return _uriLauncher.LaunchUriAsync(uri);
        }

        private string _HelpText;
        private string HelpText
        {
            get
            {
                if (_HelpText is null)
                {
                    string parameter = "{swaggerUIAddress}";
                    string defaultAddress = "[BaseAddress]/swagger";
                    var helpText = new StringBuilder();
                    helpText.Append(Strings.Usage.Bold());
                    helpText.AppendLine("ui [{swaggerUIAddress}]");
                    helpText.AppendLine();
                    helpText.AppendLine(Strings.UICommand_Description);
                    helpText.AppendLine();
                    helpText.AppendLine(string.Format(Strings.UICommand_HelpText_Line1, Name));
                    helpText.AppendLine(string.Format(Strings.UICommand_HelpText_Line2, parameter));
                    helpText.AppendLine(string.Format(Strings.UICommand_HelpText_Line3, WellKnownPreference.SwaggerUIEndpoint));
                    helpText.AppendLine(string.Format(Strings.UICommand_HelpText_Line4, defaultAddress));

                    _HelpText = helpText.ToString();
                }
                return _HelpText;
            }
        }

        public string GetHelpDetails(IShellState shellState, HttpState programState, ICoreParseResult parseResult)
        {
            if (parseResult.ContainsAtLeast(Name))
            {
                return HelpText;
            }

            return null;
        }

        public string GetHelpSummary(IShellState shellState, HttpState programState)
        {
            return Resources.Strings.UICommand_HelpSummary;
        }

        public IEnumerable<string> Suggest(IShellState shellState, HttpState programState, ICoreParseResult parseResult)
        {
            parseResult = parseResult ?? throw new ArgumentNullException(nameof(parseResult));

            if (parseResult.SelectedSection == 0 &&
                (string.IsNullOrEmpty(parseResult.Sections[parseResult.SelectedSection]) || Name.StartsWith(parseResult.Sections[0].Substring(0, parseResult.CaretPositionWithinSelectedSection), StringComparison.OrdinalIgnoreCase)))
            {
                return new[] { Name };
            }

            return null;
        }
    }
}
