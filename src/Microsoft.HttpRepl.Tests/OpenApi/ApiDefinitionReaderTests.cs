// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.HttpRepl.Fakes;
using Microsoft.HttpRepl.OpenApi;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.HttpRepl.Tests.OpenApi
{
    public class ApiDefinitionReaderTests
    {
        [Fact]
        public void Read_WithJObjectFormatNotSupportedByAnyExistingReader_ReturnsNull()
        {
            string json = @"{
  ""info"": {
    ""version"": ""v1"",
    ""title"": ""My API""
  }
}";
            JObject jobject = JObject.Parse(json);
            ApiDefinitionReader apiDefinitionReader = new ApiDefinitionReader();

            ApiDefinition definition = apiDefinitionReader.Read(jobject, null);

            Assert.Null(definition);
        }

        [Fact]
        public void RegisterReader_AddNewReader_VerifyReadReturnsApiDefinitionWithStructure()
        {
            string json = @"{
  ""fakeApi"": ""1.0.0"",
  ""info"": {
    ""version"": ""v1""
  }
}";
            JObject jobject = JObject.Parse(json);
            ApiDefinition apiDefinition = new ApiDefinition() { DirectoryStructure = new DirectoryStructure(null) };
            ApiDefinitionReaderStub apiDefinitionReaderStub = new ApiDefinitionReaderStub(apiDefinition);

            ApiDefinitionReader reader = new ApiDefinitionReader();
            reader.RegisterReader(apiDefinitionReaderStub);

            ApiDefinition result = reader.Read(jobject, null);

            Assert.Same(apiDefinition, result);
        }
    }
}
