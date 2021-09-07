// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

namespace Microsoft.Repl.Commanding
{
    public class CommandInputProcessingIssue
    {
        public CommandInputProcessingIssueKind Kind { get; }

        public string Text { get; }

        public CommandInputProcessingIssue(CommandInputProcessingIssueKind kind, string text)
        {
            Kind = kind;
            Text = text;
        }
    }
}
