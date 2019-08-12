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
using Microsoft.HttpRepl.Suggestions;
using Microsoft.HttpRepl.UserProfile;
using Xunit;

namespace Microsoft.HttpRepl.Tests.Suggestions
{
    public class ServerPathCompletionTests
    {
        [Fact]
        public void GetCompletions_AbsoluteUri_ReturnsNull()
        {
            HttpState httpState = SetupHttpState();

            IEnumerable<string> result = ServerPathCompletion.GetCompletions(httpState, "https://github.com/");

            Assert.Null(result);
        }

        [Fact]
        public void GetCompletions_WithBackslashes_ProperResults()
        {
            HttpState httpState = SetupHttpState();

            IEnumerable<string> result = ServerPathCompletion.GetCompletions(httpState, "child1\\g");

            Assert.Equal(2, result.Count());
            Assert.Contains("child1/grandchild1", result, StringComparer.OrdinalIgnoreCase);
            Assert.Contains("child1/grandchild2", result, StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public void GetCompletions_MixedCase_ProperResults()
        {
            HttpState httpState = SetupHttpState();

            IEnumerable<string> result = ServerPathCompletion.GetCompletions(httpState, "cHiLd1/gRaNd");

            Assert.Equal(2, result.Count());
            Assert.Contains("child1/grandchild1", result, StringComparer.OrdinalIgnoreCase);
            Assert.Contains("child1/grandchild2", result, StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public void GetCompletions_OnlyRoot_ProperResults()
        {
            HttpState httpState = SetupHttpState();

            IEnumerable<string> result = ServerPathCompletion.GetCompletions(httpState, "ch");

            Assert.Equal(2, result.Count());
            Assert.Contains("child1", result, StringComparer.OrdinalIgnoreCase);
            Assert.Contains("child2", result, StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public void GetCompletions_EmptyDirectory_ReturnsEmpty()
        {
            HttpState httpState = SetupHttpState();

            IEnumerable<string> result = ServerPathCompletion.GetCompletions(httpState, "child1/grandchild1/g");

            Assert.Empty(result);
        }

        [Fact]
        public void GetCompletions_NoMatches_ReturnsEmpty()
        {
            HttpState httpState = SetupHttpState();

            IEnumerable<string> result = ServerPathCompletion.GetCompletions(httpState, "child1/abc");

            Assert.Empty(result);
        }

        [Fact]
        public void GetCompletions_NullStructure_ReturnsNull()
        {
            HttpState httpState = SetupHttpState();
            httpState.ApiDefinition = null;

            IEnumerable<string> result = ServerPathCompletion.GetCompletions(httpState, "c");

            Assert.Null(result);
        }

        private static HttpState SetupHttpState()
        {
            IFileSystem fileSystem = new FileSystemStub();
            IUserProfileDirectoryProvider userProfileDirectoryProvider = new UserProfileDirectoryProvider();
            IPreferences preferences = new UserFolderPreferences(fileSystem, userProfileDirectoryProvider, null);
            HttpClient httpClient = new HttpClient();

            HttpState httpState = new HttpState(fileSystem, preferences, httpClient);

            DirectoryStructure structure = new DirectoryStructure(null);
            DirectoryStructure child1 = structure.DeclareDirectory("child1");
            structure.DeclareDirectory("child2");
            child1.DeclareDirectory("grandchild1");
            child1.DeclareDirectory("grandchild2");

            ApiDefinition apiDefinition = new ApiDefinition();
            apiDefinition.DirectoryStructure = structure;
            httpState.ApiDefinition = apiDefinition;

            return httpState;
        }
    }
}
