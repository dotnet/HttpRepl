// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
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
            string[] parts = entry.Path.Split('/');

            foreach (string part in parts)
            {
                if (!string.IsNullOrEmpty(part))
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
                        dirRequestInfo.SetFallbackRequestBody(method, parameterSetsByContentType.Key, GetBodyString(null, parameterSetsByContentType.Value));
                    }

                    dirRequestInfo.SetRequestBody(method, parameterSetsByContentType.Key, GetBodyString(parameterSetsByContentType.Key, parameterSetsByContentType.Value));
                }

                dirRequestInfo.AddMethod(method);
            }

            if (dirRequestInfo.Methods.Count > 0)
            {
                parent.RequestInfo = dirRequestInfo;
            }
        }

        private static string GetBodyString(string contentType, IEnumerable<Parameter> operation)
        {
            Parameter body = operation.FirstOrDefault(x => string.Equals(x.Location, "body", StringComparison.OrdinalIgnoreCase));

            if (body != null)
            {
                JToken result = GenerateData(body.Schema);
                return result?.ToString() ?? "{\n}";
            }

            return null;
        }

        private static JToken GenerateData(Schema schema)
        {
            if (schema == null)
            {
                return null;
            }

            if (schema.Example != null)
            {
                return JToken.FromObject(schema.Example);
            }

            if (schema.Default != null)
            {
                return JToken.FromObject(schema.Default);
            }

            if (schema.Type is null)
            {
                if (schema.Properties != null || schema.AdditionalProperties != null || schema.MinProperties.HasValue || schema.MaxProperties.HasValue)
                {
                    schema.Type = "OBJECT";
                }
                else if (schema.Items != null || schema.MinItems.HasValue || schema.MaxItems.HasValue)
                {
                    schema.Type = "ARRAY";
                }
                else if (schema.Minimum.HasValue || schema.Maximum.HasValue || schema.MultipleOf.HasValue)
                {
                    schema.Type = "INTEGER";
                }
            }

            switch (schema.Type?.ToUpperInvariant())
            {
                case null:
                case "STRING":
                    return "";
                case "NUMBER":
                    if (schema.Minimum.HasValue)
                    {
                        if (schema.Maximum.HasValue)
                        {
                            return (schema.Maximum.Value + schema.Minimum.Value) / 2;
                        }

                        if (schema.ExclusiveMinimum)
                        {
                            return schema.Minimum.Value + 1;
                        }

                        return schema.Minimum.Value;
                    }
                    return 1.1;
                case "INTEGER":
                    if (schema.Minimum.HasValue)
                    {
                        if (schema.Maximum.HasValue)
                        {
                            return (int)((schema.Maximum.Value + schema.Minimum.Value) / 2);
                        }

                        if (schema.ExclusiveMinimum)
                        {
                            return schema.Minimum.Value + 1;
                        }

                        return schema.Minimum.Value;
                    }
                    return 0;
                case "BOOLEAN":
                    return true;
                case "ARRAY":
                    JArray container = new JArray();
                    JToken item = GenerateData(schema.Items) ?? "";

                    int count = schema.MinItems.GetValueOrDefault();
                    count = Math.Max(1, count);

                    for (int i = 0; i < count; ++i)
                    {
                        container.Add(item.DeepClone());
                    }

                    return container;
                case "OBJECT":
                    if (schema.Properties != null)
                    {
                        JObject obj = new JObject();
                        foreach (KeyValuePair<string, Schema> property in schema.Properties)
                        {
                            JToken data = GenerateData(property.Value) ?? "";
                            obj[property.Key] = data;
                        }
                        return obj;
                    }
                    return null;
            }

            return null;
        }
    }
}
