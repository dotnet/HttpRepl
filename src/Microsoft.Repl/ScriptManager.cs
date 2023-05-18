// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Repl
{
    public class ScriptManager : IScriptManager
    {
        public ScriptManager() {
            IsActive = false;
            Statuses = new Dictionary<string, IEnumerable<string>>();
            CurrentRequest = 0;
            NumberOfRequests = 0;
        }

        public bool IsActive { get; set; }

        public Dictionary<string, IEnumerable<string>> Statuses { get; set; }

        public int NumberOfRequests { get; set; }
        public int CurrentRequest { get; set; }
    }
}
