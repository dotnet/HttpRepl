// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System.Collections.Generic;

namespace Microsoft.Repl
{
    public interface IScriptManager
    {
        bool IsActive { get; set;  }
        Dictionary<string, IEnumerable<string>> Statuses { get; }
        int NumberOfRequests { get; set; }
    }
}
