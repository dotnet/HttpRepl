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

        private async Task FindSwaggerDoc(HttpClient client, IEnumerable<string> swaggerSearchPaths, VerbosityLogger logger, CancellationToken cancellationToken)
        {
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
                        var result = await GetSwaggerDocAsync(client, swaggerUri, logger, cancellationToken);
                        if (result.Success)
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

        private async Task<(bool Success, string Document)> GetSwaggerDocAsync(HttpClient client, Uri uri, VerbosityLogger logger, CancellationToken cancellationToken)
        {
            try
            {
                logger.WriteVerbose(Resources.Strings.ApiConnection_Logging_Checking, uri);
                var response = await client.GetAsync(uri, cancellationToken).ConfigureAwait(false);
                if (cancellationToken.IsCancellationRequested)
                {
                    logger.WriteLineVerbose(Resources.Strings.ApiConnection_Logging_Cancelled);
                    return (false, null);
                }

                if (response.IsSuccessStatusCode)
                {
                    logger.WriteLineVerbose(Resources.Strings.ApiConnection_Logging_Found);
                    string responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    logger.WriteVerbose(Resources.Strings.ApiConnection_Logging_Parsing);
                    ApiDefinitionReader reader = new ApiDefinitionReader();
                    if (reader.CanHandle(responseString))
                    {
                        logger.WriteLineVerbose(Resources.Strings.ApiConnection_Logging_Successful);
                        return (true, responseString);
                    }
                    else
                    {
                        logger.WriteLineVerbose(Resources.Strings.ApiConnection_Logging_Failed);
                        return (false, null);
                    }
                }
                else
                {
                    logger.WriteLineVerbose(response.StatusCode.ToString());
                    return (false, null);
                }
            }
            catch (Exception e)
            {
                logger.WriteLineVerbose(e.Message);
                return (false, null);
            }
            finally
            {
                logger.WriteLineVerbose();
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

        public async Task SetupHttpState(HttpState httpState, bool performAutoDetect, VerbosityLogger logger, CancellationToken cancellationToken)
        {
            if (HasSwaggerUri)
            {
                var result = await GetSwaggerDocAsync(httpState.Client, SwaggerUri, logger, cancellationToken);
                if (result.Success)
                {
                    SwaggerDocument = result.Document;
                }
            }
            else if (performAutoDetect)
            {
                await FindSwaggerDoc(httpState.Client, GetSwaggerSearchPaths(), logger, cancellationToken);
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
