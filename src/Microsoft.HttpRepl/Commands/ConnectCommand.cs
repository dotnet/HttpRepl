// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Preferences;
using Microsoft.HttpRepl.Telemetry;
using Microsoft.HttpRepl.Telemetry.Events;
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
        private const string VerbosityOption = nameof(VerbosityOption);
        private const string PersistHeadersOption = nameof(PersistHeadersOption);
        private const string PersistPathOption = nameof(PersistPathOption);
        public override string Name => "connect";
        private const string WebApiDefaultPathSuffix = "/swagger/";

        private readonly IPreferences _preferences;
        private readonly ITelemetry _telemetry;

        public ConnectCommand(IPreferences preferences, ITelemetry telemetry)
        {
            _preferences = preferences;
            _telemetry = telemetry;
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
                                                                                                                                   forms: new[] { "--openapi", "-o",
                                                                                                                                                  "--swagger", "-s" }))
                                                                                        .WithOption(new CommandOptionSpecification(id: VerbosityOption,
                                                                                                                                   acceptsValue: false,
                                                                                                                                   minimumOccurrences: 0,
                                                                                                                                   maximumOccurrences: 1,
                                                                                                                                   forms: new[] { "--verbose", "-v" }))
                                                                                        .WithOption(new CommandOptionSpecification(id: PersistHeadersOption,
                                                                                                                                   maximumOccurrences: 1,
                                                                                                                                   forms: new[] { "--persist-headers"}))
                                                                                        .WithOption(new CommandOptionSpecification(id: PersistPathOption,
                                                                                                                                   maximumOccurrences: 1,
                                                                                                                                   forms: new[] { "--persist-path"}))
                                                                                        .Finish();

        public override string GetHelpSummary(IShellState shellState, HttpState programState)
        {
            return Resources.Strings.ConnectCommand_Description;
        }

        protected override string GetHelpDetails(IShellState shellState, HttpState programState, DefaultCommandInput<ICoreParseResult> commandInput, ICoreParseResult parseResult)
        {
            if (parseResult.ContainsAtLeast(Name))
            {
                StringBuilder helpText = new StringBuilder();
                helpText.Append(Resources.Strings.Usage.Bold());
                helpText.AppendLine("connect [rootAddress] [--base baseAddress] [--openapi openApiDescriptionAddress] [--verbose] [--persist-headers] [--persist-paths]");
                helpText.AppendLine();
                helpText.AppendLine(Resources.Strings.ConnectCommand_HelpDetails_Line1);
                helpText.AppendLine();
                helpText.AppendLine(Resources.Strings.ConnectCommand_HelpDetails_Line2);
                helpText.AppendLine(Resources.Strings.ConnectCommand_HelpDetails_Line3);
                helpText.AppendLine(Resources.Strings.ConnectCommand_HelpDetails_Line4);
                helpText.AppendLine(Resources.Strings.ConnectCommand_HelpDetails_Line5);
                helpText.AppendLine(Resources.Strings.ConnectCommand_HelpDetails_Line6);
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
            bool isVerbosityEnabled = GetOptionExistsFromCommand(commandInput, VerbosityOption);
            bool persistHeaders = GetOptionExistsFromCommand(commandInput, PersistHeadersOption);
            bool persistPath = GetOptionExistsFromCommand(commandInput, PersistPathOption);

            ApiConnection connectionInfo = GetConnectionInfo(shellState, programState, rootAddress, baseAddress, swaggerAddress, _preferences, isVerbosityEnabled);

            bool rootSpecified = !string.IsNullOrWhiteSpace(rootAddress);
            bool baseSpecified = !string.IsNullOrWhiteSpace(baseAddress);
            bool openApiSpecified = !string.IsNullOrWhiteSpace(swaggerAddress);

            if (connectionInfo is null)
            {
                _telemetry.TrackEvent(new ConnectEvent(baseSpecified, rootSpecified, openApiSpecified, openApiFound: false));
                return;
            }

            await connectionInfo.SetupHttpState(programState, performAutoDetect: true, persistHeaders, persistPath, cancellationToken);

            bool openApiFound = connectionInfo?.HasSwaggerDocument == true;

            _telemetry.TrackEvent(new ConnectEvent(baseSpecified, rootSpecified, openApiSpecified, openApiFound));

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

            // Always show help link after connecting
            shellState.ConsoleManager.WriteLine(Resources.Strings.HelpCommand_Core_Details_Line2.Bold().Cyan());
        }

        private ApiConnection GetConnectionInfo(IShellState shellState, HttpState programState, string rootAddress, string baseAddress, string swaggerAddress, IPreferences preferences, bool isVerbosityEnabled)
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

            // Even if verbosity is not enabled, we still want to be verbose about finding OpenAPI Descriptions
            // if they specified one directly.
            bool logVerboseMessages = isVerbosityEnabled || !string.IsNullOrWhiteSpace(swaggerAddress);

            ApiConnection apiConnection = new ApiConnection(programState, preferences, shellState.ConsoleManager, logVerboseMessages);
            if (!string.IsNullOrWhiteSpace(rootAddress))
            {
                // The `dotnet new webapi` template now has a default start url of `swagger`. Because
                // the default Swashbuckle-generated OpenAPI description doesn't contain a Servers element
                // this will put HttpRepl users into a pit of failure by having a base address of
                // https://localhost:{port}/swagger/, even though the API is, by default, based at the root.
                // Since it is unlikely a user would put their API inside the /swagger path, we will
                // special-case this scenario and remove that from the url. We will give the user an escape
                // hatch via the preference if they do put their API under that path.
                if (rootAddress.EndsWith(WebApiDefaultPathSuffix, StringComparison.OrdinalIgnoreCase))
                {
                    WebApiF5FixEvent fixEvent;
                    if (preferences.GetBoolValue(WellKnownPreference.ConnectCommandSkipRootFix))
                    {
                        fixEvent = new WebApiF5FixEvent(skippedByPreference: true);
                    }
                    else
                    {
                        rootAddress = rootAddress.Substring(0, rootAddress.Length - WebApiDefaultPathSuffix.Length);
                        fixEvent = new WebApiF5FixEvent();
                    }

                    _telemetry.TrackEvent(fixEvent);
                }

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

        private static bool GetOptionExistsFromCommand(DefaultCommandInput<ICoreParseResult> commandInput, string optionId)
        {
            return commandInput.Options[optionId].Count > 0;
        }
    }
}
