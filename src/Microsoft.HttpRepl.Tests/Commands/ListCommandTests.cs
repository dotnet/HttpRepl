// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.Fakes;
using Microsoft.HttpRepl.Preferences;
using Microsoft.Repl.Parsing;
using Xunit;

namespace Microsoft.HttpRepl.Tests.Commands
{
    public class ListCommandTests : CommandTestsBase
    {
        [Fact]
        public void Suggest_WithBlankSecondParameter_NoExceptionAndNullSuggestions()
        {
            ArrangeInputs(commandText: "dir ",
                          baseAddress: "https://localhost/",
                          path: "/",
                          urlsWithResponse: null,
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out _,
                          out IPreferences preferences);

            ListCommand listCommand = new ListCommand(preferences);

            IEnumerable<string> suggestions = listCommand.Suggest(shellState, httpState, parseResult);

            Assert.Empty(suggestions);
        }
    }
}
