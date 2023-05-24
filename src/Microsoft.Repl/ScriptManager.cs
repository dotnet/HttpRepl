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
            Verbose = false;
            VerboseFailure = false;
            FirstFailure = false;
            CancellationFromFailure = false;

        }

        public bool IsActive { get; set; }

        public bool Verbose { get; set; }
        public bool VerboseFailure { get; set; }
        public bool FirstFailure { get; set; }
        public bool CancellationFromFailure { get; set; }
        private Dictionary<string, IEnumerable<string>> Statuses { get; set; }

        public int NumberOfRequests { get; set; }

        
        public int CurrentRequest { get; set; }

        public void AddStatus(string status)
        {
            if (Statuses.TryGetValue(status, out IEnumerable<string> values))
            {
                Statuses[status] = values.Append(status);

            }
            else
            {
                Statuses.Add(status, Enumerable.Repeat(status, 1));
            }
            
        }

        public string GetStatus()
        {
            StringBuilder stringBuilder = new();
            foreach (KeyValuePair<string, IEnumerable<string>> entry in Statuses)
            {
                stringBuilder.Append($"Status code {entry.Key} had this many results: {entry.Value.Count()} \r\n");              
            }
            return stringBuilder.ToString();
        }

        public void Reset()
        {
            IsActive = false;
            Statuses = new Dictionary<string, IEnumerable<string>>();
            CurrentRequest = 0;
            NumberOfRequests = 0;
            Verbose = false;
            VerboseFailure = false;
            FirstFailure = false;
            CancellationFromFailure = false;
            IsActive = false;
            Statuses = new Dictionary<string, IEnumerable<string>>();
            CurrentRequest = 0;
            NumberOfRequests = 0;
            Verbose = false;
            VerboseFailure = false;
            FirstFailure = false;
            CancellationFromFailure = false;
        }
    }
}
