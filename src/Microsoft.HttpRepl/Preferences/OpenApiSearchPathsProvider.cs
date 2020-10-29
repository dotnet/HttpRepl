// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.HttpRepl.OpenApi;

namespace Microsoft.HttpRepl.Preferences
{
    internal class OpenApiSearchPathsProvider : IOpenApiSearchPathsProvider
    {
        // OpenAPI description search paths are appended to the base url to
        // attempt to find the description document. A search path is a
        // relative url that is appended to the base url using Uri.TryCreate,
        // so the semantics of relative urls matter here.
        // Example: Base path https://localhost/v1/ and search path openapi.json
        //          will result in https://localhost/v1/openapi.json being tested.
        // Example: Base path https://localhost/v1/ and search path /openapi.json
        //          will result in https://localhost/openapi.json being tested.
        internal static IEnumerable<string> DefaultSearchPaths { get; } = new[] {
            "swagger.json",
            "/swagger.json",
            "swagger/v1/swagger.json",
            "/swagger/v1/swagger.json",
            "openapi.json",
            "/openapi.json",
        };

        private readonly IPreferences _preferences;
        public OpenApiSearchPathsProvider(IPreferences preferences)
        {
            _preferences = preferences;
        }

        public IEnumerable<string> GetOpenApiSearchPaths()
        {
            string[] configSearchPaths = Split(_preferences.GetValue(WellKnownPreference.SwaggerSearchPaths));

            if (configSearchPaths.Length > 0)
            {
                return configSearchPaths;
            }

            string[] addToSearchPaths = Split(_preferences.GetValue(WellKnownPreference.SwaggerAddToSearchPaths));
            string[] removeFromSearchPaths = Split(_preferences.GetValue(WellKnownPreference.SwaggerRemoveFromSearchPaths));

            return DefaultSearchPaths.Union(addToSearchPaths).Except(removeFromSearchPaths);
        }

        private static string[] Split(string searchPaths)
        {
            if (string.IsNullOrWhiteSpace(searchPaths))
            {
                return Array.Empty<string>();
            }
            else
            {
                return searchPaths.Split('|', StringSplitOptions.RemoveEmptyEntries);
            }
        }
    }
}
