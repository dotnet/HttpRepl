// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using Microsoft.HttpRepl.Fakes;
using Microsoft.HttpRepl.FileSystem;
using Microsoft.HttpRepl.OpenApi;
using Microsoft.HttpRepl.Preferences;
using Microsoft.HttpRepl.Suggestions;
using Microsoft.HttpRepl.UserProfile;
using Xunit;

namespace Microsoft.HttpRepl.Tests.Suggestions
{
    public class HeaderCompletionTests
    {
        [Fact]
        public void GetCompletions_NullExistingHeaders_ProperResults()
        {
            IEnumerable<string> result = HeaderCompletion.GetCompletions(null, "U");

            Assert.Equal(3, result.Count());
            Assert.Contains("Upgrade", result, StringComparer.OrdinalIgnoreCase);
            Assert.Contains("Upgrade-Insecure-Requests", result, StringComparer.OrdinalIgnoreCase);
            Assert.Contains("User-Agent", result, StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public void GetCompletions_ExistingHeader_ProperResults()
        {
            IReadOnlyCollection<string> existingHeaders = new Collection<string>() { "User-Agent" };

            IEnumerable<string> result = HeaderCompletion.GetCompletions(existingHeaders, "U");

            Assert.Equal(2, result.Count());
            Assert.Contains("Upgrade", result, StringComparer.OrdinalIgnoreCase);
            Assert.Contains("Upgrade-Insecure-Requests", result, StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public void GetCompletions_NotAnExistingHeader_ProperResults()
        {
            IReadOnlyCollection<string> existingHeaders = new Collection<string>() { "Content-Type" };

            IEnumerable<string> result = HeaderCompletion.GetCompletions(existingHeaders, "U");

            Assert.Equal(3, result.Count());
            Assert.Contains("Upgrade", result, StringComparer.OrdinalIgnoreCase);
            Assert.Contains("Upgrade-Insecure-Requests", result, StringComparer.OrdinalIgnoreCase);
            Assert.Contains("User-Agent", result, StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public void GetCompletions_NoResults_ReturnsEmpty()
        {
            IEnumerable<string> result = HeaderCompletion.GetCompletions(null, "Not-A-Real-Header");

            Assert.Empty(result);
        }

        [Theory]
        [InlineData("Accept")]
        [InlineData("Content-Length")]
        [InlineData("X-Requested-With")]
        public void GetValueCompletions_NoHeaderMatch_ReturnsNull(string header)
        {
            HttpState httpState = SetupHttpState();

            IEnumerable<string> result = HeaderCompletion.GetValueCompletions(method: null, path: null, header, prefix: null, httpState);

            Assert.Null(result);
        }

        [Fact]
        public void GetValueCompletions_NoMatch_ReturnsNull()
        {
            HttpState httpState = SetupHttpState();

            IEnumerable<string> result = HeaderCompletion.GetValueCompletions(method: "GET", path: "", header: "Content-Type", "", httpState);

            Assert.Null(result);
        }

        [Fact]
        public void GetValueCompletions_OneMatch_ReturnsMatch()
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

            IEnumerable<string> result = HeaderCompletion.GetValueCompletions(method: "GET", path: "", header: "Content-Type", "", httpState);

            Assert.Single(result);
            Assert.Contains("application/json", result, StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public void GetValueCompletions_MultipleMatches_ReturnsCorrectMatches()
        {
            DirectoryStructure directoryStructure = new DirectoryStructure(null);
            RequestInfo requestInfo = new RequestInfo();
            requestInfo.SetRequestBody("GET", "application/json", "");
            requestInfo.SetRequestBody("GET", "text/plain", "");
            requestInfo.SetRequestBody("PUT", "application/xml", "");
            directoryStructure.RequestInfo = requestInfo;

            HttpState httpState = SetupHttpState();
            httpState.BaseAddress = new Uri("https://localhost/");
            ApiDefinition apiDefinition = new ApiDefinition();
            apiDefinition.DirectoryStructure = directoryStructure;
            httpState.ApiDefinition = apiDefinition;

            IEnumerable<string> result = HeaderCompletion.GetValueCompletions(method: "GET", path: "", header: "Content-Type", "", httpState);

            Assert.Equal(2, result.Count());
            Assert.Contains("application/json", result, StringComparer.OrdinalIgnoreCase);
            Assert.Contains("text/plain", result, StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public void GetValueCompletions_NoMethod_ReturnsAll()
        {
            DirectoryStructure directoryStructure = new DirectoryStructure(null);
            RequestInfo requestInfo = new RequestInfo();
            requestInfo.SetRequestBody("GET", "application/json", "");
            requestInfo.SetRequestBody("GET", "text/plain", "");
            requestInfo.SetRequestBody("PUT", "application/xml", "");
            directoryStructure.RequestInfo = requestInfo;

            HttpState httpState = SetupHttpState();
            httpState.BaseAddress = new Uri("https://localhost/");
            ApiDefinition apiDefinition = new ApiDefinition();
            apiDefinition.DirectoryStructure = directoryStructure;
            httpState.ApiDefinition = apiDefinition;

            IEnumerable<string> result = HeaderCompletion.GetValueCompletions(method: null, path: "", header: "Content-Type", "", httpState);

            Assert.Equal(3, result.Count());
            Assert.Contains("application/json", result, StringComparer.OrdinalIgnoreCase);
            Assert.Contains("text/plain", result, StringComparer.OrdinalIgnoreCase);
            Assert.Contains("application/xml", result, StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public void GetValueCompletions_WithPrefix_ReturnsMatch()
        {
            DirectoryStructure directoryStructure = new DirectoryStructure(null);
            RequestInfo requestInfo = new RequestInfo();
            requestInfo.SetRequestBody("GET", "application/json", "");
            requestInfo.SetRequestBody("GET", "text/plain", "");
            requestInfo.SetRequestBody("PUT", "application/xml", "");
            directoryStructure.RequestInfo = requestInfo;

            HttpState httpState = SetupHttpState();
            httpState.BaseAddress = new Uri("https://localhost/");
            ApiDefinition apiDefinition = new ApiDefinition();
            apiDefinition.DirectoryStructure = directoryStructure;
            httpState.ApiDefinition = apiDefinition;

            IEnumerable<string> result = HeaderCompletion.GetValueCompletions(method: "GET", path: "", header: "Content-Type", "a", httpState);

            Assert.Single(result);
            Assert.Contains("application/json", result, StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public void GetValueCompletions_EmptyContentType_Skips()
        {
            DirectoryStructure directoryStructure = new DirectoryStructure(null);
            RequestInfo requestInfo = new RequestInfo();
            requestInfo.SetRequestBody("GET", "application/json", "");
            requestInfo.SetRequestBody("GET", "", "");
            directoryStructure.RequestInfo = requestInfo;

            HttpState httpState = SetupHttpState();
            httpState.BaseAddress = new Uri("https://localhost/");
            ApiDefinition apiDefinition = new ApiDefinition();
            apiDefinition.DirectoryStructure = directoryStructure;
            httpState.ApiDefinition = apiDefinition;

            IEnumerable<string> result = HeaderCompletion.GetValueCompletions(method: "GET", path: "", header: "Content-Type", "", httpState);

            Assert.Single(result);
            Assert.Contains("application/json", result, StringComparer.OrdinalIgnoreCase);
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
