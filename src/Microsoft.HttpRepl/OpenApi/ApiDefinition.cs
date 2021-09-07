// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.HttpRepl.OpenApi
{
    public class ApiDefinition
    {
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
