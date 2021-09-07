// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System.Collections.Generic;

namespace Microsoft.Repl.Commanding
{
    public class CommandOptionSpecification
    {
        public string Id { get; }

        public IReadOnlyList<string> Forms { get; }

        public int MaximumOccurrences { get; }

        public int MinimumOccurrences { get; }

        public bool AcceptsValue { get; }

        public bool RequiresValue { get; }

        public CommandOptionSpecification(string id, bool acceptsValue = false, bool requiresValue = false, int minimumOccurrences = 0, int maximumOccurrences = int.MaxValue, params string[] forms)
        {
            Id = id;
            Forms = forms;
            MinimumOccurrences = minimumOccurrences;
            MaximumOccurrences = maximumOccurrences > minimumOccurrences ? maximumOccurrences : minimumOccurrences;
            RequiresValue = requiresValue;
            AcceptsValue = RequiresValue || acceptsValue;
        }
    }
}
