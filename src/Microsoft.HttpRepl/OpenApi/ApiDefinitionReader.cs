// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

#nullable enable

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

        public ApiDefinitionParseResult CanHandle(string document)
        {
            if (document is null)
            {
                return ApiDefinitionParseResult.Failed;
            }

            foreach (IApiDefinitionReader reader in _readers)
            {
                ApiDefinitionParseResult result = reader.CanHandle(document);
                if (result.Success)
                {
                    return result;
                }
            }
            return ApiDefinitionParseResult.Failed;
        }

        public ApiDefinitionParseResult Read(string document, Uri? swaggerUri)
        {
            foreach (IApiDefinitionReader reader in _readers)
            {
                ApiDefinitionParseResult parseResult = reader.CanHandle(document);
                if (parseResult.Success)
                {
                    ApiDefinitionParseResult result = reader.ReadDefinition(document, swaggerUri);

                    return result;
                }
            }

            return ApiDefinitionParseResult.Failed;
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
