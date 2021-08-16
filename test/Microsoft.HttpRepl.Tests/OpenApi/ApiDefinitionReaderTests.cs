// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System;
using Microsoft.HttpRepl.Fakes;
using Microsoft.HttpRepl.OpenApi;
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

            ApiDefinitionReader apiDefinitionReader = new ApiDefinitionReader();

            ApiDefinitionParseResult result = apiDefinitionReader.Read(json, null);

            Assert.False(result.Success);
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

            ApiDefinition apiDefinition = new ApiDefinition() { DirectoryStructure = new DirectoryStructure(null) };
            ApiDefinitionReaderStub apiDefinitionReaderStub = new ApiDefinitionReaderStub(apiDefinition);

            ApiDefinitionReader reader = new ApiDefinitionReader();
            reader.RegisterReader(apiDefinitionReaderStub);

            ApiDefinitionParseResult result = reader.Read(json, null);

            Assert.Same(apiDefinition, result.ApiDefinition);
        }
    }
}
