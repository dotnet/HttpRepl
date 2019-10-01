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
    public class ConnectCommand : CommandWithStructuredInputBase<HttpState, ICoreParseResult>
    {
        private const string BaseAddressOption = nameof(BaseAddressOption);
        private const string SwaggerAddressOption = nameof(SwaggerAddressOption);
        private const string Name = "connect";

        private readonly IPreferences _preferences;

        public ConnectCommand(IPreferences preferences)
        {
            _preferences = preferences;
        }

        public override CommandInputSpecification InputSpec => CommandInputSpecification.Create("connect")
                                                                                        .MinimumArgCount(0)
                                                                                        .MaximumArgCount(1)
                                                                                        .WithOption(new CommandOptionSpecification(id: BaseAddressOption,
                                                                                                                                   requiresValue: true,
                                                                                                                                   minimumOccurrences: 0,
                                                                                                                                   maximumOccurrences: 1,
                                                                                                                                   forms: new[] { "--base", "-b" }))
                                                                                        .WithOption(new CommandOptionSpecification(id: SwaggerAddressOption,
                                                                                                                                   requiresValue: true,
                                                                                                                                   minimumOccurrences: 0,
                                                                                                                                   maximumOccurrences: 1,
                                                                                                                                   forms: new[] { "--swagger", "-s" }))
                                                                                        .Finish();

        public override string GetHelpSummary(IShellState shellState, HttpState programState)
        {
            return Resources.Strings.ConnectCommand_Description;
        }

        protected override string GetHelpDetails(IShellState shellState, HttpState programState, DefaultCommandInput<ICoreParseResult> commandInput, ICoreParseResult parseResult)
        {
            if (parseResult.ContainsAtLeast(Name))
            {
                var helpText = new StringBuilder();
                helpText.Append(Resources.Strings.Usage.Bold());
                helpText.AppendLine("connect [rootAddress] [--base baseAddress] [--swagger swaggerAddress]");
                helpText.AppendLine();
                helpText.AppendLine(Resources.Strings.ConnectCommand_HelpDetails_Line1);
                helpText.AppendLine();
                helpText.AppendLine(Resources.Strings.ConnectCommand_HelpDetails_Line2);
                helpText.AppendLine(Resources.Strings.ConnectCommand_HelpDetails_Line3);
                return helpText.ToString();
            }
            return null;
        }

        protected override async Task ExecuteAsync(IShellState shellState, HttpState programState, DefaultCommandInput<ICoreParseResult> commandInput, ICoreParseResult parseResult, CancellationToken cancellationToken)
        {
            commandInput = commandInput ?? throw new ArgumentNullException(nameof(commandInput));

            shellState = shellState ?? throw new ArgumentNullException(nameof(shellState));

            programState = programState ?? throw new ArgumentNullException(nameof(programState));

            string rootAddress = commandInput.Arguments.SingleOrDefault()?.Text?.EnsureTrailingSlash();
            string baseAddress = GetBaseAddressFromCommand(commandInput)?.EnsureTrailingSlash();
            string swaggerAddress = GetSwaggerAddressFromCommand(commandInput);

            ApiConnection connectionInfo = GetConnectionInfo(shellState, programState, rootAddress, baseAddress, swaggerAddress, _preferences);

            if (connectionInfo is null)
            {
                return;
            }

            await connectionInfo.SetupHttpState(programState, performAutoDetect: true, cancellationToken);

            WriteStatus(shellState, programState);
        }


        private static void WriteStatus(IShellState shellState, HttpState programState)
        {
            if (programState.BaseAddress is null)
            {
                shellState.ConsoleManager.WriteLine(Resources.Strings.ConnectCommand_Status_NoBase.SetColor(programState.WarningColor));
            }
            else
            {
                shellState.ConsoleManager.WriteLine(string.Format(Resources.Strings.ConnectCommand_Status_Base, programState.BaseAddress));
            }

            if (programState.SwaggerEndpoint is null)
            {
                shellState.ConsoleManager.WriteLine(Resources.Strings.ConnectCommand_Status_NoSwagger.SetColor(programState.WarningColor));
            }
            else
            {
                shellState.ConsoleManager.WriteLine(string.Format(Resources.Strings.ConnectCommand_Status_Swagger, programState.SwaggerEndpoint));
            }
        }

        private static ApiConnection GetConnectionInfo(IShellState shellState, HttpState programState, string rootAddress, string baseAddress, string swaggerAddress, IPreferences preferences)
        {
            rootAddress = rootAddress?.Trim();
            baseAddress = baseAddress?.Trim();
            swaggerAddress = swaggerAddress?.Trim();

            if (string.IsNullOrWhiteSpace(rootAddress) && string.IsNullOrWhiteSpace(baseAddress) && string.IsNullOrWhiteSpace(swaggerAddress))
            {
                shellState.ConsoleManager.Error.WriteLine(Resources.Strings.ConnectCommand_Error_NothingSpecified);
                return null;
            }

            if (!string.IsNullOrWhiteSpace(rootAddress) && !Uri.IsWellFormedUriString(rootAddress, UriKind.Absolute))
            {
                shellState.ConsoleManager.Error.WriteLine(Resources.Strings.ConnectCommand_Error_RootAddressNotValid);
                return null;
            }

            ApiConnection apiConnection = new ApiConnection(preferences);
            if (!string.IsNullOrWhiteSpace(rootAddress))
            {
                apiConnection.RootUri = new Uri(rootAddress, UriKind.Absolute);
            }

            if (!SetupBaseAddress(shellState, baseAddress, apiConnection) || !SetupSwaggerAddress(shellState, swaggerAddress, apiConnection))
            {
                return null;
            }

            apiConnection.AllowBaseOverrideBySwagger = !apiConnection.HasBaseUri;

            if (apiConnection.HasRootUri && !apiConnection.HasBaseUri)
            {
                apiConnection.BaseUri = apiConnection.RootUri;
            }

            return apiConnection;
        }

        private static bool SetupSwaggerAddress(IShellState shellState, string swaggerAddress, ApiConnection connectionInfo)
        {
            if (!string.IsNullOrWhiteSpace(swaggerAddress))
            {
                if (!connectionInfo.HasRootUri && !Uri.IsWellFormedUriString(swaggerAddress, UriKind.Absolute))
                {
                    shellState.ConsoleManager.Error.WriteLine(Resources.Strings.ConnectCommand_Error_NoRootNoAbsoluteSwagger);
                    return false;
                }
                else if (connectionInfo.HasRootUri && !Uri.IsWellFormedUriString(swaggerAddress, UriKind.RelativeOrAbsolute))
                {
                    shellState.ConsoleManager.Error.WriteLine(Resources.Strings.ConnectCommand_Error_InvalidSwagger);
                    return false;
                }

                if (Uri.IsWellFormedUriString(swaggerAddress, UriKind.Absolute))
                {
                    connectionInfo.SwaggerUri = new Uri(swaggerAddress, UriKind.Absolute);
                }
                else if (Uri.IsWellFormedUriString(swaggerAddress, UriKind.Relative))
                {
                    connectionInfo.SwaggerUri = new Uri(connectionInfo.RootUri, swaggerAddress);
                }
            }

            return true;
        }

        private static bool SetupBaseAddress(IShellState shellState, string baseAddress, ApiConnection connectionInfo)
        {
            if (!string.IsNullOrWhiteSpace(baseAddress))
            {
                if (!connectionInfo.HasRootUri && !Uri.IsWellFormedUriString(baseAddress, UriKind.Absolute))
                {
                    shellState.ConsoleManager.Error.WriteLine(Resources.Strings.ConnectCommand_Error_NoRootNoAbsoluteBase);
                    return false;
                }
                else if (connectionInfo.HasRootUri && !Uri.IsWellFormedUriString(baseAddress, UriKind.RelativeOrAbsolute))
                {
                    shellState.ConsoleManager.Error.WriteLine(Resources.Strings.ConnectCommand_Error_InvalidBase);
                    return false;
                }

                if (Uri.IsWellFormedUriString(baseAddress, UriKind.Absolute))
                {
                    connectionInfo.BaseUri = new Uri(baseAddress, UriKind.Absolute);
                }
                else if (Uri.IsWellFormedUriString(baseAddress, UriKind.Relative))
                {
                    connectionInfo.BaseUri = new Uri(connectionInfo.RootUri, baseAddress);
                }
            }

            return true;
        }

        private static string GetBaseAddressFromCommand(DefaultCommandInput<ICoreParseResult> commandInput)
        {
            return GetOptionValueFromCommand(commandInput, BaseAddressOption);
        }

        private static string GetSwaggerAddressFromCommand(DefaultCommandInput<ICoreParseResult> commandInput)
        {
            return GetOptionValueFromCommand(commandInput, SwaggerAddressOption);
        }

        private static string GetOptionValueFromCommand(DefaultCommandInput<ICoreParseResult> commandInput, string optionId)
        {
            if (commandInput.Options.TryGetValue(optionId, out IReadOnlyList<InputElement> inputElements))
            {
                InputElement inputElement = inputElements.Any() ? inputElements[0] : null;
                return inputElement?.Text;
            }

            return null;
        }

        
    }
}
