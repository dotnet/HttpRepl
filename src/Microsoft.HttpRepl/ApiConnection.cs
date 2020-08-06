// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.OpenApi;
using Microsoft.HttpRepl.Preferences;

namespace Microsoft.HttpRepl
{
    internal class ApiConnection
    {
        // OpenAPI description search paths are appended to the base url to
        // attempt to find the description document. A search path is a
        // relative url that is appended to the base url using Uri.TryCreate,
        // so the semantics of relative urls matter here.
        // Example: Base path https://localhost/v1/ and search path openapi.json
        //          will result in https://localhost/v1/openapi.json being tested.
        // Example: Base path https://localhost/v1/ and search path /openapi.json
        //          will result in https://localhost/openapi.json being tested.
        private static readonly string[] OpenApiDescriptionSearchPaths = new[] {
            "swagger.json",
            "/swagger.json",
            "swagger/v1/swagger.json",
            "/swagger/v1/swagger.json",
            "openapi.json",
            "/openapi.json",
        };

        private readonly IPreferences _preferences;

        public Uri RootUri { get; set; }
        public bool HasRootUri => RootUri is object;
        public Uri BaseUri { get; set; }
        public bool HasBaseUri => BaseUri is object;
        public Uri SwaggerUri { get; set; }
        public bool HasSwaggerUri => SwaggerUri is object;
        public string SwaggerDocument { get; set; }
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

        public async Task<string> GetSwaggerDocAsync(HttpClient client, Uri uri, CancellationToken cancellationToken)
        {
            var resp = await client.GetAsync(uri, cancellationToken).ConfigureAwait(false);
            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            resp.EnsureSuccessStatusCode();
            string responseString = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);

            return responseString;
        }

        public async Task<(bool Success, string Document)> TryGetSwaggerDocAsync(HttpClient client, Uri uri, CancellationToken cancellationToken)
        {
            try
            {
                string document = await GetSwaggerDocAsync(client, uri, cancellationToken);
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
            string rawValue = _preferences.GetValue(WellKnownPreference.SwaggerSearchPaths);

            if (rawValue is null)
            {
                return OpenApiDescriptionSearchPaths;
            }
            else
            {
                string[] paths = rawValue?.Split('|', StringSplitOptions.RemoveEmptyEntries);
                return paths;
            }
        }
    }
}
