// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.HttpRepl.Fakes;
using Microsoft.HttpRepl.FileSystem;
using Microsoft.HttpRepl.OpenApi;
using Microsoft.HttpRepl.Preferences;
using Microsoft.HttpRepl.UserProfile;
using Xunit;

namespace Microsoft.HttpRepl.Tests
{
    public class HttpStateTests
    {
        [Fact]
        public void GetRelativePathString_EmptyPathSections_Slash()
        {
            string expected = "/";
            HttpState state = SetupHttpState();
            state.PathSections.Clear();

            string result = state.GetRelativePathString();

            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetRelativePathString_SinglePathSection_CorrectString()
        {
            string expected = "/FirstDirectory";
            HttpState state = SetupHttpState();
            state.PathSections.Push("FirstDirectory");

            string result = state.GetRelativePathString();

            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetRelativePathString_MultiplePathSections_CorrectString()
        {
            string expected = "/FirstDirectory/SecondDirectory";
            HttpState state = SetupHttpState();
            state.PathSections.Push("FirstDirectory");
            state.PathSections.Push("SecondDirectory");

            string result = state.GetRelativePathString();

            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetApplicableContentTypes_NoBaseAddress_ReturnsNull()
        {
            HttpState httpState = SetupHttpState();
            httpState.BaseAddress = null;

            IEnumerable<string> result = httpState.GetApplicableContentTypes(null, string.Empty);

            Assert.Null(result);
        }

        [Fact]
        public void GetApplicableContentTypes_NoStructure_ReturnsNull()
        {
            HttpState httpState = SetupHttpState();

            httpState.BaseAddress = new Uri("https://localhost/");
            httpState.ApiDefinition = null;

            IEnumerable<string> result = httpState.GetApplicableContentTypes(null, string.Empty);

            Assert.Null(result);
        }

        [Fact]
        public void GetApplicableContentTypes_NoMethod_ReturnsAll()
        {
            DirectoryStructure directoryStructure = new DirectoryStructure(null);
            RequestInfo requestInfo = new RequestInfo();
            requestInfo.SetRequestBody("GET", "application/json", "");
            requestInfo.SetRequestBody("PUT", "application/xml", "");
            directoryStructure.RequestInfo = requestInfo;

            HttpState httpState = SetupHttpState();
            httpState.BaseAddress = new Uri("https://localhost/");
            ApiDefinition apiDefinition = new ApiDefinition();
            apiDefinition.DirectoryStructure = directoryStructure;
            httpState.ApiDefinition = apiDefinition;

            IEnumerable<string> result = httpState.GetApplicableContentTypes(null, "");

            Assert.NotNull(result);

            Assert.Equal(2, result.Count());
            Assert.Contains("application/json", result, StringComparer.OrdinalIgnoreCase);
            Assert.Contains("application/xml", result, StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public void GetApplicableContentTypes_GetMethod_ReturnsCorrectOne()
        {
            DirectoryStructure directoryStructure = new DirectoryStructure(null);
            RequestInfo requestInfo = new RequestInfo();
            requestInfo.SetRequestBody("GET", "application/json", "");
            requestInfo.SetRequestBody("PUT", "application/xml", "");
            directoryStructure.RequestInfo = requestInfo;

            HttpState httpState = SetupHttpState();
            httpState.BaseAddress = new Uri("https://localhost/");
            ApiDefinition apiDefinition = new ApiDefinition();
            apiDefinition.DirectoryStructure = directoryStructure;
            httpState.ApiDefinition = apiDefinition;

            IEnumerable<string> result = httpState.GetApplicableContentTypes("GET", "");

            Assert.Single(result);
            Assert.Contains("application/json", result, StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public void GetApplicableContentTypes_WithPath_ReturnsCorrectOne()
        {
            DirectoryStructure parentDirectoryStructure = new DirectoryStructure(null);
            RequestInfo parentRequestInfo = new RequestInfo();
            parentRequestInfo.SetRequestBody("GET", "application/json", "");
            parentDirectoryStructure.RequestInfo = parentRequestInfo;
            DirectoryStructure childDirectoryStructure = parentDirectoryStructure.DeclareDirectory("child");
            RequestInfo childRequestInfo = new RequestInfo();
            childRequestInfo.SetRequestBody("GET", "application/xml", "");
            childDirectoryStructure.RequestInfo = childRequestInfo;

            HttpState httpState = SetupHttpState();
            httpState.BaseAddress = new Uri("https://localhost/");
            ApiDefinition apiDefinition = new ApiDefinition();
            apiDefinition.DirectoryStructure = parentDirectoryStructure;
            httpState.ApiDefinition = apiDefinition;

            IEnumerable<string> result = httpState.GetApplicableContentTypes("GET", "child");

            Assert.Single(result);
            Assert.Contains("application/xml", result, StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public void GetEffectivePath_NoBaseAddressOrAbsoluteUri_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => HttpState.GetEffectivePath(null, "", "/NotAnAbsoluteUri"));
        }

        [Theory]
        [InlineData("https://github.com/", "", "https://localhost/pets", "https://localhost/pets")]
        [InlineData("https://localhost/", "dir1", "dir2", "https://localhost/dir1/dir2")]
        [InlineData("https://localhost/", "dir1?q=5&r=6", "dir2?s=7", "https://localhost/dir1/dir2?q=5&r=6&s=7")]
        [InlineData("https://petstore.swagger.io/v2/", "pet", "", "https://petstore.swagger.io/v2/pet")]
        [InlineData("https://petstore.swagger.io/v2/", "/pet", "", "https://petstore.swagger.io/pet")]
        [InlineData("https://petstore.swagger.io/v2/", "", "pet", "https://petstore.swagger.io/v2/pet")]
        [InlineData("https://petstore.swagger.io/v2/", "", "/pet", "https://petstore.swagger.io/pet")]
        [InlineData("https://petstore.swagger.io/v2/", "/pet", "/buy", "https://petstore.swagger.io/buy")]
        [InlineData("https://petstore.swagger.io/", "pet", "", "https://petstore.swagger.io/pet")]
        [InlineData("https://petstore.swagger.io/", "/pet", "", "https://petstore.swagger.io/pet")]
        [InlineData("https://petstore.swagger.io/", "", "pet", "https://petstore.swagger.io/pet")]
        [InlineData("https://petstore.swagger.io/", "", "/pet", "https://petstore.swagger.io/pet")]
        public void GetEffectivePath_ProperConcatenation(string baseUriString, string pathSections, string specifiedPath, string expectedResult)
        {
            Uri baseUri = new Uri(baseUriString);

            Uri result = HttpState.GetEffectivePath(baseUri, pathSections, specifiedPath);

            Assert.Equal(expectedResult, result.ToString(), StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public void GetEffectivePath_NullBaseAddressAndNoPath_Throws()
        {
            HttpState httpState = SetupHttpState();
            httpState.BaseAddress = null;

            Assert.Throws<ArgumentNullException>("baseAddress", () => httpState.GetEffectivePath(""));
        }

        [Fact]
        public void GetEffectivePathForPrompt_NullBaseAddress_ReturnsNull()
        {
            HttpState httpState = SetupHttpState();

            httpState.BaseAddress = null;

            Uri result = httpState.GetEffectivePathForPrompt();

            Assert.Null(result);
        }

        [Fact]
        public void HeaderSetup_WithDefaultUserAgent_UsesHttpRepl()
        {
            HttpState httpState = SetupHttpState(preferencesFileContent: "");

            Assert.Equal("HTTP-REPL", httpState.Headers["User-Agent"].Single(), StringComparer.Ordinal);
        }

        [Fact]
        public void HeaderSetup_WithCustomUserAgent_UsesCustom()
        {
            string differentUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3875.0 Safari/537.36 Edg/78.0.245.0";

            HttpState httpState = SetupHttpState(preferencesFileContent: $"{WellKnownPreference.HttpClientUserAgent}={differentUserAgent}");

            Assert.Equal(differentUserAgent, httpState.Headers["User-Agent"].Single(), StringComparer.Ordinal);
        }

        private static HttpState SetupHttpState(string preferencesFileContent = null)
        {
            UserProfileDirectoryProvider userProfileDirectoryProvider = new UserProfileDirectoryProvider();
            IFileSystem fileSystem;
            if (preferencesFileContent != null)
            {
                fileSystem = new MockedFileSystem();
            }
            else
            {
                fileSystem = new FileSystemStub();
            }
            UserFolderPreferences preferences = new UserFolderPreferences(fileSystem, userProfileDirectoryProvider, null);
            if (preferencesFileContent != null)
            {
                ((MockedFileSystem)fileSystem).AddFile(preferences.PreferencesFilePath, preferencesFileContent);
            }
            
            HttpClient client = new HttpClient();
            HttpState state = new HttpState(fileSystem, preferences, client);

            return state;
        }
    }
}
