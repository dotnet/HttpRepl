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
using Microsoft.HttpRepl.Preferences;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.HttpRepl
{
    internal class ApiConnection
    {
        private readonly IPreferences _preferences;
        private const string SwaggerSearchPaths = "swagger.json|swagger/v1/swagger.json|/swagger.json|/swagger/v1/swagger.json";

        public Uri RootUri { get; set; }
        public bool HasRootUri => RootUri is object;
        public Uri BaseUri { get; set; }
        public bool HasBaseUri => BaseUri is object;
        public Uri SwaggerUri { get; set; }
        public bool HasSwaggerUri => SwaggerUri is object;
        public JObject SwaggerDocument { get; set; }
        public bool HasSwaggerDocument => SwaggerDocument is object;
        public bool AllowBaseOverrideBySwagger { get; set; }

        public ApiConnection(IPreferences preferences)
        {
            _preferences = preferences;
        }

        public async Task FindSwaggerDoc(HttpClient client, IEnumerable<string> swaggerSearchPaths, CancellationToken cancellationToken)
        {
            ApiDefinitionReader reader = new ApiDefinitionReader();
            HashSet<Uri> checkedUris = new HashSet<Uri>();
            List<Uri> baseUrisToCheck = new List<Uri>();
            if (HasRootUri)
            {
                baseUrisToCheck.Add(RootUri);
            }
            if (HasBaseUri)
            {
                baseUrisToCheck.Add(BaseUri);
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
                            SwaggerUri = swaggerUri;
                            SwaggerDocument = result.Document;
                            return;
                        }
                        checkedUris.Add(swaggerUri);
                    }
                }
            }
        }

        public async Task<JObject> GetSwaggerDocAsync(HttpClient client, Uri uri, CancellationToken cancellationToken)
        {
            var resp = await client.GetAsync(uri, cancellationToken).ConfigureAwait(false);
            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            resp.EnsureSuccessStatusCode();
            string responseString = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
            JsonSerializer serializer = new JsonSerializer { PreserveReferencesHandling = PreserveReferencesHandling.All };
            using (StringReader stringReader = new StringReader(responseString))
            {
                JObject responseObject = (JObject)serializer.Deserialize(stringReader, typeof(JObject));
                responseObject = await PointerUtil.ResolvePointersAsync(uri, responseObject, client).ConfigureAwait(false) as JObject;

                return responseObject;
            }
        }

        public async Task<(bool Success, JObject Document)> TryGetSwaggerDocAsync(HttpClient client, Uri uri, CancellationToken cancellationToken)
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

        public void SetupApiDefinition(HttpState programState)
        {
            ApiDefinitionReader reader = new ApiDefinitionReader();
            programState.ApiDefinition = reader.Read(SwaggerDocument, SwaggerUri);
            if (programState.ApiDefinition is object)
            {
                programState.SwaggerEndpoint = SwaggerUri;
            }
        }

        public async Task SetupHttpState(HttpState httpState, bool performAutoDetect, CancellationToken cancellationToken)
        {
            if (HasSwaggerUri)
            {
                var result = await TryGetSwaggerDocAsync(httpState.Client, SwaggerUri, cancellationToken);
                if (result.Success)
                {
                    SwaggerDocument = result.Document;
                }
            }
            else if (performAutoDetect)
            {
                await FindSwaggerDoc(httpState.Client, GetSwaggerSearchPaths(), cancellationToken);
            }

            if (HasSwaggerDocument)
            {
                SetupApiDefinition(httpState);
            }

            // If there's a base address in the api definition and there was no explicit base address, set the
            // base address to the first one in the api definition
            if (httpState.ApiDefinition?.BaseAddresses?.Any() == true && AllowBaseOverrideBySwagger)
            {
                httpState.BaseAddress = httpState.ApiDefinition.BaseAddresses[0].Url;
            }
            else if (HasBaseUri)
            {
                httpState.BaseAddress = BaseUri;
            }
        }

        private IEnumerable<string> GetSwaggerSearchPaths()
        {
            string rawValue = _preferences.GetValue(WellKnownPreference.SwaggerSearchPaths, SwaggerSearchPaths);

            string[] paths = rawValue?.Split('|', StringSplitOptions.RemoveEmptyEntries);

            return paths;
        }
    }
}
