// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace Microsoft.HttpRepl.OpenApi
{
    internal class OpenApiDotNetApiDefinitionReader : IApiDefinitionReader
    {
        public ApiDefinitionParseResult CanHandle(string document)
        {
            OpenApiStringReader reader = new();

            try
            {
                OpenApiDocument openApiDocument = reader.Read(document, out OpenApiDiagnostic diagnostic);

                IEnumerable<string> messages = diagnostic.Errors.Select(e => e.ToString());

                return new ApiDefinitionParseResult(true, null, messages);
            }
            catch
            {
                return ApiDefinitionParseResult.Failed;
            }
        }

        public ApiDefinitionParseResult ReadDefinition(string document, Uri? sourceUri)
        {
            OpenApiStringReader reader = new();

            OpenApiDocument openApiDocument = reader.Read(document, out OpenApiDiagnostic diagnostic);

            ApiDefinition apiDefinition = new ApiDefinition();

            ReadServers(apiDefinition, sourceUri, openApiDocument);

            IList<EndpointMetadata> metadata = ReadPaths(openApiDocument);

            apiDefinition.DirectoryStructure = BuildDirectoryStructure(metadata);

            return new ApiDefinitionParseResult(true, apiDefinition, diagnostic.Errors.Select(e => e.ToString()));
        }

        private static void ReadServers(ApiDefinition apiDefinition, Uri? sourceUri, OpenApiDocument openApiDocument)
        {
            foreach (OpenApiServer server in openApiDocument.Servers)
            {
                string? url = server.Url?.EnsureTrailingSlash();
                string description = server.Description;

                if (url is null)
                {
                    continue;
                }

                if (Uri.IsWellFormedUriString(url, UriKind.Absolute) && Uri.TryCreate(url, UriKind.Absolute, out Uri? absoluteServerUri))
                {
                    apiDefinition.BaseAddresses.Add(new ApiDefinition.Server() { Url = absoluteServerUri, Description = description });
                }
                else if (Uri.TryCreate(sourceUri, url, out Uri? relativeServerUri))
                {
                    apiDefinition.BaseAddresses.Add(new ApiDefinition.Server() { Url = relativeServerUri, Description = description });
                }
            }
        }

        private static IList<EndpointMetadata> ReadPaths(OpenApiDocument openApiDocument)
        {
            List<EndpointMetadata> metadata = new List<EndpointMetadata>();

            if (openApiDocument.Paths is not null)
            {
                foreach (KeyValuePair<string, OpenApiPathItem> path in openApiDocument.Paths)
                {
                    string relativeUrl = path.Key;

                    EndpointMetadata endpointMetadata = ReadOperations(relativeUrl, path.Value.Operations);

                    metadata.Add(endpointMetadata);
                }
            }

            return metadata;
        }

        private static EndpointMetadata ReadOperations(string path, IDictionary<OperationType, OpenApiOperation> operations)
        {
            EndpointMetadata endpointMetadata = new EndpointMetadata(path);

            if (operations is not null)
            {
                foreach (KeyValuePair<OperationType, OpenApiOperation> operation in operations)
                {
                    RequestMetadata requestMetadata = new RequestMetadata(operation.Key);

                    foreach (OpenApiParameter parameter in operation.Value.Parameters)
                    {
                        requestMetadata.Parameters.Add(parameter);
                    }

                    if (operation.Value.RequestBody?.Content is not null)
                    {
                        foreach (KeyValuePair<string, OpenApiMediaType> content in operation.Value.RequestBody.Content)
                        {
                            string contentType = content.Key;
                            bool isRequired = operation.Value.RequestBody.Required;
                            OpenApiSchema? schema = content.Value?.Schema;

                            requestMetadata.Content.Add(new RequestContentMetadata(contentType, isRequired, schema));
                        }
                    }

                    endpointMetadata.AvailableRequests.Add(requestMetadata);
                }
            }

            return endpointMetadata;
        }

        private static DirectoryStructure BuildDirectoryStructure(IList<EndpointMetadata> metadata)
        {
            DirectoryStructure d = new DirectoryStructure(null);

            if (metadata is not null)
            {
                for (int index = 0; index < metadata.Count; index++)
                {
                    System.Diagnostics.Debug.WriteLine(index);
                    EndpointMetadata entry = metadata[index];
                    ApiDefinitionReader.FillDirectoryInfo(d, entry);
                }
            }

            return d;
        }
    }
}
