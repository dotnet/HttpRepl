// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

namespace Microsoft.Repl.Commanding
{
    public class InputElement
    {
        public CommandInputLocation Location { get; }

        public string Text { get; }

        public string NormalizedText { get; }

        public InputElement Owner { get; }

        public int ParseResultSectionIndex { get; }

        public InputElement(CommandInputLocation location, string text, string normalizedText, int sectionIndex)
            : this(null, location, text, normalizedText, sectionIndex)
        {
        }

        public InputElement(InputElement owner, CommandInputLocation location, string text, string normalizedText, int sectionIndex)
        {
            Owner = owner;
            Location = location;
            Text = text;
            NormalizedText = normalizedText;
            ParseResultSectionIndex = sectionIndex;
        }
    }
}
