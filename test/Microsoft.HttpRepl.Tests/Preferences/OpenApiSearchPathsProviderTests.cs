// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.HttpRepl.Fakes;
using Microsoft.HttpRepl.Preferences;
using Xunit;

namespace Microsoft.HttpRepl.Tests.Preferences
{
    public class OpenApiSearchPathsProviderTests
    {
        [Fact]
        public void WithNoOverrides_ReturnsDefault()
        {
            // Arrange
            NullPreferences preferences = new();
            OpenApiSearchPathsProvider provider = new(preferences);
            IEnumerable<string> expectedPaths = OpenApiSearchPathsProvider.DefaultSearchPaths;

            // Act
            IEnumerable<string> paths = provider.GetOpenApiSearchPaths();

            // Assert
            AssertPathLists(expectedPaths, paths);
        }

        [Fact]
        public void WithFullOverride_ReturnsConfiguredOverride()
        {
            // Arrange
            string searchPathOverrides = "/red|/green|/blue";
            FakePreferences preferences = new();
            preferences.SetValue(WellKnownPreference.SwaggerSearchPaths, searchPathOverrides);
            OpenApiSearchPathsProvider provider = new(preferences);
            string[] expectedPaths = searchPathOverrides.Split('|');

            // Act
            IEnumerable<string> paths = provider.GetOpenApiSearchPaths();

            // Assert
            AssertPathLists(expectedPaths, paths);
        }

        [Fact]
        public void WithAdditions_ReturnsDefaultPlusAdditions()
        {
            // Arrange
            string[] searchPathAdditions = new[] { "/red", "/green", "/blue" };
            FakePreferences preferences = new();
            preferences.SetValue(WellKnownPreference.SwaggerAddToSearchPaths, string.Join('|', searchPathAdditions));
            OpenApiSearchPathsProvider provider = new(preferences);
            IEnumerable<string> expectedPaths = OpenApiSearchPathsProvider.DefaultSearchPaths.Union(searchPathAdditions);

            // Act
            IEnumerable<string> paths = provider.GetOpenApiSearchPaths();

            // Assert
            AssertPathLists(expectedPaths, paths);
        }

        [Fact]
        public void WithRemovals_ReturnsDefaultMinusRemovals()
        {
            // Arrange
            string[] searchPathRemovals = new[] { "swagger.json", "/swagger.json", "swagger/v1/swagger.json", "/swagger/v1/swagger.json" };
            FakePreferences preferences = new();
            preferences.SetValue(WellKnownPreference.SwaggerRemoveFromSearchPaths, string.Join('|', searchPathRemovals));
            OpenApiSearchPathsProvider provider = new(preferences);
            IEnumerable<string> expectedPaths = OpenApiSearchPathsProvider.DefaultSearchPaths.Except(searchPathRemovals);

            // Act
            IEnumerable<string> paths = provider.GetOpenApiSearchPaths();

            // Assert
            AssertPathLists(expectedPaths, paths);
        }

        [Fact]
        public void WithAdditionsAndRemovals_ReturnsCorrectSet()
        {
            // Arrange
            string[] searchPathAdditions = new[] { "/red", "/green", "/blue" };
            string[] searchPathRemovals = new[] { "swagger.json", "/swagger.json", "swagger/v1/swagger.json", "/swagger/v1/swagger.json" };
            FakePreferences preferences = new();
            preferences.SetValue(WellKnownPreference.SwaggerAddToSearchPaths, string.Join('|', searchPathAdditions));
            preferences.SetValue(WellKnownPreference.SwaggerRemoveFromSearchPaths, string.Join('|', searchPathRemovals));
            OpenApiSearchPathsProvider provider = new(preferences);
            IEnumerable<string> expectedPaths = OpenApiSearchPathsProvider.DefaultSearchPaths.Union(searchPathAdditions).Except(searchPathRemovals);

            // Act
            IEnumerable<string> paths = provider.GetOpenApiSearchPaths();

            // Assert
            AssertPathLists(expectedPaths, paths);
        }

        private static void AssertPathLists(IEnumerable<string> expectedPaths, IEnumerable<string> paths)
        {
            Assert.NotNull(expectedPaths);
            Assert.NotNull(paths);

            IEnumerator<string> expectedPathsEnumerator = expectedPaths.GetEnumerator();
            IEnumerator<string> pathsEnumerator = paths.GetEnumerator();

            while (expectedPathsEnumerator.MoveNext())
            {
                Assert.True(pathsEnumerator.MoveNext(), $"Missing path \"{expectedPathsEnumerator.Current}\"");
                Assert.Equal(expectedPathsEnumerator.Current, pathsEnumerator.Current, StringComparer.Ordinal);
            }

            if (pathsEnumerator.MoveNext())
            {
                // We can't do a one-liner here like the Missing path version above because
                // the order the second parameter is evaluated regardless of the result of the
                // evaluation of the first parameter. Also xUnit doesn't have an Assert.Fail,
                // so we have to use Assert.True(false) per their comparison chart.
                Assert.True(false, $"Extra path \"{pathsEnumerator.Current}\"");
            }
        }
    }
}
