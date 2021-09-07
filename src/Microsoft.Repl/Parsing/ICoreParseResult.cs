// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System.Collections.Generic;

namespace Microsoft.Repl.Parsing
{
    public interface ICoreParseResult
    {
        int CaretPositionWithinCommandText { get; }

        int CaretPositionWithinSelectedSection { get; }

        string CommandText { get; }

        IReadOnlyList<string> Sections { get; }

        bool IsQuotedSection(int index);

        int SelectedSection { get; }

        IReadOnlyDictionary<int, int> SectionStartLookup { get; }

        ICoreParseResult Slice(int numberOfLeadingSectionsToRemove);
    }
}
