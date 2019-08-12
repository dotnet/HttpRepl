// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.OpenApi;
using Microsoft.HttpRepl.Resources;
using Microsoft.Repl;
using Microsoft.Repl.Commanding;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Parsing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.HttpRepl.Commands
{
    public class SetSwaggerCommand : ICommand<HttpState, ICoreParseResult>
    {
        private static readonly string Name = "set";
        private static readonly string SubCommand = "swagger";

        public string Description => Strings.SetSwaggerCommand_Description;



        

       

        private static async Task<ApiDefinition> GetSwaggerDocAsync(HttpClient client, Uri uri)
        {
            var resp = await client.GetAsync(uri).ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();
            string responseString = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
            JsonSerializer serializer = new JsonSerializer { PreserveReferencesHandling = PreserveReferencesHandling.All };
            JObject responseObject = (JObject)serializer.Deserialize(new StringReader(responseString), typeof(JObject));
            EndpointMetadataReader reader = new EndpointMetadataReader();
            responseObject = await PointerUtil.ResolvePointersAsync(uri, responseObject, client).ConfigureAwait(false) as JObject;

            if (responseObject is null)
            {
                return new ApiDefinition();
            }

            return reader.Read(responseObject, uri);
        }

        public string GetHelpSummary(IShellState shellState, HttpState programState)
        {
            return Description;
        }

        public string GetHelpDetails(IShellState shellState, HttpState programState, ICoreParseResult parseResult)
        {
            if (parseResult.ContainsAtLeast(Name, SubCommand))
            {
                return Description;
            }

            return null;
        }

        public IEnumerable<string> Suggest(IShellState shellState, HttpState programState, ICoreParseResult parseResult)
        {
            if (parseResult.Sections.Count == 0)
            {
                return new[] { Name };
            }

            if (parseResult.Sections.Count > 0 && parseResult.SelectedSection == 0 && Name.StartsWith(parseResult.Sections[0].Substring(0, parseResult.CaretPositionWithinSelectedSection), StringComparison.OrdinalIgnoreCase))
            {
                return new[] { Name };
            }

            if (string.Equals(Name, parseResult.Sections[0], StringComparison.OrdinalIgnoreCase) && parseResult.SelectedSection == 1 && (parseResult.Sections.Count < 2 || SubCommand.StartsWith(parseResult.Sections[1].Substring(0, parseResult.CaretPositionWithinSelectedSection), StringComparison.OrdinalIgnoreCase)))
            {
                return new[] { SubCommand };
            }

            return null;
        }

        public bool? CanHandle(IShellState shellState, HttpState programState, ICoreParseResult parseResult)
        {
            return parseResult.Sections.Count > 1 && string.Equals(parseResult.Sections[0], Name, StringComparison.OrdinalIgnoreCase) && string.Equals(parseResult.Sections[1], SubCommand, StringComparison.OrdinalIgnoreCase)
                ? (bool?)true
                : null;
        }

        public async Task ExecuteAsync(IShellState shellState, HttpState programState, ICoreParseResult parseResult, CancellationToken cancellationToken)
        {
            if (parseResult.Sections.Count == 2)
            {
                programState.ApiDefinition = null;
                return;
            }

            if (parseResult.Sections.Count != 3 || string.IsNullOrEmpty(parseResult.Sections[2]))
            {
                shellState.ConsoleManager.Error.WriteLine(Strings.SetSwaggerCommand_SpecifySwaggerDocument.SetColor(programState.ErrorColor));
            }
            else
            {
                Uri serverUri;
                if ((Uri.IsWellFormedUriString(parseResult.Sections[2], UriKind.Absolute) && Uri.TryCreate(parseResult.Sections[2], UriKind.Absolute, out serverUri)) ||
                    (!(programState.BaseAddress is null) && Uri.TryCreate(programState.BaseAddress, parseResult.Sections[2], out serverUri)))
                {
                    await CreateDirectoryStructureForSwaggerEndpointAsync(shellState, programState, serverUri, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    shellState.ConsoleManager.Error.WriteLine(Strings.SetSwaggerCommand_InvalidSwaggerUri.SetColor(programState.ErrorColor));
                }
            }
        }

        internal static async Task CreateDirectoryStructureForSwaggerEndpointAsync(IShellState shellState, HttpState programState, Uri serverUri, CancellationToken cancellationToken)
        {
            programState.SwaggerEndpoint = serverUri;

            try
            {
                ApiDefinition definition = await GetSwaggerDocAsync(programState.Client, serverUri).ConfigureAwait(false);
                programState.ApiDefinition = !cancellationToken.IsCancellationRequested ? definition : null;

                //IEnumerable<EndpointMetadata> doc = await GetSwaggerDocAsync(programState.Client, serverUri).ConfigureAwait(false);

                //DirectoryStructure d = new DirectoryStructure(null);

                //foreach (EndpointMetadata entry in doc)
                //{
                //    FillDirectoryInfo(d, entry);
                //}

                //ApiDefinition definition = new ApiDefinition();
                //definition.DirectoryStructure = d;

                //programState.ApiDefinition = !cancellationToken.IsCancellationRequested ? definition : null;
            }
            catch
            {
                programState.ApiDefinition = null;
            }
        }
    }
}
