// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.FileSystem;
using Microsoft.HttpRepl.Preferences;
using Microsoft.HttpRepl.UserProfile;
using Microsoft.Repl;
using Microsoft.Repl.Commanding;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Parsing;

namespace Microsoft.HttpRepl
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await new Program().Start(args);
        }

        public async Task Start(string[] args, IConsoleManager consoleManager = null, IPreferences preferences = null)
        {
            args = args ?? throw new ArgumentNullException(nameof(args));

            ComposeDependencies(ref consoleManager, ref preferences, out HttpState state, out Shell shell);

            if (Console.IsOutputRedirected && !consoleManager.AllowOutputRedirection)
            {
                Reporter.Error.WriteLine(Resources.Strings.Error_OutputRedirected.SetColor(preferences.GetColorValue(WellKnownPreference.ErrorColor)));
                return;
            }

            using (CancellationTokenSource source = new CancellationTokenSource())
            {
                shell.ShellState.ConsoleManager.AddBreakHandler(() => source.Cancel());
                if (args.Length > 0)
                {
                    if (string.Equals(args[0], "--help", StringComparison.OrdinalIgnoreCase) || string.Equals(args[0], "-h", StringComparison.OrdinalIgnoreCase))
                    {
                        shell.ShellState.ConsoleManager.WriteLine(Resources.Strings.Help_Usage);
                        shell.ShellState.ConsoleManager.WriteLine("  dotnet httprepl [<BASE_ADDRESS>] [options]");
                        shell.ShellState.ConsoleManager.WriteLine();
                        shell.ShellState.ConsoleManager.WriteLine(Resources.Strings.Help_Arguments);
                        shell.ShellState.ConsoleManager.WriteLine(string.Format(Resources.Strings.Help_BaseAddress, "<BASE_ADDRESS>"));
                        shell.ShellState.ConsoleManager.WriteLine();
                        shell.ShellState.ConsoleManager.WriteLine(Resources.Strings.Help_Options);
                        shell.ShellState.ConsoleManager.WriteLine(string.Format(Resources.Strings.Help_Help, "-h|--help"));

                        shell.ShellState.ConsoleManager.WriteLine();
                        shell.ShellState.ConsoleManager.WriteLine(Resources.Strings.Help_REPLCommands);
                        new HelpCommand(preferences).CoreGetHelp(shell.ShellState, (ICommandDispatcher<HttpState, ICoreParseResult>)shell.ShellState.CommandDispatcher, state);
                        return;
                    }

                    // allow running a script file directly.
                    if (string.Equals(args[0], "run", StringComparison.OrdinalIgnoreCase))
                    {
                        shell.ShellState.CommandDispatcher.OnReady(shell.ShellState);
                        shell.ShellState.InputManager.SetInput(shell.ShellState, string.Join(' ', args));
                        await shell.ShellState.CommandDispatcher.ExecuteCommandAsync(shell.ShellState, CancellationToken.None).ConfigureAwait(false);
                        return;
                    }

                    string combinedArgs = string.Join(' ', args);

                    shell.ShellState.CommandDispatcher.OnReady(shell.ShellState);
                    shell.ShellState.InputManager.SetInput(shell.ShellState, $"connect {combinedArgs}");
                    await shell.ShellState.CommandDispatcher.ExecuteCommandAsync(shell.ShellState, CancellationToken.None).ConfigureAwait(false);
                }

                await shell.RunAsync(source.Token).ConfigureAwait(false);
            }
        }

        private static void ComposeDependencies(ref IConsoleManager consoleManager, ref IPreferences preferences, out HttpState state, out Shell shell)
        {
            consoleManager = consoleManager ?? new ConsoleManager();
            IFileSystem fileSystem = new RealFileSystem();
            preferences = preferences ?? new UserFolderPreferences(fileSystem, new UserProfileDirectoryProvider(), CreateDefaultPreferences());
            var httpClient = GetHttpClientWithPreferences(preferences);
            state = new HttpState(fileSystem, preferences, httpClient);

            var dispatcher = DefaultCommandDispatcher.Create(state.GetPrompt, state);
            dispatcher.AddCommand(new ChangeDirectoryCommand());
            dispatcher.AddCommand(new ClearCommand());
            dispatcher.AddCommand(new ConnectCommand(preferences));
            dispatcher.AddCommand(new DeleteCommand(fileSystem, preferences));
            dispatcher.AddCommand(new EchoCommand());
            dispatcher.AddCommand(new ExitCommand());
            dispatcher.AddCommand(new HeadCommand(fileSystem, preferences));
            dispatcher.AddCommand(new HelpCommand(preferences));
            dispatcher.AddCommand(new GetCommand(fileSystem, preferences));
            dispatcher.AddCommand(new ListCommand(preferences));
            dispatcher.AddCommand(new OptionsCommand(fileSystem, preferences));
            dispatcher.AddCommand(new PatchCommand(fileSystem, preferences));
            dispatcher.AddCommand(new PrefCommand(preferences));
            dispatcher.AddCommand(new PostCommand(fileSystem, preferences));
            dispatcher.AddCommand(new PutCommand(fileSystem, preferences));
            dispatcher.AddCommand(new RunCommand(fileSystem));
            dispatcher.AddCommand(new SetBaseCommand());
            dispatcher.AddCommand(new SetHeaderCommand());
            dispatcher.AddCommand(new UICommand(new UriLauncher(), preferences));

            shell = new Shell(dispatcher, consoleManager: consoleManager);
        }

        internal static Dictionary<string, string> CreateDefaultPreferences()
        {
            return new Dictionary<string, string>
            {
                { WellKnownPreference.ProtocolColor, "BoldGreen" },
                { WellKnownPreference.StatusColor, "BoldYellow" },

                { WellKnownPreference.JsonArrayBraceColor, "BoldCyan" },
                { WellKnownPreference.JsonCommaColor, "BoldYellow" },
                { WellKnownPreference.JsonNameColor, "BoldMagenta" },
                { WellKnownPreference.JsonNameSeparatorColor, "BoldWhite" },
                { WellKnownPreference.JsonObjectBraceColor, "Cyan" },
                { WellKnownPreference.JsonColor, "Green" }
            };
        }

        private static HttpClient GetHttpClientWithPreferences(IPreferences preferences)
        {
            if (preferences.GetBoolValue(WellKnownPreference.UseDefaultCredentials))
            {
#pragma warning disable CA2000 // Dispose objects before losing scope
                return new HttpClient(new HttpClientHandler { UseDefaultCredentials = true });
#pragma warning restore CA2000 // Dispose objects before losing scope
            }

            return new HttpClient();
        }
    }
}
