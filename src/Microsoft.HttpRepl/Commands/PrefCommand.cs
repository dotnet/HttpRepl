// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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
    public class PrefCommand : CommandWithStructuredInputBase<HttpState, ICoreParseResult>
    {
        private const string _CommandSyntax = "pref [get/set] {setting} [{value}]";
        private const string _GetCommandSyntax = "pref get [{setting}]";
        private const string _SetCommandSyntax = "pref set {setting} [{value}]";
        private readonly HashSet<string> _allowedSubcommands = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {"get", "set"};
        private readonly IPreferences _preferences;

        public PrefCommand(IPreferences preferences)
        {
            _preferences = preferences;
        }

        public override string GetHelpSummary(IShellState shellState, HttpState programState)
        {
            return string.Format(Resources.Strings.PrefCommand_HelpSummary, _CommandSyntax);
        }

        protected override bool CanHandle(IShellState shellState, HttpState programState, DefaultCommandInput<ICoreParseResult> commandInput)
        {
            commandInput = commandInput ?? throw new ArgumentNullException(nameof(commandInput));

            shellState = shellState ?? throw new ArgumentNullException(nameof(shellState));

            if (commandInput.Arguments.Count == 0 || !_allowedSubcommands.Contains(commandInput.Arguments[0]?.Text))
            {
                shellState.ConsoleManager.Error.WriteLine(Resources.Strings.PrefCommand_Error_NoGetOrSet);
                return false;
            }

            if (!string.Equals("get", commandInput.Arguments[0].Text, StringComparison.OrdinalIgnoreCase) && (commandInput.Arguments.Count < 2 || string.IsNullOrEmpty(commandInput.Arguments[1]?.Text)))
            {
                shellState.ConsoleManager.Error.WriteLine(Resources.Strings.PrefCommand_Error_NoPreferenceName);
                return false;
            }

            return true;
        }

        protected override string GetHelpDetails(IShellState shellState, HttpState programState, DefaultCommandInput<ICoreParseResult> commandInput, ICoreParseResult parseResult)
        {
            var helpText = new StringBuilder();
            helpText.Append(Resources.Strings.Help_Usage.Bold());

            commandInput = commandInput ?? throw new ArgumentNullException(nameof(commandInput));

            if (commandInput.Arguments.Count == 0 || !_allowedSubcommands.Contains(commandInput.Arguments[0]?.Text))
            {
                helpText.AppendFormat(Resources.Strings.PrefCommand_HelpDetails_Syntax, _CommandSyntax);
            }
            else if (string.Equals(commandInput.Arguments[0].Text, "get", StringComparison.OrdinalIgnoreCase))
            {
                helpText.AppendFormat(Resources.Strings.PrefCommand_HelpDetails_GetSyntax, _GetCommandSyntax);
            }
            else
            {
                helpText.AppendFormat(Resources.Strings.PrefCommand_HelpDetails_SetSyntax, _SetCommandSyntax);
            }

            helpText.AppendLine();
            helpText.AppendLine(Resources.Strings.PrefCommand_HelpDetails_DefaultPreferences);
            foreach (var pref in _preferences.DefaultPreferences)
            {
                var val = pref.Value;
                if (pref.Key.Contains("colors", StringComparison.OrdinalIgnoreCase))
                {
                    val = GetColor(val);
                }
                helpText.AppendLine($"{pref.Key,-50}{val}");
            }
            helpText.AppendLine();
            helpText.AppendLine(Resources.Strings.PrefCommand_HelpDetails_CurrentPreferences);
            foreach (var pref in _preferences.CurrentPreferences)
            {
                var val = pref.Value;
                if (pref.Key.Contains("colors", StringComparison.OrdinalIgnoreCase))
                {
                    val = GetColor(val);
                }
                helpText.AppendLine($"{pref.Key,-50}{val}");
            }

            return helpText.ToString();
        }

        private static string GetColor(string value)
        {
            if (value.Contains("Bold", StringComparison.OrdinalIgnoreCase))
            {
                value = value.Bold();
            }

            if (value.Contains("Yellow", StringComparison.OrdinalIgnoreCase))
            {
                value = value.Yellow();
            }

            if (value.Contains("Cyan", StringComparison.OrdinalIgnoreCase))
            {
                value = value.Cyan();
            }

            if (value.Contains("Magenta", StringComparison.OrdinalIgnoreCase))
            {
                value = value.Magenta();
            }

            if (value.Contains("Green", StringComparison.OrdinalIgnoreCase))
            {
                value = value.Green();
            }

            if (value.Contains("White", StringComparison.OrdinalIgnoreCase))
            {
                value = value.White();
            }

            if (value.Contains("Black", StringComparison.OrdinalIgnoreCase))
            {
                value = value.Black();
            }

            return value;
        }

        protected override Task ExecuteAsync(IShellState shellState, HttpState programState, DefaultCommandInput<ICoreParseResult> commandInput, ICoreParseResult parseResult, CancellationToken cancellationToken)
        {
            commandInput = commandInput ?? throw new ArgumentNullException(nameof(commandInput));

            shellState = shellState ?? throw new ArgumentNullException(nameof(shellState));

            programState = programState ?? throw new ArgumentNullException(nameof(programState));

            if (string.Equals(commandInput.Arguments[0].Text, "get", StringComparison.OrdinalIgnoreCase))
            {
                GetSetting(shellState, programState, commandInput);
            }
            else
            {
                SetSetting(shellState, programState, commandInput);
            }

            return Task.CompletedTask;
        }

        private void SetSetting(IShellState shellState, HttpState programState, DefaultCommandInput<ICoreParseResult> commandInput)
        {
            string prefName = commandInput.Arguments[1].Text;
            string prefValue = commandInput.Arguments.Count > 2 ? commandInput.Arguments[2]?.Text : null;

            if (!_preferences.SetValue(prefName, prefValue))
            {
                shellState.ConsoleManager.Error.WriteLine(Resources.Strings.PrefCommand_Error_Saving.SetColor(programState.ErrorColor));
            }
        }

        private void GetSetting(IShellState shellState, HttpState programState, DefaultCommandInput<ICoreParseResult> commandInput)
        {
            string preferenceName = commandInput.Arguments.Count > 1 ? commandInput.Arguments[1]?.Text : null;
            
            //If there's a particular setting to get the value of
            if (!string.IsNullOrEmpty(preferenceName))
            {
                if (_preferences.TryGetValue(preferenceName, out string value))
                {
                    shellState.ConsoleManager.WriteLine(string.Format(Resources.Strings.PrefCommand_Get_ConfiguredValue, value));
                }
                else
                {
                    shellState.ConsoleManager.Error.WriteLine(string.Format(Resources.Strings.PrefCommand_Error_NoConfiguredValue, commandInput.Arguments[1].Text).SetColor(programState.ErrorColor));
                }
            }
            else
            {
                foreach (KeyValuePair<string, string> entry in _preferences.CurrentPreferences.OrderBy(x => x.Key))
                {
                    shellState.ConsoleManager.WriteLine($"{entry.Key}={entry.Value}");
                }
            }

        }

        public override CommandInputSpecification InputSpec { get; } = CommandInputSpecification.Create("pref")
            .MinimumArgCount(1)
            .MaximumArgCount(3)
            .Finish();


        protected override IEnumerable<string> GetArgumentSuggestionsForText(IShellState shellState, HttpState programState, ICoreParseResult parseResult, DefaultCommandInput<ICoreParseResult> commandInput, string normalCompletionString)
        {
            parseResult = parseResult ?? throw new ArgumentNullException(nameof(parseResult));

            if (parseResult.SelectedSection == 1)
            {
                return _allowedSubcommands.Where(x => x.StartsWith(normalCompletionString, StringComparison.OrdinalIgnoreCase));
            }

            if (parseResult.SelectedSection == 2)
            {
                string prefix = parseResult.Sections.Count > 2 ? normalCompletionString : string.Empty;
                List<string> matchingProperties = new List<string>();

                foreach (string val in WellKnownPreference.Catalog.Names)
                {
                    if (val.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        matchingProperties.Add(val);
                    }
                }

                return matchingProperties;
            }

            if (parseResult.SelectedSection == 3
                && parseResult.Sections[2].StartsWith("colors.", StringComparison.OrdinalIgnoreCase))
            {
                string prefix = parseResult.Sections.Count > 3 ? normalCompletionString : string.Empty;
                List<string> matchingProperties = new List<string>();

                foreach (string val in Enum.GetNames(typeof(AllowedColors)))
                {
                    if (val.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        matchingProperties.Add(val);
                    }
                }

                return matchingProperties;
            }

            return null;
        }
    }
}
