// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace Microsoft.HttpRepl.OpenApi
{
    internal class OpenApiDotNetApiDefinitionReader : IApiDefinitionReader
    {
        public bool CanHandle(string document)
        {
            OpenApiStringReader reader = new();

            try
            {
                OpenApiDocument openApiDocument = reader.Read(document, out _);

                return openApiDocument is not null;
            }
            catch
            {
                return false;
            }
        }

        public ApiDefinition ReadDefinition(string document, Uri sourceUri)
        {
            OpenApiStringReader reader = new();

            OpenApiDocument openApiDocument = reader.Read(document, out _);

            ApiDefinition apiDefinition = new ApiDefinition();

            ReadServers(apiDefinition, sourceUri, openApiDocument);

            IList<EndpointMetadata> metadata = ReadPaths(openApiDocument);

            apiDefinition.DirectoryStructure = BuildDirectoryStructure(metadata);

            return apiDefinition;
        }

        private static void ReadServers(ApiDefinition apiDefinition, Uri sourceUri, OpenApiDocument openApiDocument)
        {
            foreach (OpenApiServer server in openApiDocument.Servers)
            {
                string url = server.Url?.EnsureTrailingSlash();
                string description = server.Description;

                if (url is null)
                {
                    continue;
                }

                if (Uri.IsWellFormedUriString(url, UriKind.Absolute) && Uri.TryCreate(url, UriKind.Absolute, out Uri absoluteServerUri))
                {
                    apiDefinition.BaseAddresses.Add(new ApiDefinition.Server() { Url = absoluteServerUri, Description = description });
                }
                else if (Uri.TryCreate(sourceUri, url, out Uri relativeServerUri))
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
                            OpenApiSchema schema = content.Value?.Schema;

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
