// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.HttpRepl.FileSystem;
using Microsoft.HttpRepl.Preferences;
using Microsoft.HttpRepl.Tests.TestDoubles;
using Microsoft.HttpRepl.UserProfile;
using Xunit;

namespace Microsoft.HttpRepl.Tests
{
    public class HttpStateTests
    {
        [Fact]
        public void GetPathString_EmptyPathSections_Slash()
        {
            string expected = "/";
            HttpState state = SetupHttpState();
            state.PathSections.Clear();

            string result = state.GetRelativePathString();

            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetPathString_SinglePathSection_CorrectString()
        {
            string expected = "/FirstDirectory";
            HttpState state = SetupHttpState();
            state.PathSections.Push("FirstDirectory");

            string result = state.GetRelativePathString();

            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetPathString_MultiplePathSections_CorrectString()
        {
            string expected = "/FirstDirectory/SecondDirectory";
            HttpState state = SetupHttpState();
            state.PathSections.Push("FirstDirectory");
            state.PathSections.Push("SecondDirectory");

            string result = state.GetRelativePathString();

            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetApplicableContentTypes_NoStructure_ReturnsNull()
        {
            HttpState httpState = SetupHttpState();

            httpState.BaseAddress = new Uri("https://localhost/");
            httpState.Structure = null;

            IEnumerable<string> result = httpState.GetApplicableContentTypes(null, string.Empty);

            Assert.Null(result);
        }

        [Fact]
        public void GetApplicableContentTypes_NoMethod_ReturnsAll()
        {
            HttpState httpState = SetupHttpState();

            DirectoryStructure directoryStructure = new DirectoryStructure(null);
            RequestInfo requestInfo = new RequestInfo();
            requestInfo.SetRequestBody("GET", "application/json", "");
            requestInfo.SetRequestBody("PUT", "application/xml", "");
            directoryStructure.RequestInfo = requestInfo;

            httpState.BaseAddress = new Uri("https://localhost/");
            httpState.Structure = directoryStructure;

            IEnumerable<string> result = httpState.GetApplicableContentTypes(null, "");

            Assert.NotNull(result);

            List<string> resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.Contains("application/json", resultList, StringComparer.OrdinalIgnoreCase);
            Assert.Contains("application/xml", resultList, StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public void GetApplicableContentTypes_GetMethod_ReturnsCorrectOne()
        {
            HttpState httpState = SetupHttpState();

            DirectoryStructure directoryStructure = new DirectoryStructure(null);
            RequestInfo requestInfo = new RequestInfo();
            requestInfo.SetRequestBody("GET", "application/json", "");
            requestInfo.SetRequestBody("PUT", "application/xml", "");
            directoryStructure.RequestInfo = requestInfo;

            httpState.BaseAddress = new Uri("https://localhost/");
            httpState.Structure = directoryStructure;

            IEnumerable<string> result = httpState.GetApplicableContentTypes("GET", "");

            Assert.Single(result);
            Assert.Contains("application/json", result, StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public void GetApplicableContentTypes_WithPath_ReturnsCorrectOne()
        {
            HttpState httpState = SetupHttpState();

            DirectoryStructure parentDirectoryStructure = new DirectoryStructure(null);
            RequestInfo parentRequestInfo = new RequestInfo();
            parentRequestInfo.SetRequestBody("GET", "application/json", "");
            parentDirectoryStructure.RequestInfo = parentRequestInfo;
            DirectoryStructure childDirectoryStructure = parentDirectoryStructure.DeclareDirectory("child");
            RequestInfo childRequestInfo = new RequestInfo();
            childRequestInfo.SetRequestBody("GET", "application/xml", "");
            childDirectoryStructure.RequestInfo = childRequestInfo;

            httpState.BaseAddress = new Uri("https://localhost/");
            httpState.Structure = parentDirectoryStructure;

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
        public void GetEffectivePathForPrompt_NullBaseAddress_ReturnsNull()
        {
            HttpState httpState = SetupHttpState();

            httpState.BaseAddress = null;

            Uri result = httpState.GetEffectivePathForPrompt();

            Assert.Null(result);
        }

        private static HttpState SetupHttpState()
        {
            IFileSystem fileSystem = new FileSystemStub();
            IUserProfileDirectoryProvider userProfileDirectoryProvider = new UserProfileDirectoryProvider();
            IPreferences preferences = new UserFolderPreferences(fileSystem, userProfileDirectoryProvider, null);
            HttpClient httpClient = new HttpClient();

            return new HttpState(fileSystem, preferences, httpClient);
        }
    }
}
