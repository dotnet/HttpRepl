// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.HttpRepl.FileSystem;
using Microsoft.HttpRepl.OpenApi;
using Microsoft.HttpRepl.Preferences;
using Microsoft.Repl.ConsoleHandling;

namespace Microsoft.HttpRepl
{
    public class HttpState
    {
        private readonly IPreferences _preferences;

        public HttpClient Client { get; }

        public AllowedColors ErrorColor => _preferences.GetColorValue(WellKnownPreference.ErrorColor, AllowedColors.BoldRed);

        public AllowedColors WarningColor => _preferences.GetColorValue(WellKnownPreference.WarningColor, AllowedColors.BoldYellow);

        public Stack<string> PathSections { get; }

        public ApiDefinition ApiDefinition { get; set; }

        public Uri BaseAddress { get; set; }

        public IDirectoryStructure Structure => ApiDefinition?.DirectoryStructure;

        public bool EchoRequest { get; set; }

        public Dictionary<string, IEnumerable<string>> Headers { get; }

        public Uri SwaggerEndpoint { get; set; }

        public HttpState(IPreferences preferences, HttpClient httpClient)
        {
            preferences = preferences ?? throw new ArgumentNullException(nameof(preferences));

            _preferences = preferences;
            Client = httpClient;
            PathSections = new Stack<string>();
            Headers = new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase);
            AddDefaultHeaders();
        }

        public string GetPrompt()
        {
            return $"{GetEffectivePathForPrompt()?.ToString() ?? "(Disconnected)"}> ";
        }

        public IEnumerable<string> GetApplicableContentTypes(string method, string path)
        {
            if (BaseAddress is null && !Uri.IsWellFormedUriString(path, UriKind.Absolute))
            {
                return null;
            }

            Uri effectivePath = GetEffectivePath(path);
            string rootRelativePath = effectivePath.LocalPath.Substring(BaseAddress.LocalPath.Length).TrimStart('/');
            IDirectoryStructure structure = Structure?.TraverseTo(rootRelativePath);
            IReadOnlyDictionary<string, IReadOnlyList<string>> contentTypesByMethod = structure?.RequestInfo?.ContentTypesByMethod;

            if (contentTypesByMethod != null)
            {
                if (method is null)
                {
                    return contentTypesByMethod.Values.SelectMany(x => x).Distinct(StringComparer.OrdinalIgnoreCase);
                }

                if (contentTypesByMethod.TryGetValue(method, out IReadOnlyList<string> contentTypes))
                {
                    return contentTypes;
                }
            }

            return null;
        }

        public Uri GetEffectivePath(string commandSpecifiedPath)
        {
            return GetEffectivePath(BaseAddress, string.Join('/', PathSections.Reverse()), commandSpecifiedPath);
        }

        public static Uri GetEffectivePath(Uri baseAddress, string pathSections, string commandSpecifiedPath)
        {
            // If an absolute uri string was already specified, just return that.
            if (Uri.IsWellFormedUriString(commandSpecifiedPath, UriKind.Absolute))
            {
                return new Uri(commandSpecifiedPath, UriKind.Absolute);
            }
            // If it wasn't, and there also isn't a base address, throw an exception
            else if (baseAddress == null)
            {
                throw new ArgumentNullException(nameof(baseAddress), string.Format(Resources.Strings.HttpState_Error_NoAbsoluteUriNoBaseAddress, nameof(commandSpecifiedPath), nameof(baseAddress)));
            }

            pathSections = pathSections ?? throw new ArgumentNullException(nameof(pathSections));

            UriBuilder builder = GetUriBuilderFromBaseAddressAndPath(baseAddress, pathSections, out string baseAndPathQuery);

            UpdateUriBuilderForSpecifiedPath(builder, commandSpecifiedPath, out string commandQuery);
            AppendQueryToBuilder(builder, baseAndPathQuery);
            AppendQueryToBuilder(builder, commandQuery);

            return builder.Uri;
        }

        public Uri GetEffectivePathForPrompt()
        {
            if (BaseAddress == null)
            {
                return null;
            }

            return GetEffectivePath(BaseAddress, string.Join('/', PathSections.Reverse()), "");
        }

        public string GetRelativePathString()
        {
            string pathString = "/";

            if (PathSections != null && PathSections.Count > 0)
            {
                pathString += string.Join("/", PathSections.Reverse());
            }

            return pathString;
        }

        private static UriBuilder GetUriBuilderFromBaseAddressAndPath(Uri baseAddress, string pathSections, out string query)
        {
            // Get a builder for the base address
            UriBuilder builder = new UriBuilder(baseAddress);

            // Get everything beyond the BaseAddress for the current location
            string path = pathSections;

            // Split that off into the path and the query string parameters (if any)
            string[] parts = path.Split('?');
            query = null;

            // If there are some query string parameters, split the path off so it doesn't
            // contain them and leave the query string parameters in query
            if (parts.Length > 1)
            {
                path = parts[0];
                query = string.Join('?', parts.Skip(1));
            }

            if (path.StartsWith('/'))
            {
                // Set the builder path to the current path
                builder.Path = path;
            }
            else
            {
                // Add the current path to the builder path
                builder.Path += path;
            }

            return builder;
        }

        private static void UpdateUriBuilderForSpecifiedPath(UriBuilder builder, string commandSpecifiedPath, out string query)
        {
            query = null;
            // If the parameter has a non-empty value
            if (!string.IsNullOrEmpty(commandSpecifiedPath))
            {
                // If the parameter doesn't start with a slash
                if (commandSpecifiedPath[0] != '/')
                {
                    // If the current builder path doesn't end with a slash, then add one to the parameter
                    string argPath = commandSpecifiedPath;
                    if (builder.Path.Length > 0 && builder.Path[builder.Path.Length - 1] != '/')
                    {
                        argPath = "/" + argPath;
                    }

                    // Split the parameter between the path and the query string
                    int queryIndex = argPath.IndexOf('?', StringComparison.Ordinal);
                    string path = argPath;

                    if (queryIndex > -1)
                    {
                        query = argPath.Substring(queryIndex + 1);
                        path = argPath.Substring(0, queryIndex);
                    }

                    // Add just the path part of the parameter to the current builder path
                    builder.Path += path;
                }
                // if the parameter does start with a slash
                else
                {
                    // Split the parameter between the path and the query string
                    int queryIndex = commandSpecifiedPath.IndexOf('?', StringComparison.Ordinal);
                    string path = commandSpecifiedPath;

                    if (queryIndex > -1)
                    {
                        query = commandSpecifiedPath.Substring(queryIndex + 1);
                        path = commandSpecifiedPath.Substring(0, queryIndex);
                    }

                    // Set the builder path to just the path part of the parameter
                    builder.Path = path;
                }
            }
        }

        private static void AppendQueryToBuilder(UriBuilder builder, string query)
        {
            if (query != null)
            {
                if (!string.IsNullOrEmpty(builder.Query))
                {
                    query = "&" + query;
                }

                builder.Query += query;
            }
        }

        private void AddDefaultHeaders()
        {
            Headers["User-Agent"] = new[] { _preferences.GetValue(WellKnownPreference.HttpClientUserAgent, "HTTP-REPL") };
        }

        public void ResetState(bool persistHeaders = false, bool persistPath = false)
        {
            BaseAddress = null;
            SwaggerEndpoint = null;

            if (!persistHeaders)
            {
                Headers.Clear();
                AddDefaultHeaders();
            }

            if (!persistPath)
            {
                PathSections.Clear();
            }
        }
    }
}
