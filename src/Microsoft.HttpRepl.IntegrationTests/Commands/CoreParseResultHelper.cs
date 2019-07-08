// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Repl.Parsing;

namespace Microsoft.HttpRepl.IntegrationTests.Commands
{
    internal static class CoreParseResultHelper
    {
        public static ICoreParseResult Create(string commandText)
        {
            if (commandText == null)
            {
                throw new ArgumentNullException(nameof(commandText));
            }

            CoreParser coreParser = new CoreParser();
            ICoreParseResult parseResult = coreParser.Parse(commandText, commandText.Length);

            return parseResult;
        }
    }
}
