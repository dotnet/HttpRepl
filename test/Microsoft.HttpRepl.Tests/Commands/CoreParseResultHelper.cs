// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
