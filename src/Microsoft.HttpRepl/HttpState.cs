// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.HttpRepl.Diagnostics;
using Microsoft.HttpRepl.FileSystem;
using Microsoft.HttpRepl.Preferences;
using Microsoft.Repl.ConsoleHandling;

namespace Microsoft.HttpRepl
{
    public class HttpState
    {
        private readonly IFileSystem _fileSystem;
        private readonly IPreferences _preferences;

        public HttpClient Client { get; }

        public AllowedColors ErrorColor => _preferences.GetColorValue(WellKnownPreference.ErrorColor, AllowedColors.BoldRed);

        public AllowedColors WarningColor => _preferences.GetColorValue(WellKnownPreference.WarningColor, AllowedColors.BoldYellow);

        public Stack<string> PathSections { get; }

        public IDirectoryStructure SwaggerStructure { get; set; }

        public IDirectoryStructure Structure => DiagnosticsState.DiagEndpointsStructure == null
            ? SwaggerStructure
            : SwaggerStructure == null
                ? DiagnosticsState.DiagEndpointsStructure
                : new AggregateDirectoryStructure(SwaggerStructure, DiagnosticsState.DiagEndpointsStructure);

        public Uri BaseAddress { get; set; }

        public bool EchoRequest { get; set; }

        public Dictionary<string, IEnumerable<string>> Headers { get; }

        public DiagnosticsState DiagnosticsState { get; }

        public Uri SwaggerEndpoint { get; set; }

        public HttpState(IFileSystem fileSystem, IPreferences preferences)
        {
            _fileSystem = fileSystem;
            _preferences = preferences;
            Client = new HttpClient();
            PathSections = new Stack<string>();
            Headers = new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "User-Agent", new[] { "HTTP-REPL" } }
            };
            DiagnosticsState = new DiagnosticsState();
        }

        public string GetPrompt()
        {
            return $"{GetEffectivePath(new string[0], false, out int _)?.ToString() ?? "(Disconnected)"}~ ";
        }

        public string GetExampleBody(string path, ref string contentType, string method)
        {
            Uri effectivePath = GetEffectivePath(path);
            string rootRelativePath = effectivePath.LocalPath.Substring(BaseAddress.LocalPath.Length).TrimStart('/');
            IDirectoryStructure structure = SwaggerStructure?.TraverseTo(rootRelativePath);
            return structure?.RequestInfo?.GetRequestBodyForContentType(ref contentType, method);
        }

        public IEnumerable<string> GetApplicableContentTypes(string method, string path)
        {
            Uri effectivePath = GetEffectivePath(path);
            string rootRelativePath = effectivePath.LocalPath.Substring(BaseAddress.LocalPath.Length).TrimStart('/');
            IDirectoryStructure structure = SwaggerStructure?.TraverseTo(rootRelativePath);
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
            if (Uri.TryCreate(commandSpecifiedPath, UriKind.Absolute, out Uri absoluteUri))
            {
                return absoluteUri;
            }

            UriBuilder builder = new UriBuilder(BaseAddress);
            string path = string.Join('/', PathSections.Reverse());
            string[] parts = path.Split('?');
            string query = null;
            string query2 = null;

            if (parts.Length > 1)
            {
                path = parts[0];
                query = string.Join('?', parts.Skip(1));
            }

            builder.Path += path;

            if (commandSpecifiedPath.Length > 0)
            {
                if (commandSpecifiedPath[0] != '/')
                {
                    string argPath = commandSpecifiedPath;
                    if (builder.Path.Length > 0 && builder.Path[builder.Path.Length - 1] != '/')
                    {
                        argPath = "/" + argPath;
                    }

                    int queryIndex = argPath.IndexOf('?');
                    path = argPath;

                    if (queryIndex > -1)
                    {
                        query2 = argPath.Substring(queryIndex + 1);
                        path = argPath.Substring(0, queryIndex);
                    }

                    builder.Path += path;
                }
                else
                {
                    int queryIndex = commandSpecifiedPath.IndexOf('?');
                    path = commandSpecifiedPath;

                    if (queryIndex > -1)
                    {
                        query2 = commandSpecifiedPath.Substring(queryIndex + 1);
                        path = commandSpecifiedPath.Substring(0, queryIndex);
                    }

                    builder.Path = path;
                }
            }
            else
            {

                int queryIndex = commandSpecifiedPath.IndexOf('?');
                path = commandSpecifiedPath;

                if (queryIndex > -1)
                {
                    query2 = commandSpecifiedPath.Substring(queryIndex + 1);
                    path = commandSpecifiedPath.Substring(0, queryIndex);
                }

                builder.Path += path;
            }

            if (query != null)
            {
                if (!string.IsNullOrEmpty(builder.Query))
                {
                    query = "&" + query;
                }

                builder.Query += query;
            }

            if (query2 != null)
            {
                if (!string.IsNullOrEmpty(builder.Query))
                {
                    query2 = "&" + query2;
                }

                builder.Query += query2;
            }

            return builder.Uri;
        }

        public Uri GetEffectivePath(IReadOnlyList<string> sections, bool requiresBody, out int filePathIndex)
        {
            filePathIndex = 1;

            if (BaseAddress == null)
            {
                return null;
            }

            UriBuilder builder = new UriBuilder(BaseAddress);
            string path = string.Join('/', PathSections.Reverse());
            string[] parts = path.Split('?');
            string query = null;
            string query2 = null;

            if (parts.Length > 1)
            {
                path = parts[0];
                query = string.Join('?', parts.Skip(1));
            }

            builder.Path += path;

            if (sections.Count > 1)
            {
                if (!requiresBody || !_fileSystem.FileExists(sections[1]))
                {
                    if (sections[1].Length > 0)
                    {
                        if (sections[1][0] != '/')
                        {
                            string argPath = sections[1];
                            if (builder.Path.Length > 0 && builder.Path[builder.Path.Length - 1] != '/')
                            {
                                argPath = "/" + argPath;
                            }

                            int queryIndex = argPath.IndexOf('?');
                            path = argPath;

                            if (queryIndex > -1)
                            {
                                query2 = argPath.Substring(queryIndex + 1);
                                path = argPath.Substring(0, queryIndex);
                            }

                            builder.Path += path;
                        }
                        else
                        {
                            int queryIndex = sections[1].IndexOf('?');
                            path = sections[1];

                            if (queryIndex > -1)
                            {
                                query2 = sections[1].Substring(queryIndex + 1);
                                path = sections[1].Substring(0, queryIndex);
                            }

                            builder.Path = path;
                        }
                    }
                    else
                    {

                        int queryIndex = sections[1].IndexOf('?');
                        path = sections[1];

                        if (queryIndex > -1)
                        {
                            query2 = sections[1].Substring(queryIndex + 1);
                            path = sections[1].Substring(0, queryIndex);
                        }

                        builder.Path += path;
                    }

                    filePathIndex = 2;
                }
            }

            if (query != null)
            {
                if (!string.IsNullOrEmpty(builder.Query))
                {
                    query = "&" + query;
                }

                builder.Query += query;
            }

            if (query2 != null)
            {
                if (!string.IsNullOrEmpty(builder.Query))
                {
                    query2 = "&" + query2;
                }

                builder.Query += query2;
            }

            return builder.Uri;
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
    }
}
