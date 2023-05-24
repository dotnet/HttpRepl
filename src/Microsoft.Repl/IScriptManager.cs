// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System.Collections.Generic;

namespace Microsoft.Repl
{
    public interface IScriptManager
    {
        bool IsActive { get; set;  }
        bool Verbose { get; set; }
        bool VerboseFailure { get; set; }
        bool FirstFailure { get; set; }
        bool CancellationFromFailure { get; set; }
        void AddStatus(string status);
        string GetStatus();
        void Reset();
        int CurrentRequest { get; set; }
        int NumberOfRequests { get; set; }
    }
}
