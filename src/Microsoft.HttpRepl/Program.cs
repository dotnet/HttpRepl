// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.FileSystem;
using Microsoft.Repl;
using Microsoft.Repl.Commanding;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Parsing;

namespace Microsoft.HttpRepl
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IFileSystem, RealFileSystem>()
                .AddSingleton<ICommand<HttpState, ICoreParseResult>, ChangeDirectoryCommand>()
                .AddSingleton<ICommand<HttpState, ICoreParseResult>, ClearCommand>()
                //.AddSingleton<ICommand<HttpState, ICoreParseResult>, ConfigCommand>()
                .AddSingleton<ICommand<HttpState, ICoreParseResult>, DeleteCommand>()
                .AddSingleton<ICommand<HttpState, ICoreParseResult>, EchoCommand>()
                .AddSingleton<ICommand<HttpState, ICoreParseResult>, ExitCommand>()
                .AddSingleton<ICommand<HttpState, ICoreParseResult>, HeadCommand>()
                .AddSingleton<ICommand<HttpState, ICoreParseResult>, HelpCommand>()
                .AddSingleton<ICommand<HttpState, ICoreParseResult>, GetCommand>()
                .AddSingleton<ICommand<HttpState, ICoreParseResult>, ListCommand>()
                .AddSingleton<ICommand<HttpState, ICoreParseResult>, OptionsCommand>()
                .AddSingleton<ICommand<HttpState, ICoreParseResult>, PatchCommand>()
                .AddSingleton<ICommand<HttpState, ICoreParseResult>, PrefCommand>()
                .AddSingleton<ICommand<HttpState, ICoreParseResult>, PostCommand>()
                .AddSingleton<ICommand<HttpState, ICoreParseResult>, PutCommand>()
                .AddSingleton<ICommand<HttpState, ICoreParseResult>, RunCommand>()
                .AddSingleton<ICommand<HttpState, ICoreParseResult>, SetBaseCommand>()
                .AddSingleton<ICommand<HttpState, ICoreParseResult>, SetDiagCommand>()
                .AddSingleton<ICommand<HttpState, ICoreParseResult>, SetHeaderCommand>()
                .AddSingleton<ICommand<HttpState, ICoreParseResult>, SetSwaggerCommand>()
                .AddSingleton<ICommand<HttpState, ICoreParseResult>, UICommand>()
                .BuildServiceProvider();

            HttpState state = new HttpState(serviceProvider.GetService<IFileSystem>());

            if (Console.IsOutputRedirected)
            {
                Reporter.Error.WriteLine("Cannot start the REPL when output is being redirected".SetColor(state.ErrorColor));
                return;
            }

            var dispatcher = DefaultCommandDispatcher.Create(state.GetPrompt, state);

            IEnumerable<ICommand<HttpState, ICoreParseResult>> commands = serviceProvider.GetServices<ICommand<HttpState, ICoreParseResult>>();

            foreach (ICommand<HttpState, ICoreParseResult> command in commands)
            {
                dispatcher.AddCommand(command);
            }

            CancellationTokenSource source = new CancellationTokenSource();
            Shell shell = new Shell(dispatcher);
            shell.ShellState.ConsoleManager.AddBreakHandler(() => source.Cancel());
            if (args.Length > 0)
            {
                if (string.Equals(args[0], "--help", StringComparison.OrdinalIgnoreCase) || string.Equals(args[0], "-h", StringComparison.OrdinalIgnoreCase))
                {
                    shell.ShellState.ConsoleManager.WriteLine("Usage: dotnet httprepl [<BASE_ADDRESS>] [options]");
                    shell.ShellState.ConsoleManager.WriteLine();
                    shell.ShellState.ConsoleManager.WriteLine("Arguments:");
                    shell.ShellState.ConsoleManager.WriteLine("  <BASE_ADDRESS> - The initial base address for the REPL.");
                    shell.ShellState.ConsoleManager.WriteLine();
                    shell.ShellState.ConsoleManager.WriteLine("Options:");
                    shell.ShellState.ConsoleManager.WriteLine("  --help - Show help information.");

                    shell.ShellState.ConsoleManager.WriteLine();
                    shell.ShellState.ConsoleManager.WriteLine("REPL Commands:");
                    new HelpCommand().CoreGetHelp(shell.ShellState, (ICommandDispatcher<HttpState, ICoreParseResult>)shell.ShellState.CommandDispatcher, state);
                    return;
                }

                shell.ShellState.CommandDispatcher.OnReady(shell.ShellState);
                shell.ShellState.InputManager.SetInput(shell.ShellState, $"set base \"{args[0]}\"");
                await shell.ShellState.CommandDispatcher.ExecuteCommandAsync(shell.ShellState, CancellationToken.None).ConfigureAwait(false);
            }
            Task result = shell.RunAsync(source.Token);
            await result.ConfigureAwait(false);
        }
    }
}
