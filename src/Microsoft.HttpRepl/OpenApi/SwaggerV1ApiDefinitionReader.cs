// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Microsoft.HttpRepl.OpenApi
{
    public class SwaggerV1ApiDefinitionReader : IApiDefinitionReader
    {
        public bool CanHandle(JObject document)
        {
            document = document ?? throw new ArgumentNullException(nameof(document));

            return (document["swaggerVersion"]?.ToString() ?? "").StartsWith("1.", StringComparison.Ordinal);
        }

        public ApiDefinition ReadDefinition(JObject document, Uri sourceUri)
        {
            document = document ?? throw new ArgumentNullException(nameof(document));

            ApiDefinition apiDefinition = new ApiDefinition();
            List<EndpointMetadata> metadata = new List<EndpointMetadata>();

            string basePath = document["basePath"]?.Value<string>()?.EnsureTrailingSlash();

            if (!string.IsNullOrWhiteSpace(basePath))
            {
                if (Uri.TryCreate(basePath, UriKind.Absolute, out Uri serverUri))
                {
                    sourceUri = sourceUri ?? throw new ArgumentNullException(nameof(sourceUri));

                    apiDefinition.BaseAddresses.Add(new ApiDefinition.Server() { Url = serverUri, Description = $"Swagger v1 basePath from {sourceUri.ToString()}" });
                }
            }

            if (!(document["consumes"] is JArray globalConsumes))
            {
                globalConsumes = new JArray();
            }

            if (document["apis"] is JArray jArray)
            {
                foreach (JObject obj in jArray)
                {
                    string path = obj["path"]?.ToString();

                    if (path is null)
                    {
                        continue;
                    }

                    Dictionary<string, IReadOnlyDictionary<string, IReadOnlyList<Parameter>>> requestMethods = new Dictionary<string, IReadOnlyDictionary<string, IReadOnlyList<Parameter>>>(StringComparer.Ordinal);

                    if (obj["operations"] is JArray operations)
                    {
                        foreach (JObject operationObject in operations.OfType<JObject>())
                        {
                            string method = operationObject["method"]?.ToString();
                            List<Parameter> parameters = new List<Parameter>();

                            if (operationObject["parameters"] is JArray parametersArray)
                            {
                                foreach (JObject parameterObj in parametersArray.OfType<JObject>())
                                {
                                    Parameter p = parameterObj.ToObject<Parameter>();
                                    p.Location = parameterObj["paramType"]?.ToString();
                                    p.IsRequired = parameterObj["required"]?.ToObject<bool>() ?? false;

                                    string type = parameterObj["type"]?.ToString();

                                    if (type is null)
                                    {
                                        continue;
                                    }

                                    switch (type.ToUpperInvariant())
                                    {
                                        case "INTEGER":
                                        case "NUMBER":
                                        case "STRING":
                                        case "BOOLEAN":
                                            p.Schema = new Schema { Type = type };
                                            break;
                                        case "FILE":
                                            break;
                                        default:
                                            if (document["models"]?[type] is JObject schemaObject)
                                            {
                                                //TODO: Handle subtypes (https://github.com/OAI/OpenAPI-Specification/blob/master/versions/1.2.md#527-model-object)
                                                p.Schema = schemaObject.ToObject<Schema>();
                                            }
                                            break;
                                    }

                                    parameters.Add(p);
                                }
                            }

                            if (!(operationObject["consumes"] is JArray consumes))
                            {
                                consumes = globalConsumes;
                            }

                            Dictionary<string, IReadOnlyList<Parameter>> parametersByContentType = new Dictionary<string, IReadOnlyList<Parameter>>(StringComparer.OrdinalIgnoreCase)
                            {
                                { "", parameters }
                            };

                            foreach (JValue value in consumes.OfType<JValue>().Where(x => x.Type == JTokenType.String))
                            {
                                parametersByContentType[value.ToString()] = parameters;
                            }

                            if (method != null)
                            {
                                requestMethods[method] = parametersByContentType;
                            }
                        }
                    }

                    metadata.Add(new EndpointMetadata(path, requestMethods));
                }
            }

            DirectoryStructure d = new DirectoryStructure(null);

            foreach (EndpointMetadata entry in metadata)
            {
                ApiDefinitionReader.FillDirectoryInfo(d, entry);
            }

            apiDefinition.DirectoryStructure = d;

            return apiDefinition;
        }
    }
}
