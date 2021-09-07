// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System;
using Microsoft.Repl.Parsing;

namespace Microsoft.HttpRepl.Tests.Commands
{
    internal static class CoreParseResultHelper
    {
        public static ICoreParseResult Create(string commandText, int caretPosition = -1)
        {
            if (commandText == null)
            {
                throw new ArgumentNullException(nameof(commandText));
            }

            if (caretPosition == -1)
            {
                caretPosition = commandText.Length;
            }

            CoreParser coreParser = new CoreParser();
            ICoreParseResult parseResult = coreParser.Parse(commandText, caretPosition);

            return parseResult;
        }
    }
}
