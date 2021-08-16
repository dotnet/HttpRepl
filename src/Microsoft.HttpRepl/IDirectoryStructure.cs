// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System.Collections.Generic;

namespace Microsoft.HttpRepl
{
    public interface IDirectoryStructure
    {
        IEnumerable<string> DirectoryNames { get; }

        IDirectoryStructure Parent { get; }

        IDirectoryStructure GetChildDirectory(string name);

        IRequestInfo RequestInfo { get; }
    }
}
