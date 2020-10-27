// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.FileSystem;
using Microsoft.HttpRepl.Preferences;
using Microsoft.HttpRepl.Telemetry;
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
            await Start(args);
        }

        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "CA1062 Doesn't understand that ComposeDependencies ensures `consoleManager`, `preferences` and `telemetry` are non-null before use.")]
        public static async Task Start(string[] args, IConsoleManager consoleManager = null, IPreferences preferences = null, ITelemetry telemetry = null)
        {
            args = args ?? throw new ArgumentNullException(nameof(args));

            RegisterEncodingProviders();
            ComposeDependencies(ref consoleManager, ref preferences, ref telemetry, out HttpState state, out Shell shell);

            if (!telemetry.FirstTimeUseNoticeSentinel.Exists() && !Telemetry.Telemetry.SkipFirstTimeExperience)
            {
                Reporter.Output.WriteLine(string.Format(Resources.Strings.Telemetry_WelcomeMessage, VersionSensor.AssemblyVersion.ToString(2)));
                telemetry.FirstTimeUseNoticeSentinel.CreateIfNotExists();
            }

            if (Console.IsOutputRedirected && !consoleManager.AllowOutputRedirection)
            {
                telemetry.TrackStartedEvent(withOutputRedirection: true);
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
                        telemetry.TrackStartedEvent(withHelp: true);
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
                        new HelpCommand().CoreGetHelp(shell.ShellState, (ICommandDispatcher<HttpState, ICoreParseResult>)shell.ShellState.CommandDispatcher, state);
                        return;
                    }

                    // allow running a script file directly.
                    if (string.Equals(args[0], "run", StringComparison.OrdinalIgnoreCase))
                    {
                        telemetry.TrackStartedEvent(withRun: true);
                        shell.ShellState.CommandDispatcher.OnReady(shell.ShellState);
                        shell.ShellState.InputManager.SetInput(shell.ShellState, string.Join(' ', args));
                        await shell.ShellState.CommandDispatcher.ExecuteCommandAsync(shell.ShellState, CancellationToken.None).ConfigureAwait(false);
                        return;
                    }

                    telemetry.TrackStartedEvent(withOtherArgs: args.Length > 0);

                    string combinedArgs = string.Join(' ', args);

                    shell.ShellState.CommandDispatcher.OnReady(shell.ShellState);
                    shell.ShellState.InputManager.SetInput(shell.ShellState, $"connect {combinedArgs}");
                    await shell.ShellState.CommandDispatcher.ExecuteCommandAsync(shell.ShellState, CancellationToken.None).ConfigureAwait(false);
                }
                else
                {
                    telemetry.TrackStartedEvent();
                }

                await shell.RunAsync(source.Token).ConfigureAwait(false);
            }
        }

        private static void ComposeDependencies(ref IConsoleManager consoleManager, ref IPreferences preferences, ref ITelemetry telemetry, out HttpState state, out Shell shell)
        {
            consoleManager ??= new ConsoleManager();
            IFileSystem fileSystem = new RealFileSystem();
            preferences ??= new UserFolderPreferences(fileSystem, new UserProfileDirectoryProvider(), CreateDefaultPreferences());
            telemetry ??= new Telemetry.Telemetry(VersionSensor.AssemblyInformationalVersion);
            var httpClient = GetHttpClientWithPreferences(preferences);
            state = new HttpState(fileSystem, preferences, httpClient);

            var dispatcher = DefaultCommandDispatcher.Create(state.GetPrompt, state);
            dispatcher.AddCommandWithTelemetry(telemetry, new ChangeDirectoryCommand());
            dispatcher.AddCommandWithTelemetry(telemetry, new ClearCommand());
            dispatcher.AddCommandWithTelemetry(telemetry, new ConnectCommand(preferences, telemetry));
            dispatcher.AddCommandWithTelemetry(telemetry, new DeleteCommand(fileSystem, preferences));
            dispatcher.AddCommandWithTelemetry(telemetry, new EchoCommand());
            dispatcher.AddCommandWithTelemetry(telemetry, new ExitCommand());
            dispatcher.AddCommandWithTelemetry(telemetry, new HeadCommand(fileSystem, preferences));
            dispatcher.AddCommandWithTelemetry(telemetry, new HelpCommand());
            dispatcher.AddCommandWithTelemetry(telemetry, new GetCommand(fileSystem, preferences));
            dispatcher.AddCommandWithTelemetry(telemetry, new ListCommand(preferences));
            dispatcher.AddCommandWithTelemetry(telemetry, new OptionsCommand(fileSystem, preferences));
            dispatcher.AddCommandWithTelemetry(telemetry, new PatchCommand(fileSystem, preferences));
            dispatcher.AddCommandWithTelemetry(telemetry, new PrefCommand(preferences, telemetry));
            dispatcher.AddCommandWithTelemetry(telemetry, new PostCommand(fileSystem, preferences));
            dispatcher.AddCommandWithTelemetry(telemetry, new PutCommand(fileSystem, preferences));
            dispatcher.AddCommandWithTelemetry(telemetry, new RunCommand(fileSystem));
            dispatcher.AddCommandWithTelemetry(telemetry, new SetHeaderCommand(telemetry));
            dispatcher.AddCommandWithTelemetry(telemetry, new UICommand(new UriLauncher(), preferences));

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
            bool useDefaultCredentials = preferences.GetBoolValue(WellKnownPreference.UseDefaultCredentials);
            bool proxyUseDefaultCredentials = preferences.GetBoolValue(WellKnownPreference.ProxyUseDefaultCredentials);

            if (useDefaultCredentials || proxyUseDefaultCredentials)
            {
#pragma warning disable CA2000 // Dispose objects before losing scope
                HttpClientHandler handler = new HttpClientHandler()
                {
                    UseDefaultCredentials = useDefaultCredentials,
                    DefaultProxyCredentials = proxyUseDefaultCredentials ? CredentialCache.DefaultCredentials : null
                };

                return new HttpClient(handler);
#pragma warning restore CA2000 // Dispose objects before losing scope
            }

            return new HttpClient();
        }

        private static void RegisterEncodingProviders()
        {
            // Adds Windows-1252, among others
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
    }
}
