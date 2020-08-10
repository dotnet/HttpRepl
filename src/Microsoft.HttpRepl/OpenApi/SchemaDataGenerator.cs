// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;

namespace Microsoft.HttpRepl.OpenApi
{
    internal static class SchemaDataGenerator
    {
        // TODO: Make this a setting?
        private const int MaxExampleDataDepth = 3;

        public static string GetBodyString(OpenApiSchema schema)
        {
            if (schema is not null)
            {
                JToken result = GenerateData(schema);
                return result?.ToString() ?? "{\n}";
            }

            return null;
        }

        internal static JToken GenerateData(OpenApiSchema schema, int depth = 0)
        {
            if (schema is null)
            {
                return null;
            }

            if (schema.Example is not null)
            {
                return JToken.FromObject(schema.Example);
            }

            if (schema.Default is not null)
            {
                return JToken.FromObject(schema.Default);
            }

            if (schema.Type is null)
            {
                if (schema.Properties is not null || schema.AdditionalProperties is not null || schema.MinProperties.HasValue || schema.MaxProperties.HasValue)
                {
                    schema.Type = "OBJECT";
                }
                else if (schema.Items is not null || schema.MinItems.HasValue || schema.MaxItems.HasValue)
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
                    if (string.Equals(schema.Format, "date-time", StringComparison.OrdinalIgnoreCase))
                    {
                        return DateTimeOffset.Now.ToString("o");
                    }
                    return "";
                case "NUMBER":
                    if (schema.Minimum.HasValue)
                    {
                        if (schema.Maximum.HasValue)
                        {
                            return (schema.Maximum.Value + schema.Minimum.Value) / 2;
                        }

                        if (schema.ExclusiveMinimum is true)
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

                        if (schema.ExclusiveMinimum is true)
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

                    if (depth < MaxExampleDataDepth)
                    {
                        JToken item = GenerateData(schema.Items, depth + 1) ?? "";

                        int count = schema.MinItems.GetValueOrDefault();
                        count = Math.Max(1, count);

                        for (int i = 0; i < count; ++i)
                        {
                            container.Add(item.DeepClone());
                        }
                    }

                    return container;
                case "OBJECT":
                    if (schema.Properties is not null)
                    {
                        JObject obj = new JObject();
                        if (depth < MaxExampleDataDepth)
                        {
                            foreach (KeyValuePair<string, OpenApiSchema> property in schema.Properties)
                            {
                                if (property.Value.ReadOnly)
                                {
                                    continue;
                                }

                                JToken data = GenerateData(property.Value, depth + 1) ?? "";
                                obj[property.Key] = data;
                            }
                        }
                        return obj;
                    }
                    return null;
            }

            return null;
        }
    }
}
