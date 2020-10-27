// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.OpenApi;
using Microsoft.HttpRepl.Preferences;
using Microsoft.Repl.ConsoleHandling;

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
        private readonly IWritable _logger;
        private readonly bool _logVerboseMessages;

        public Uri? RootUri { get; set; }
        public bool HasRootUri => RootUri is object;
        public Uri? BaseUri { get; set; }
        public bool HasBaseUri => BaseUri is object;
        public Uri? SwaggerUri { get; set; }
        public bool HasSwaggerUri => SwaggerUri is object;
        public string? SwaggerDocument { get; set; }
        public bool HasSwaggerDocument => SwaggerDocument is object;
        public bool AllowBaseOverrideBySwagger { get; set; }

        public ApiConnection(IPreferences preferences, IWritable logger, bool logVerboseMessages)
        {
            _preferences = preferences ?? throw new ArgumentNullException(nameof(preferences));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logVerboseMessages = logVerboseMessages;
        }

        private async Task FindSwaggerDoc(HttpClient client, IEnumerable<string> swaggerSearchPaths, CancellationToken cancellationToken)
        {
            HashSet<Uri> checkedUris = new HashSet<Uri>();
            List<Uri> baseUrisToCheck = new List<Uri>();
            if (HasRootUri)
            {
                baseUrisToCheck.Add(RootUri!);
            }
            if (HasBaseUri)
            {
                baseUrisToCheck.Add(BaseUri!);
            }

            foreach (Uri baseUriToCheck in baseUrisToCheck)
            {
                foreach (string swaggerSearchPath in swaggerSearchPaths)
                {
                    if (Uri.TryCreate(baseUriToCheck, swaggerSearchPath, out Uri? swaggerUri) && !checkedUris.Contains(swaggerUri))
                    {
                        var document = await GetSwaggerDocAsync(client, swaggerUri, cancellationToken);
                        if (document is not null)
                        {
                            SwaggerUri = swaggerUri;
                            SwaggerDocument = document;
                            return;
                        }
                        checkedUris.Add(swaggerUri);
                    }
                }
            }
        }

        private async Task<string?> GetSwaggerDocAsync(HttpClient client, Uri uri, CancellationToken cancellationToken)
        {
            try
            {
                WriteVerbose(string.Format(Resources.Strings.ApiConnection_Logging_Checking, uri));
                var response = await client.GetAsync(uri, cancellationToken).ConfigureAwait(false);
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.WriteLine(Resources.Strings.ApiConnection_Logging_Cancelled);
                    return null;
                }

                if (response.IsSuccessStatusCode)
                {
                    WriteLineVerbose(Resources.Strings.ApiConnection_Logging_Found);
                    string responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    WriteVerbose(Resources.Strings.ApiConnection_Logging_Parsing);
                    ApiDefinitionReader reader = new ApiDefinitionReader();
                    if (reader.CanHandle(responseString))
                    {
                        WriteLineVerbose(Resources.Strings.ApiConnection_Logging_Successful);
                        return responseString;
                    }
                    else
                    {
                        WriteLineVerbose(Resources.Strings.ApiConnection_Logging_Failed);
                        return null;
                    }
                }
                else
                {
                    WriteLineVerbose(response.StatusCode.ToString());
                    return null;
                }
            }
            catch (Exception e)
            {
                WriteLineVerbose(e.Message);
                return null;
            }
            finally
            {
                WriteLineVerbose();
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
                SwaggerDocument = await GetSwaggerDocAsync(httpState.Client, SwaggerUri!, cancellationToken);
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
                string[] paths = rawValue.Split('|', StringSplitOptions.RemoveEmptyEntries);
                return paths;
            }
        }

        private void WriteVerbose(string s)
        {
            if (_logVerboseMessages)
            {
                _logger.Write(s);
            }
        }

        private void WriteLineVerbose(string s)
        {
            if (_logVerboseMessages)
            {
                _logger.WriteLine(s);
            }
        }

        private void WriteLineVerbose()
        {
            if (_logVerboseMessages)
            {
                _logger.WriteLine();
            }
        }
    }
}
