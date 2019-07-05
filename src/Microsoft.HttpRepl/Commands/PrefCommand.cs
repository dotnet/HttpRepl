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

        protected override bool CanHandle(IShellState shellState, HttpState programState, DefaultCommandInput<ICoreParseResult> commandInput)
        {
            if (commandInput.Arguments.Count == 0 || !_allowedSubcommands.Contains(commandInput.Arguments[0]?.Text))
            {
                shellState.ConsoleManager.Error.WriteLine(Resources.Strings.PrefCommand_Error_NoGetOrSet);
                return false;
            }

            if (!string.Equals("get", commandInput.Arguments[0].Text) && (commandInput.Arguments.Count < 2 || string.IsNullOrEmpty(commandInput.Arguments[1]?.Text)))
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
            foreach (var pref in programState.DefaultPreferences)
            {
                var val = pref.Value;
                if (pref.Key.Contains("colors"))
                {
                    val = GetColor(val);
                }
                helpText.AppendLine($"{pref.Key,-50}{val}");
            }
            helpText.AppendLine();
            helpText.AppendLine(Resources.Strings.PrefCommand_HelpDetails_CurrentPreferences);
            foreach (var pref in programState.Preferences)
            {
                var val = pref.Value;
                if (pref.Key.Contains("colors"))
                {
                    val = GetColor(val);
                }
                helpText.AppendLine($"{pref.Key,-50}{val}");
            }

            return helpText.ToString();
        }

        private static string GetColor(string value)
        {
            if (value.Contains("Bold"))
            {
                value = value.Bold();
            }

            if (value.Contains("Yellow"))
            {
                value = value.Yellow();
            }

            if (value.Contains("Cyan"))
            {
                value = value.Cyan();
            }

            if (value.Contains("Magenta"))
            {
                value = value.Magenta();
            }

            if (value.Contains("Green"))
            {
                value = value.Green();
            }

            if (value.Contains("White"))
            {
                value = value.White();
            }

            if (value.Contains("Black"))
            {
                value = value.Black();
            }

            return value;
        }

        protected override Task ExecuteAsync(IShellState shellState, HttpState programState, DefaultCommandInput<ICoreParseResult> commandInput, ICoreParseResult parseResult, CancellationToken cancellationToken)
        {
            if (string.Equals(commandInput.Arguments[0].Text, "get", StringComparison.OrdinalIgnoreCase))
            {
                return GetSetting(shellState, programState, commandInput);
            }

            return SetSetting(shellState, programState, commandInput);
        }

        private static Task SetSetting(IShellState shellState, HttpState programState, DefaultCommandInput<ICoreParseResult> commandInput)
        {
            string prefName = commandInput.Arguments[1].Text;
            string prefValue = commandInput.Arguments.Count > 2 ? commandInput.Arguments[2]?.Text : null;

            if (string.IsNullOrEmpty(prefValue))
            {
                if (!programState.DefaultPreferences.TryGetValue(prefName, out string defaultValue))
                {
                    programState.Preferences.Remove(prefName);
                }
                else
                {
                    programState.Preferences[prefName] = defaultValue;
                }
            }
            else
            {
                programState.Preferences[prefName] = prefValue;
            }

            if (!programState.SavePreferences())
            {
                shellState.ConsoleManager.Error.WriteLine(Resources.Strings.PrefCommand_Error_Saving.SetColor(programState.ErrorColor));
            }

            return Task.CompletedTask;
        }

        private static Task GetSetting(IShellState shellState, HttpState programState, DefaultCommandInput<ICoreParseResult> commandInput)
        {
            string preferenceName = commandInput.Arguments.Count > 1 ? commandInput.Arguments[1]?.Text : null;
            
            //If there's a particular setting to get the value of
            if (!string.IsNullOrEmpty(preferenceName))
            {
                if (programState.Preferences.TryGetValue(preferenceName, out string value))
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
                foreach (KeyValuePair<string, string> entry in programState.Preferences.OrderBy(x => x.Key))
                {
                    shellState.ConsoleManager.WriteLine($"{entry.Key}={entry.Value}");
                }
            }

            return Task.CompletedTask;
        }

        public override CommandInputSpecification InputSpec { get; } = CommandInputSpecification.Create("pref")
            .MinimumArgCount(1)
            .MaximumArgCount(3)
            .Finish();


        protected override IEnumerable<string> GetArgumentSuggestionsForText(IShellState shellState, HttpState programState, ICoreParseResult parseResult, DefaultCommandInput<ICoreParseResult> commandInput, string normalCompletionString)
        {
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
