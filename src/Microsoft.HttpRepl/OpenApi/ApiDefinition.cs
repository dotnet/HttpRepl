// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.HttpRepl.OpenApi
{
    public class ApiDefinition
    {
        public Uri SourceEndpoint { get; set; }
        public IList<Server> BaseAddresses { get; } = new List<Server>();
        public IDirectoryStructure DirectoryStructure { get; set; }

        [SuppressMessage("Design", "CA1724:Type names should not match namespaces", Justification = "This is a valid name for this type.")]
        public class Server
        {
            public Uri Url { get; set; }
            public string Description { get; set; }
        }
    }
}
