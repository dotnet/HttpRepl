// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Microsoft.HttpRepl.OpenApi
{
    public static class SchemaDataGenerator
    {
        public static string GetBodyString(IEnumerable<Parameter> operation)
        {
            Parameter body = operation.FirstOrDefault(x => string.Equals(x.Location, "body", StringComparison.OrdinalIgnoreCase));

            if (body != null)
            {
                JToken result = GenerateData(body.Schema);
                return result?.ToString() ?? "{\n}";
            }

            return null;
        }

        internal static JToken GenerateData(Schema schema)
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
                            if (property.Value.ReadOnly)
                            {
                                continue;
                            }

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
