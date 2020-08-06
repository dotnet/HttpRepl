// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.HttpRepl.OpenApi
{
    internal class ApiDefinitionReader
    {
        private readonly List<IApiDefinitionReader> _readers = new List<IApiDefinitionReader>
        {
            new OpenApiDotNetApiDefinitionReader(),
        };

        internal void RegisterReader(IApiDefinitionReader reader)
        {
            _readers.Add(reader);
        }

        public bool CanHandle(string document)
        {
            if (document is null)
            {
                return false;
            }

            foreach (IApiDefinitionReader reader in _readers)
            {
                if (reader.CanHandle(document))
                {
                    return true;
                }
            }
            return false;
        }

        public ApiDefinition Read(string document, Uri swaggerUri)
        {
            foreach (IApiDefinitionReader reader in _readers)
            {
                if (reader.CanHandle(document))
                {
                    ApiDefinition result = reader.ReadDefinition(document, swaggerUri);

                    return result;
                }
            }

            return null;
        }

        public static void FillDirectoryInfo(DirectoryStructure parent, EndpointMetadata entry)
        {
            entry = entry ?? throw new ArgumentNullException(nameof(entry));

            parent = parent ?? throw new ArgumentNullException(nameof(parent));

            string[] parts = entry.Path.Split('/');

            foreach (string part in parts)
            {
                if (!string.IsNullOrEmpty(part) && parent is object)
                {
                    parent = parent.DeclareDirectory(part);
                }
            }

            RequestInfo dirRequestInfo = new RequestInfo();

            foreach (RequestMetadata requestMetadata in entry.AvailableRequests)
            {
                string method = requestMetadata.Operation.ToString();

                foreach (RequestContentMetadata content in requestMetadata.Content)
                {
                    if (string.IsNullOrWhiteSpace(content.ContentType))
                    {
                        dirRequestInfo.SetFallbackRequestBody(method, content.ContentType, SchemaDataGenerator.GetBodyString(content.BodySchema));
                    }

                    dirRequestInfo.SetRequestBody(method, content.ContentType, SchemaDataGenerator.GetBodyString(content.BodySchema));
                }

                dirRequestInfo.AddMethod(requestMetadata.Operation.ToString());
            }

            if (dirRequestInfo.Methods.Count > 0 && parent is object)
            {
                parent.RequestInfo = dirRequestInfo;
            }
        }
    }
}
