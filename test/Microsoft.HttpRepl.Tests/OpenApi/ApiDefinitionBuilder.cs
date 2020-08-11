// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.HttpRepl.OpenApi;

namespace Microsoft.HttpRepl.Tests.OpenApi
{
    internal class ApiDefinitionBuilder
    {
        private readonly List<ApiDefinitionBaseAddress> _baseAddresses = new();
        private readonly List<DirectoryBuilder<ApiDefinitionBuilder>> _directories = new();

        private ApiDefinitionBuilder() { }

        public static ApiDefinitionBuilder Start() => new ApiDefinitionBuilder();

        public ApiDefinitionBuilder AddBaseAddress(string url, string description)
        {
            _baseAddresses.Add(new ApiDefinitionBaseAddress(url, description));
            return this;
        }

        public DirectoryBuilder<ApiDefinitionBuilder> AddDirectory(string directory)
        {
            DirectoryBuilder<ApiDefinitionBuilder> directoryBuilder = new(this, directory);
            _directories.Add(directoryBuilder);
            return directoryBuilder;
        }

        public ApiDefinition Build()
        {
            ApiDefinition apiDefinition = new();
            apiDefinition.DirectoryStructure = new DirectoryStructure(null);

            foreach (ApiDefinitionBaseAddress baseAddress in _baseAddresses)
            {
                apiDefinition.BaseAddresses.Add(new ApiDefinition.Server() { Url = baseAddress.Url, Description = baseAddress.Description });
            }

            foreach (DirectoryBuilder<ApiDefinitionBuilder> subDirectoryBuilder in _directories)
            {
                DirectoryStructure subdirectory = ((DirectoryStructure)apiDefinition.DirectoryStructure).DeclareDirectory(subDirectoryBuilder.Name);
                BuildDirectory(subDirectoryBuilder, subdirectory);
            }

            return apiDefinition;
        }

        private void BuildDirectory<T>(DirectoryBuilder<T> directoryBuilder, DirectoryStructure directoryStructure)
        {
            if (directoryBuilder.Methods.Count > 0)
            {
                RequestInfo requestInfo = new();

                foreach (string method in directoryBuilder.Methods)
                {
                    requestInfo.AddMethod(method);
                }

                directoryStructure.RequestInfo = requestInfo;
            }

            foreach (DirectoryBuilder<DirectoryBuilder<T>> subDirectoryBuilder in directoryBuilder.Directories)
            {
                DirectoryStructure subdirectory = directoryStructure.DeclareDirectory(subDirectoryBuilder.Name);
                BuildDirectory(subDirectoryBuilder, subdirectory);
            }
        }

        private class ApiDefinitionBaseAddress
        {
            public ApiDefinitionBaseAddress(string url, string description)
            {
                Url = new Uri(url);
                Description = description;
            }

            public Uri Url { get; }
            public string Description { get; }
        }

        internal class DirectoryBuilder<T> 
        {
            private T _parent;
            private List<DirectoryBuilder<DirectoryBuilder<T>>> _directories = new();
            private List<string> _methods = new();

            public DirectoryBuilder(T parent, string name)
            {
                _parent = parent;
                Name = name;
            }

            public IReadOnlyList<DirectoryBuilder<DirectoryBuilder<T>>> Directories => _directories;
            public IReadOnlyList<string> Methods => _methods;
            public string Name { get; }

            public DirectoryBuilder<DirectoryBuilder<T>> AddDirectory(string directory)
            {
                DirectoryBuilder<DirectoryBuilder<T>> directoryBuilder = new(this, directory);
                _directories.Add(directoryBuilder);
                return directoryBuilder;
            }

            public DirectoryBuilder<T> AddMethod(string method)
            {
                _methods.Add(method);
                return this;
            }

            public DirectoryBuilder<T> WithGet()
            {
                return AddMethod("Get");
            }

            public DirectoryBuilder<T> WithPost()
            {
                return AddMethod("Post");
            }

            public DirectoryBuilder<T> WithPatch()
            {
                return AddMethod("Patch");
            }

            public DirectoryBuilder<T> WithDelete()
            {
                return AddMethod("Delete");
            }

            public T Finalize()
            {
                return _parent;
            }
        }
    }
}
