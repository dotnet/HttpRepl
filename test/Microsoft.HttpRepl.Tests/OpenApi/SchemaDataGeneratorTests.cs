// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.HttpRepl.OpenApi;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.HttpRepl.Tests.OpenApi
{
    public class SchemaDataGeneratorTests
    {
        [Fact]
        public void GenerateData_WithNull_ReturnsNull()
        {
            JToken result = SchemaDataGenerator.GenerateData(null);

            Assert.Null(result);
        }

        [Fact]
        public void GenerateData_WithExample_ReturnsJTokenBasedOnExample()
        {
            string stringValue = "string value";
            int intValue = 5;
            OpenApiSchema schema = new OpenApiSchema();
            schema.Example = new StringAndIntClass() { StringProperty = stringValue, IntProperty = intValue,};

            JToken result = SchemaDataGenerator.GenerateData(schema);

            Assert.NotNull(result);
            Assert.NotNull(result["StringProperty"]);
            Assert.Equal(stringValue, result["StringProperty"].Value<string>());
            Assert.NotNull(result["IntProperty"]);
            Assert.Equal(intValue, result["IntProperty"].Value<int>());
        }

        [Fact]
        public void GenerateData_WithDefault_ReturnsJTokenBasedOnDefault()
        {
            string stringValue = "string value";
            int intValue = 5;
            OpenApiSchema schema = new OpenApiSchema();
            schema.Default = new StringAndIntClass() { StringProperty = stringValue, IntProperty = intValue };

            JToken result = SchemaDataGenerator.GenerateData(schema);

            Assert.NotNull(result);
            Assert.NotNull(result["StringProperty"]);
            Assert.Equal(stringValue, result["StringProperty"].Value<string>());
            Assert.NotNull(result["IntProperty"]);
            Assert.Equal(intValue, result["IntProperty"].Value<int>());
        }

        [Fact]
        public void GenerateData_WithExampleAndDefault_ReturnsJTokenBasedOnExample()
        {
            string stringValue = "string value";
            int intValue = 5;
            OpenApiSchema schema = new OpenApiSchema();
            schema.Example = new StringAndIntClass() { StringProperty = stringValue, IntProperty = intValue };
            schema.Default = new StringAndIntClass() { StringProperty = "a different string value", IntProperty = 7 };

            JToken result = SchemaDataGenerator.GenerateData(schema);

            Assert.NotNull(result);
            Assert.NotNull(result["StringProperty"]);
            Assert.Equal(stringValue, result["StringProperty"].Value<string>());
            Assert.NotNull(result["IntProperty"]);
            Assert.Equal(intValue, result["IntProperty"].Value<int>());
        }

        [Fact]
        public void GenerateData_WithStringNoFormat_ReturnsEmptyString()
        {
            OpenApiSchema schema = new OpenApiSchema();
            schema.Type = "string";
            schema.Format = null;

            JToken result = SchemaDataGenerator.GenerateData(schema);

            Assert.NotNull(result);
            Assert.True(result is JValue);
            JValue jValue = (JValue)result;
            Assert.Equal(string.Empty, jValue.Value);
        }

        [Fact]
        public void GenerateData_WithStringDateTimeFormat_ReturnsDateTimeString()
        {
            OpenApiSchema schema = new OpenApiSchema();
            schema.Type = "string";
            schema.Format = "date-time";
            // Format we are looking for is ISO8601 - YYYY-MM-DDTHH:MM:SS.MMMMMMM-HH:MM
            string iso8601RegEx = "^([0-9]{4})-(1[0-2]|0[1-9])-(3[01]|0[1-9]|[12][0-9])T(2[0-3]|[01][0-9]):([0-5][0-9]):([0-5][0-9])(\\.[0-9]+)(Z|([+-](2[0-3]|[01][0-9]):([0-5][0-9])))$";

            JToken result = SchemaDataGenerator.GenerateData(schema);

            Assert.NotNull(result);
            Assert.True(result is JValue);
            JValue jValue = (JValue)result;
            string stringValue = jValue.Value<string>();
            Assert.Matches(iso8601RegEx, stringValue);
        }

        [Fact]
        public void GenerateData_WithObjectWithReadOnlyProperty_DoesNotIncludeReadOnlyProperty()
        {
            OpenApiSchema rootSchema = new OpenApiSchema()
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>()
            };
            OpenApiSchema readOnlySchema = new OpenApiSchema()
            {
                Type = "integer",
                ReadOnly = true
            };
            OpenApiSchema writeableSchema = new OpenApiSchema()
            {
                Type = "string"
            };
            rootSchema.Properties.Add("Writeable", writeableSchema);
            rootSchema.Properties.Add("ReadOnly", readOnlySchema);

            JToken result = SchemaDataGenerator.GenerateData(rootSchema);

            Assert.NotNull(result);
            Assert.True(result is JObject);
            JObject jObject = (JObject)result;
            IEnumerable<string> propertyNames = jObject.Properties().Select(j => j.Name);
            Assert.Contains("Writeable", propertyNames, StringComparer.Ordinal);
            Assert.DoesNotContain("ReadOnly", propertyNames, StringComparer.Ordinal);
        }

        private class StringAndIntClass : IOpenApiAny
        {
            public AnyType AnyType => AnyType.Object;
            public string StringProperty { get; set; }
            public int IntProperty { get; set; }

            public void Write(IOpenApiWriter writer, OpenApiSpecVersion specVersion)
            {
                // Nothing to do here
            }
        }
    }
}
