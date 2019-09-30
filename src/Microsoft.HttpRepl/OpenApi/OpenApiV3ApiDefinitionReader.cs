// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Microsoft.HttpRepl.OpenApi
{
    public class OpenApiV3ApiDefinitionReader : IApiDefinitionReader
    {
        private static readonly HashSet<string> _ValidOperationNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "get", "put", "post", "delete", "options", "head", "patch", "trace" };
        public bool CanHandle(JObject document)
        {
            document = document ?? throw new ArgumentNullException(nameof(document));

            return (document["openapi"]?.ToString() ?? "").StartsWith("3.", StringComparison.Ordinal);
        }

        // Based on latest spec at https://github.com/OAI/OpenAPI-Specification/blob/master/versions/3.0.2.md
        public ApiDefinition ReadDefinition(JObject document, Uri sourceUri)
        {
            document = document ?? throw new ArgumentNullException(nameof(document));

            ApiDefinition apiDefinition = new ApiDefinition();
            List<EndpointMetadata> metadata = new List<EndpointMetadata>();

            if (document["servers"] is JArray serverArray)
            {
                foreach (JObject server in serverArray)
                {
                    string url = server["url"]?.Value<string>()?.EnsureTrailingSlash();
                    string description = server["description"]?.Value<string>();

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

            if (document["paths"] is JObject paths)
            {
                foreach (JProperty path in paths.Properties())
                {
                    if (!(path.Value is JObject pathBody))
                    {
                        continue;
                    }

                    Dictionary<string, IReadOnlyDictionary<string, IReadOnlyList<Parameter>>> requestMethods = new Dictionary<string, IReadOnlyDictionary<string, IReadOnlyList<Parameter>>>(StringComparer.OrdinalIgnoreCase);

                    foreach (JProperty method in pathBody.Properties())
                    {
                        // A path item has a fixed set of field names and we are only looking for the set of
                        // field names that are for operation objects
                        if (!_ValidOperationNames.Contains(method.Name))
                        {
                            continue;
                        }

                        requestMethods[method.Name] = new Dictionary<string, IReadOnlyList<Parameter>>(StringComparer.OrdinalIgnoreCase);

                        List<Parameter> parameters = new List<Parameter>();

                        if (method.Value is JObject methodBody)
                        {
                            if (methodBody["parameters"] is JArray parametersArray)
                            {
                                foreach (JObject parameterObj in parametersArray.OfType<JObject>())
                                {
                                    Parameter p = parameterObj.ToObject<Parameter>();
                                    p.Location = parameterObj["in"]?.ToString();

                                    if (!(parameterObj["schema"] is JObject schemaObject))
                                    {
                                        schemaObject = null;
                                    }

                                    p.Schema = schemaObject?.ToObject<Schema>() ?? parameterObj.ToObject<Schema>();
                                    parameters.Add(p);
                                }
                            }

                            if (methodBody["requestBody"] is JObject bodyObject)
                            {
                                if (!(bodyObject["content"] is JObject contentTypeLookup))
                                {
                                    continue;
                                }

                                foreach (JProperty contentTypeEntry in contentTypeLookup.Properties())
                                {
                                    List<Parameter> parametersByContentType = new List<Parameter>(parameters);
                                    Parameter p = new Parameter();
                                    p.Location = "body";
                                    p.IsRequired = bodyObject["required"]?.ToObject<bool>() ?? false;

                                    if (!(contentTypeEntry.Value is JObject contentTypeObject) || !(contentTypeObject["schema"] is JObject schemaObject))
                                    {
                                        schemaObject = null;
                                    }

                                    p.Schema = schemaObject?.ToObject<Schema>() ?? bodyObject.ToObject<Schema>();
                                    parametersByContentType.Add(p);

                                    Dictionary<string, IReadOnlyList<Parameter>> bucketByMethod;
                                    if (!requestMethods.TryGetValue(method.Name, out IReadOnlyDictionary<string, IReadOnlyList<Parameter>> bucketByMethodRaw))
                                    {
                                        requestMethods[method.Name] = bucketByMethodRaw = new Dictionary<string, IReadOnlyList<Parameter>>(StringComparer.OrdinalIgnoreCase)
                                        {
                                            { "", parametersByContentType }
                                        };
                                    }

                                    bucketByMethod = (Dictionary<string, IReadOnlyList<Parameter>>)bucketByMethodRaw;
                                    bucketByMethod[contentTypeEntry.Name] = parametersByContentType;
                                }
                            }
                        }
                    }

                    metadata.Add(new EndpointMetadata(path.Name, requestMethods));
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
