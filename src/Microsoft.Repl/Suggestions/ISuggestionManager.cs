// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

namespace Microsoft.Repl.Suggestions
{
    public interface ISuggestionManager
    {
        void NextSuggestion(IShellState shellState);

        void PreviousSuggestion(IShellState shellState);
    }
}
