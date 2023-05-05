// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.HttpRepl.OpenApi;
using Xunit;

namespace Microsoft.HttpRepl.Tests.OpenApi
{
    public class OpenApiDotNetApiDefinitionReaderTests
    {
        [Theory]
        [MemberData(nameof(GetYamlResourcePaths), MemberType = typeof(OpenApiDotNetApiDefinitionReaderTests))]
        [MemberData(nameof(GetJsonResourcePaths), MemberType = typeof(OpenApiDotNetApiDefinitionReaderTests))]
        public async Task CanHandle_RealOpenApiDescriptions_ReturnsNotNull(string resourcePath)
        {
            // Arrange
            string content = await GetResourceContent(resourcePath);
            OpenApiDotNetApiDefinitionReader apiDefinitionReader = new();

            // Act
            ApiDefinitionParseResult actual = apiDefinitionReader.CanHandle(content);

            // Assert
            Assert.True(actual.Success);
        }

        [Theory]
        [MemberData(nameof(GetYamlTestSetups), MemberType = typeof(OpenApiDotNetApiDefinitionReaderTests))]
        [MemberData(nameof(GetJsonTestSetups), MemberType = typeof(OpenApiDotNetApiDefinitionReaderTests))]
        public async Task ReadDefinition_RealOpenApiDescriptions_ReturnsExpectedDocument(string resourcePath, ApiDefinition expected)
        {
            // Arrange
            string content = await GetResourceContent(resourcePath);
            OpenApiDotNetApiDefinitionReader apiDefinitionReader = new();

            // Act
            ApiDefinitionParseResult actual = apiDefinitionReader.ReadDefinition(content, null);

            // Assert
            AssertDefinition(expected, actual.ApiDefinition);
        }

        [Fact]
        public void CanHandle_WithMissingInfoAndPaths_ReturnsValidationMessages()
        {
            // Arrange
            string json = @"
{
  ""openapi"": ""3.0.0""
}
";
            OpenApiDotNetApiDefinitionReader apiDefinitionReader = new();

            // Act
            ApiDefinitionParseResult result = apiDefinitionReader.CanHandle(json);

            // Assert
            Assert.True(result.Success);
            Assert.NotEqual(0, result.ValidationMessages.Count);
        }

        private void AssertDefinition(ApiDefinition expected, ApiDefinition actual)
        {
            // Core properties
            Assert.NotNull(actual);
            Assert.NotNull(actual.BaseAddresses);
            Assert.NotNull(actual.DirectoryStructure);

            // Base Addresses/Servers
            IEnumerator<ApiDefinition.Server> expectedBaseAddressEnumerator = expected.BaseAddresses.GetEnumerator();
            IEnumerator<ApiDefinition.Server> actualBaseAddressEnumerator = actual.BaseAddresses.GetEnumerator();

            int index = 0;
            while (expectedBaseAddressEnumerator.MoveNext())
            {
                Assert.True(actualBaseAddressEnumerator.MoveNext(), $"Missing BaseAddress: {expectedBaseAddressEnumerator.Current}");

                Assert.True(expectedBaseAddressEnumerator.Current.Url == actualBaseAddressEnumerator.Current.Url, $"BaseAddress[{index}].Url does not match.");
                Assert.True(string.Equals(expectedBaseAddressEnumerator.Current.Description, actualBaseAddressEnumerator.Current.Description, System.StringComparison.InvariantCulture), $"BaseAddress[{index}].Description does not match.");

                index++;
            }
            Assert.False(actualBaseAddressEnumerator.MoveNext(), $"Extra BaseAddress: {actualBaseAddressEnumerator.Current}");

            // Directory Structure
            AssertDirectoryStructure(expected.DirectoryStructure, actual.DirectoryStructure, "/");
        }

        private void AssertDirectoryStructure(IDirectoryStructure expected, IDirectoryStructure actual, string path)
        {
            // Request Info/Methods
            if (expected.RequestInfo is null)
            {
                Assert.True(actual.RequestInfo is null, $"RequestInfo should be null on {path}");
            }
            else
            {
                AssertRequestInfo(expected.RequestInfo, actual.RequestInfo, path);
            }

            // Subdirectories
            IEnumerator<string> expectedDirectoriesEnumerator = expected.DirectoryNames.GetEnumerator();
            IEnumerator<string> actualDirectoriesEnumerator = actual.DirectoryNames.GetEnumerator();

            int directoryIndex = 0;
            while (expectedDirectoriesEnumerator.MoveNext())
            {
                Assert.True(actualDirectoriesEnumerator.MoveNext(), $"Missing subdirectory in {path}: {expectedDirectoriesEnumerator.Current}");
                Assert.True(string.Equals(expectedDirectoriesEnumerator.Current, actualDirectoriesEnumerator.Current, System.StringComparison.Ordinal), $"Expected Directory \"{expectedDirectoriesEnumerator.Current}\" does not match Actual Directory \"{actualDirectoriesEnumerator.Current}\" at index {directoryIndex} in {path}.");

                AssertDirectoryStructure(expected.TraverseTo(expectedDirectoriesEnumerator.Current), actual.TraverseTo(actualDirectoriesEnumerator.Current), $"{path}{expectedDirectoriesEnumerator.Current}/");

                directoryIndex++;
            }

            // Something changed in recent .NET SDKs such that one of these is happening:
            // 1) The message is being calculated eagerly when it was not before (thus calculating it even when MoveNext returns false)
            // 2) Current now throws if there is no current, whereas before it did not
            // #2 is more likely, but I can't prove it in the sources. We'll break out the Assert.False and do the if check
            // and message construction separately to avoid it. 
            //Assert.False(actualDirectoriesEnumerator.MoveNext(), $"Extra subdirectory in {path}: {actualDirectoriesEnumerator.Current}");

            if (actualDirectoriesEnumerator.MoveNext())
            {
                Assert.Fail($"Extra subdirectory in {path}: {actualDirectoriesEnumerator.Current}");
            }

        }

        private void AssertRequestInfo(IRequestInfo expected, IRequestInfo actual, string path)
        {
            // Core object
            Assert.True(actual is not null, $"RequestInfo should NOT be null on {path}");

            // Methods
            IEnumerator<string> expectedMethodsEnumerator = expected.Methods.GetEnumerator();
            IEnumerator<string> actualMethodsEnumerator = actual.Methods.GetEnumerator();

            int methodIndex = 0;
            while (expectedMethodsEnumerator.MoveNext())
            {
                Assert.True(actualMethodsEnumerator.MoveNext(), $"Missing method in {path}: {expectedMethodsEnumerator.Current}");

                Assert.True(string.Equals(expectedMethodsEnumerator.Current, actualMethodsEnumerator.Current, System.StringComparison.Ordinal), $"Expected Method \"{expectedMethodsEnumerator.Current}\" does not match Actual Method \"{actualMethodsEnumerator.Current}\" at index {methodIndex} in {path}.");

                methodIndex++;
            }
            Assert.False(actualMethodsEnumerator.MoveNext(), $"Extra method in {path}: {actualMethodsEnumerator.Current}");
        }


        private async Task<string> GetResourceContent(string filePath)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourcePath = assembly.GetName().Name + ".Resources." + filePath.Replace('\\', '.');
            using (Stream resourceStream = assembly.GetManifestResourceStream(resourcePath))
            using (StreamReader reader = new(resourceStream))
            {
                return await reader.ReadToEndAsync();
            }
        }

        public static IEnumerable<object[]> GetJsonTestSetups()
        {
            return GetTestSetups(".json");
        }

        public static IEnumerable<object[]> GetYamlTestSetups()
        {
            return GetTestSetups(".yml");
        }

        public static IEnumerable<object[]> GetJsonResourcePaths()
        {
            return GetResourcePaths(".json");
        }

        public static IEnumerable<object[]> GetYamlResourcePaths()
        {
            return GetResourcePaths(".yml");
        }

        private static IEnumerable<string[]> GetResourcePaths(string extension)
        {
            foreach (string resourcePath in ResourcePaths)
            {
                yield return new[] { resourcePath + extension };
            }
        }

        private static string[] ResourcePaths = new[]
        {
            "OpenApiDescriptions\\MicrosoftGraph.PowershellSdk.Subscriptions",
            "OpenApiDescriptions\\MicrosoftGraph.PowershellSdk.Analytics",
            "OpenApiDescriptions\\MicrosoftGraph.PowershellSdk.CloudCommunications",
            "OpenApiDescriptions\\xkcd",
        };

        private static IEnumerable<object[]> GetTestSetups(string extension)
        {
            int x = 0;
            yield return new object[]
            {
                ResourcePaths[x++] + extension,
                ApiDefinitionBuilder.Start()
                                    .AddBaseAddress("https://graph.microsoft.com/v1.0/", "Core")
                                    .AddDirectory("subscriptions")
                                        .WithGet()
                                        .WithPost()
                                        .AddDirectory("{subscription-id}")
                                            .WithGet()
                                            .WithPatch()
                                            .WithDelete()
                                            .Finalize()
                                        .Finalize()
                                    .Build()
            };

            yield return new object[]
            {
                ResourcePaths[x++] + extension,
                ApiDefinitionBuilder.Start()
                                    .AddBaseAddress("https://graph.microsoft.com/v1.0/", "Core")
                                    .AddDirectory("users")
                                    .AddDirectory("{user-id}")
                                        .AddDirectory("insights")
                                            .WithGet()
                                            .WithPatch()
                                            .AddDirectory("shared")
                                                .WithGet()
                                                .WithPost()
                                                .AddDirectory("{sharedInsight-id}")
                                                    .WithGet()
                                                    .WithPatch()
                                                    .AddDirectory("lastSharedMethod")
                                                        .WithGet()
                                                        .Finalize()
                                                    .AddDirectory("resource")
                                                        .WithGet()
                                                        .Finalize()
                                                    .Finalize()
                                                .Finalize()
                                            .AddDirectory("trending")
                                                .WithGet()
                                                .WithPost()
                                                .AddDirectory("{trending-id}")
                                                    .WithGet()
                                                    .WithPatch()
                                                    .AddDirectory("resource")
                                                        .WithGet()
                                                        .Finalize()
                                                    .Finalize()
                                                .Finalize()
                                            .AddDirectory("used")
                                                .WithGet()
                                                .WithPost()
                                                .AddDirectory("{usedInsight-id}")
                                                    .WithGet()
                                                    .WithPatch()
                                                    .AddDirectory("resource")
                                                        .WithGet()
                                                        .Finalize()
                                                    .Finalize()
                                                .Finalize()
                                            .Finalize()
                                        .Finalize()
                                    .Finalize()
                                    .Build()
            };
            yield return new object[]
            {
                ResourcePaths[x++] + extension,
                ApiDefinitionBuilder.Start()
                                    .AddBaseAddress("https://graph.microsoft.com/v1.0/", "Core")
                                    .AddDirectory("communications")
                                        .WithGet()
                                        .WithPatch()
                                        .AddDirectory("calls")
                                            .WithGet()
                                            .WithPost()
                                            .AddDirectory("{call-id}")
                                                .WithGet()
                                                .WithPatch()
                                                .AddDirectory("microsoft.graph.answer")
                                                    .WithPost()
                                                    .Finalize()
                                                .AddDirectory("microsoft.graph.changeScreenSharingRole")
                                                    .WithPost()
                                                    .Finalize()
                                                .AddDirectory("microsoft.graph.keepAlive")
                                                    .WithPost()
                                                    .Finalize()
                                                .AddDirectory("microsoft.graph.mute")
                                                    .WithPost()
                                                    .Finalize()
                                                .AddDirectory("microsoft.graph.playPrompt")
                                                    .WithPost()
                                                    .Finalize()
                                                .AddDirectory("microsoft.graph.recordResponse")
                                                    .WithPost()
                                                    .Finalize()
                                                .AddDirectory("microsoft.graph.redirect")
                                                    .WithPost()
                                                    .Finalize()
                                                .AddDirectory("microsoft.graph.reject")
                                                    .WithPost()
                                                    .Finalize()
                                                .AddDirectory("microsoft.graph.subscribeToTone")
                                                    .WithPost()
                                                    .Finalize()
                                                .AddDirectory("microsoft.graph.transfer")
                                                    .WithPost()
                                                    .Finalize()
                                                .AddDirectory("microsoft.graph.unmute")
                                                    .WithPost()
                                                    .Finalize()
                                                .AddDirectory("microsoft.graph.updateRecordingStatus")
                                                    .WithPost()
                                                    .Finalize()
                                                .AddDirectory("operations")
                                                    .WithGet()
                                                    .WithPost()
                                                    .AddDirectory("{commsOperation-id}")
                                                        .WithGet()
                                                        .WithPatch()
                                                        .Finalize()
                                                    .Finalize()
                                                .AddDirectory("participants")
                                                    .WithGet()
                                                    .WithPost()
                                                    .AddDirectory("{participant-id}")
                                                        .WithGet()
                                                        .WithPatch()
                                                        .AddDirectory("microsoft.graph.mute")
                                                            .WithPost()
                                                            .Finalize()
                                                        .Finalize()
                                                    .AddDirectory("microsoft.graph.invite")
                                                        .WithPost()
                                                        .Finalize()
                                                    .Finalize()
                                                .Finalize()
                                            .AddDirectory("microsoft.graph.logTeleconferenceDeviceQuality")
                                                .WithPost()
                                                .Finalize()
                                            .Finalize()
                                        .AddDirectory("onlineMeetings")
                                            .WithGet()
                                            .WithPost()
                                            .AddDirectory("{onlineMeeting-id}")
                                                .WithGet()
                                                .WithPatch()
                                                .Finalize()
                                            .Finalize()
                                        .Finalize()
                                    .Build()
            };
            yield return new object[]
            {
                ResourcePaths[x++] + extension,
                ApiDefinitionBuilder.Start()
                                    .AddBaseAddress("http://xkcd.com", null)
                                    .AddDirectory("info.0.json")
                                        .WithGet()
                                        .Finalize()
                                    .AddDirectory("{comicId}")
                                        .AddDirectory("info.0.json")
                                            .WithGet()
                                            .Finalize()
                                        .Finalize()
                                    .Build()
            };
        }
    }
}
