// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Microsoft.HttpRepl.OpenApi
{
    public class ApiDefinitionReader
    {
        private readonly List<IApiDefinitionReader> _readers = new List<IApiDefinitionReader>
        {
            new OpenApiV3ApiDefinitionReader(),
            new SwaggerV2ApiDefinitionReader(),
            new SwaggerV1ApiDefinitionReader()
        };

        internal void RegisterReader(IApiDefinitionReader reader)
        {
            _readers.Add(reader);
        }

        public bool CanHandle(JObject document)
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

        public ApiDefinition Read(JObject document, Uri swaggerUri)
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

            foreach (KeyValuePair<string, IReadOnlyDictionary<string, IReadOnlyList<Parameter>>> requestInfo in entry.AvailableRequests)
            {
                string method = requestInfo.Key;

                foreach (KeyValuePair<string, IReadOnlyList<Parameter>> parameterSetsByContentType in requestInfo.Value)
                {
                    if (string.IsNullOrEmpty(parameterSetsByContentType.Key))
                    {
                        dirRequestInfo.SetFallbackRequestBody(method, parameterSetsByContentType.Key, SchemaDataGenerator.GetBodyString(parameterSetsByContentType.Value));
                    }

                    dirRequestInfo.SetRequestBody(method, parameterSetsByContentType.Key, SchemaDataGenerator.GetBodyString(parameterSetsByContentType.Value));
                }

                dirRequestInfo.AddMethod(method);
            }

            if (dirRequestInfo.Methods.Count > 0 && parent is object)
            {
                parent.RequestInfo = dirRequestInfo;
            }
        }
    }
}
