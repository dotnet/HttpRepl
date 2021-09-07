// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System.Collections.Generic;

namespace Microsoft.Repl.Commanding
{
    public class CommandInputSpecification
    {
        public IReadOnlyList<IReadOnlyList<string>> CommandName { get; }

        public char OptionPreamble { get; }

        public int MinimumArguments { get; }

        public int MaximumArguments { get; }

        public IReadOnlyList<CommandOptionSpecification> Options { get; }

        public CommandInputSpecification(IReadOnlyList<IReadOnlyList<string>> name, char optionPreamble, IReadOnlyList<CommandOptionSpecification> options, int minimumArgs, int maximumArgs)
        {
            CommandName = name;
            OptionPreamble = optionPreamble;
            MinimumArguments = minimumArgs;
            MaximumArguments = maximumArgs;

            if (MinimumArguments < 0)
            {
                MinimumArguments = 0;
            }

            if (MaximumArguments < MinimumArguments)
            {
                MaximumArguments = MinimumArguments;
            }

            Options = options;
        }

        public static CommandInputSpecificationBuilder Create(string baseName, params string[] additionalNameParts)
        {
            List<string> nameParts = new List<string> {baseName};
            nameParts.AddRange(additionalNameParts);
            return new CommandInputSpecificationBuilder(nameParts);
        }
    }
}
