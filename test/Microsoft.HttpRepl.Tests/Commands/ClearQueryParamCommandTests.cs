// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.Fakes;
using Microsoft.Repl.Parsing;
using Xunit;

namespace Microsoft.HttpRepl.Tests.Commands
{
    public class ClearQueryParamCommandTests: CommandTestsBase
    {        
        [Fact]
        public void CanHandle_WithFirstParseResultSectionNotEqualToName_ReturnsNull()
        {
            ArrangeInputs(parseResultSections: "test query-param name",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            ClearQueryParamCommand clearQueryParamCommand = new ClearQueryParamCommand();
            bool? result = clearQueryParamCommand.CanHandle(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithSecondParseResultSectionNotEqualToSubCommand_ReturnsNull()
        {
            ArrangeInputs(parseResultSections: "set base name",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            ClearQueryParamCommand clearQueryParamCommand = new ClearQueryParamCommand();
            bool? result = clearQueryParamCommand.CanHandle(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithValidInput_ReturnsTrue()
        {
            ArrangeInputs(parseResultSections: "clear query-param",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            ClearQueryParamCommand clearQueryParamCommand = new ClearQueryParamCommand();
            bool? result = clearQueryParamCommand.CanHandle(shellState, httpState, parseResult);

            Assert.True(result);
        }

        [Fact]
        public void GetHelpSummary_ReturnsDescription()
        {
            ArrangeInputs(parseResultSections: string.Empty,
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult _);

            ClearQueryParamCommand clearQueryParamCommand = new ClearQueryParamCommand();
            string result = clearQueryParamCommand.GetHelpSummary(shellState, httpState);

            Assert.Equal(ClearQueryParamCommand.Description, result);
        }

        [Fact]
        public void GetHelpDetails_ReturnsHelpDetails()
        {
            ArrangeInputs(parseResultSections: "clear query-param",
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult parseResult);

            string expected = "\u001b[1mUsage: \u001b[39mclear query-param" + Environment.NewLine + Environment.NewLine + "Clears the query string of all key and values" + Environment.NewLine;

            ClearQueryParamCommand clearQueryParamCommand = new ClearQueryParamCommand();
            string result = clearQueryParamCommand.GetHelpDetails(shellState, httpState, parseResult);

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task ClearQueryParamCommandWorkingAsync()
        {
            ArrangeInputs(parseResultSections: "add query-param limit 5",
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult parseResult);

            AddQueryParamCommand addQueryParamCommand = new AddQueryParamCommand();
            await addQueryParamCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Dictionary<string, IEnumerable<string>> queryParamAfterAdd = httpState.QueryParam;
            Assert.True(queryParamAfterAdd.ContainsKey("limit"));

            ClearQueryParamCommand clearQueryParamCommand = new ClearQueryParamCommand();
            await clearQueryParamCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Dictionary<string, IEnumerable<string>> queryParamAfterClear = httpState.QueryParam;
            Assert.True(queryParamAfterClear.ContainsKey("limit"));         
        }
    }
}
