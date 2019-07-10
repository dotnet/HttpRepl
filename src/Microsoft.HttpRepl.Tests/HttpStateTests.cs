// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
