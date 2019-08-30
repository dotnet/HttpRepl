// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.HttpRepl.OpenApi;
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
            Schema schema = new Schema();
            schema.Example = new { StringProperty = stringValue, IntProperty = intValue };

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
            Schema schema = new Schema();
            schema.Default = new { StringProperty = stringValue, IntProperty = intValue };

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
            Schema schema = new Schema();
            schema.Example = new { StringProperty = stringValue, IntProperty = intValue };
            schema.Default = new { ADifferentStringProperty = "a different string value", ADifferentIntProperty = 7 };

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
            Schema schema = new Schema();
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
            Schema schema = new Schema();
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
    }
}
