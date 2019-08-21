// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.OpenApi;
using Microsoft.HttpRepl.Preferences;
using Microsoft.Repl;
using Microsoft.Repl.Commanding;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Parsing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.HttpRepl.Commands
{
    public class ConnectCommand : CommandWithStructuredInputBase<HttpState, ICoreParseResult>
    {
        private const string BaseAddressOption = nameof(BaseAddressOption);
        private const string SwaggerAddressOption = nameof(SwaggerAddressOption);
        private const string SwaggerSearchPaths = "swagger.json|swagger/v1/swagger.json|/swagger.json|/swagger/v1/swagger.json";
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
            string rootAddress = commandInput.Arguments.SingleOrDefault()?.Text?.EnsureTrailingSlash();
            string baseAddress = GetBaseAddressFromCommand(commandInput)?.EnsureTrailingSlash();
            string swaggerAddress = GetSwaggerAddressFromCommand(commandInput);

            ConnectionInfo connectionInfo = GetConnectionInfo(shellState, programState, rootAddress, baseAddress, swaggerAddress);

            if (connectionInfo is null)
            {
                return;
            }

            if (connectionInfo.HasRootUri && !connectionInfo.HasBaseUri)
            {
                connectionInfo.BaseUri = connectionInfo.RootUri;
            }

            if (connectionInfo.HasSwaggerUri)
            {
                var result = await TryGetSwaggerDocAsync(programState.Client, connectionInfo.SwaggerUri, cancellationToken);
                if (result.Success)
                {
                    connectionInfo.SwaggerDocument = result.Document;
                }
            }
            else
            {
                await FindSwaggerDoc(programState.Client, connectionInfo, GetSwaggerSearchPaths(), cancellationToken);
            }

            if (connectionInfo.HasSwaggerDocument)
            {
                SetupApiDefinition(connectionInfo, programState);
            }

            // If there's a base address in the api definition and there was no explicit base address, set the
            // base address to the first one in the api definition
            if (programState.ApiDefinition?.BaseAddresses?.Any() == true && connectionInfo.AllowBaseOverrideBySwagger)
            {
                programState.BaseAddress = programState.ApiDefinition.BaseAddresses[0].Url;
            }
            else if (connectionInfo.HasBaseUri)
            {
                programState.BaseAddress = connectionInfo.BaseUri;
            }

            WriteStatus(shellState, programState);
        }

        private IEnumerable<string> GetSwaggerSearchPaths()
        {
            string rawValue = _preferences.GetValue(WellKnownPreference.SwaggerSearchPaths, SwaggerSearchPaths);

            string[] paths = rawValue?.Split('|', StringSplitOptions.RemoveEmptyEntries);

            return paths;
        }

        private static void SetupApiDefinition(ConnectionInfo connectionInfo, HttpState programState)
        {
            ApiDefinitionReader reader = new ApiDefinitionReader();
            programState.ApiDefinition = reader.Read(connectionInfo.SwaggerDocument, connectionInfo.SwaggerUri);
            if (!(programState.ApiDefinition is null))
            {
                programState.SwaggerEndpoint = connectionInfo.SwaggerUri;
            }
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

        private static ConnectionInfo GetConnectionInfo(IShellState shellState, HttpState programState, string rootAddress, string baseAddress, string swaggerAddress)
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

            ConnectionInfo args = new ConnectionInfo();
            if (!string.IsNullOrWhiteSpace(rootAddress))
            {
                args.RootUri = new Uri(rootAddress, UriKind.Absolute);
            }

            if (!SetupBaseAddress(shellState, baseAddress, args) || !SetupSwaggerAddress(shellState, swaggerAddress, args))
            {
                return null;
            }

            args.AllowBaseOverrideBySwagger = !args.HasBaseUri;

            return args;
        }

        private static bool SetupSwaggerAddress(IShellState shellState, string swaggerAddress, ConnectionInfo connectionInfo)
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

        private static bool SetupBaseAddress(IShellState shellState, string baseAddress, ConnectionInfo connectionInfo)
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
                InputElement inputElement = inputElements.FirstOrDefault();
                return inputElement?.Text;
            }

            return null;
        }

        private static async Task FindSwaggerDoc(HttpClient client, ConnectionInfo connectionInfo, IEnumerable<string> swaggerSearchPaths, CancellationToken cancellationToken)
        {
            ApiDefinitionReader reader = new ApiDefinitionReader();
            HashSet<Uri> checkedUris = new HashSet<Uri>();
            List<Uri> baseUrisToCheck = new List<Uri>();
            if (connectionInfo.HasRootUri)
            {
                baseUrisToCheck.Add(connectionInfo.RootUri);
            }
            if (connectionInfo.HasBaseUri)
            {
                baseUrisToCheck.Add(connectionInfo.BaseUri);
            }

            foreach (Uri baseUriToCheck in baseUrisToCheck)
            { 
                foreach (string swaggerSearchPath in swaggerSearchPaths)
                {
                    if (Uri.TryCreate(baseUriToCheck, swaggerSearchPath, out Uri swaggerUri) && !checkedUris.Contains(swaggerUri))
                    {
                        var result = await TryGetSwaggerDocAsync(client, swaggerUri, cancellationToken);
                        if (result.Success && reader.CanHandle(result.Document))
                        {
                            connectionInfo.SwaggerUri = swaggerUri;
                            connectionInfo.SwaggerDocument = result.Document;
                            return;
                        }
                        checkedUris.Add(swaggerUri);
                    }
                }
            }
        }

        private static async Task<JObject> GetSwaggerDocAsync(HttpClient client, Uri uri, CancellationToken cancellationToken)
        {
            var resp = await client.GetAsync(uri, cancellationToken).ConfigureAwait(false);
            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            resp.EnsureSuccessStatusCode();
            string responseString = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
            JsonSerializer serializer = new JsonSerializer { PreserveReferencesHandling = PreserveReferencesHandling.All };
            JObject responseObject = (JObject)serializer.Deserialize(new StringReader(responseString), typeof(JObject));
            ApiDefinitionReader reader = new ApiDefinitionReader();
            responseObject = await PointerUtil.ResolvePointersAsync(uri, responseObject, client).ConfigureAwait(false) as JObject;

            return responseObject;
        }

        private static async Task<(bool Success, JObject Document)> TryGetSwaggerDocAsync(HttpClient client, Uri uri, CancellationToken cancellationToken)
        {
            try
            {
                JObject document = await GetSwaggerDocAsync(client, uri, cancellationToken);
                return (true, document);
            }
            catch
            {
                return (false, null);
            }
        }

        private class ConnectionInfo
        {
            public Uri RootUri { get; set; }
            public bool HasRootUri => !(RootUri is null);
            public Uri BaseUri { get; set; }
            public bool HasBaseUri => !(BaseUri is null);
            public Uri SwaggerUri { get; set; }
            public bool HasSwaggerUri => !(SwaggerUri is null);
            public JObject SwaggerDocument { get; set; }
            public bool HasSwaggerDocument => !(SwaggerDocument is null);
            public bool AllowBaseOverrideBySwagger { get; set; }
        }
    }
}
