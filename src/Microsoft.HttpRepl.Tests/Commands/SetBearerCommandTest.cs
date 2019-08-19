
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.Fakes;
using Microsoft.Repl.Parsing;
using Xunit;

namespace Microsoft.HttpRepl.Tests.Commands
{
    public class SetBearerCommandTest : CommandTestsBase
    {
        [Fact]
        public void CanHandle_WithParseResultSectionsLessThanTwo_ReturnsNull()
        {
            ArrangeInputs(parseResultSections: "set",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            SetBearerCommand setBearerCommand = new SetBearerCommand();
            bool? result = setBearerCommand.CanHandle(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithFirstParseResultSectionNotEqualToName_ReturnsNull()
        {
            ArrangeInputs(parseResultSections: "test bearer token",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            SetBearerCommand setBearerCommand = new SetBearerCommand();
            bool? result = setBearerCommand.CanHandle(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithSecondParseResultSectionNotEqualToSubCommand_ReturnsNull()
        {
            ArrangeInputs(parseResultSections: "set base token",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            SetBearerCommand setBearerCommand = new SetBearerCommand();
            bool? result = setBearerCommand.CanHandle(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithValidInput_ReturnsTrue()
        {
            ArrangeInputs(parseResultSections: "set bearer token",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            SetBearerCommand setBearerCommand = new SetBearerCommand();
            bool? result = setBearerCommand.CanHandle(shellState, httpState, parseResult);

            Assert.True(result);
        }


        [Fact]
        public void GetHelpSummary_ReturnsDescription()
        {
            ArrangeInputs(parseResultSections: string.Empty,
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult _);

            SetBearerCommand setBearerCommand = new SetBearerCommand();
            string result = setBearerCommand.GetHelpSummary(shellState, httpState);

            Assert.Equal(setBearerCommand.Description, result);
        }

        [Fact]
        public void GetHelpDetails_ReturnsHelpDetails()
        {
            ArrangeInputs(parseResultSections: "set bearer",
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult parseResult);

            string expected = "\u001b[1mUsage: \u001b[39mset bearer [token]"
                                + Environment.NewLine
                                + Environment.NewLine
                                + "Sets or clears the bearer authorization. When [token] is empty the bearer authorization is cleared."
                                + Environment.NewLine;

            SetBearerCommand setBearerCommand = new SetBearerCommand();
            string result = setBearerCommand.GetHelpDetails(shellState, httpState, parseResult);

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task ExecuteAsync_WithExactlyTwoValidParseResultSections_DoesNotSetToken()
        {
            ArrangeInputs(parseResultSections: "set bearer",
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult parseResult);

            SetBearerCommand setBearerCommand = new SetBearerCommand();
            await setBearerCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Null(httpState.BearerToken);
        }

        [Fact]
        public async Task ExecuteAsync_WithExactlyThreeValidParseResultSections_SetsEntryAsToken()
        {
            ArrangeInputs(parseResultSections: "set bearer aSampleJwtToken",
                             out MockedShellState shellState,
                             out HttpState httpState,
                             out ICoreParseResult parseResult);

            SetBearerCommand setBearerCommand = new SetBearerCommand();
            await setBearerCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Equal("aSampleJwtToken", httpState.BearerToken);
        }

        [Fact]
        public async Task ExecuteAsync_WithMoreThanThreeValidParseResultSections_SetsThirdSectionAsToken()
        {
            ArrangeInputs(parseResultSections: "set bearer aSampleJwtToken token token2 token3",
                             out MockedShellState shellState,
                             out HttpState httpState,
                             out ICoreParseResult parseResult);

            SetBearerCommand setBearerCommand = new SetBearerCommand();
            await setBearerCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Equal("aSampleJwtToken", httpState.BearerToken);
        }

        [Fact]
        public void Suggest_WithNoParseResultSections_ReturnsName()
        {
            ArrangeInputs(parseResultSections: string.Empty,
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult parseResult);

            SetBearerCommand setBearerCommand = new SetBearerCommand();
            IEnumerable<string> suggestions = setBearerCommand.Suggest(shellState, httpState, parseResult);

            Assert.Single(suggestions);
            Assert.Equal("set", suggestions.First());
        }

        [Fact]
        public void Suggest_WithOneParseResultSectionAndSelectedSectionAtZero_ReturnsName()
        {
            ArrangeInputs(parseResultSections: "set",
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult parseResult,
                 caretPosition: 0);

            SetBearerCommand setBearerCommand = new SetBearerCommand();
            IEnumerable<string> suggestions = setBearerCommand.Suggest(shellState, httpState, parseResult);

            Assert.Single(suggestions);
            Assert.Equal("set", suggestions.First());
        }

        [Fact]
        public void Suggest_WithOneParseResultSectionAndSelectedSectionGreaterThanZero_ReturnsName()
        {
            ArrangeInputs(parseResultSections: "set",
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult parseResult,
                 caretPosition: 3);

            SetBearerCommand setBearerCommand = new SetBearerCommand();
            IEnumerable<string> suggestions = setBearerCommand.Suggest(shellState, httpState, parseResult);

            Assert.Single(suggestions);
            Assert.Equal("set", suggestions.First());
        }

        [Fact]
        public void Suggest_WithMoreThanOneParseResultSectionAndSelectedSectionGreaterThanZero_ReturnsName()
        {
            ArrangeInputs(parseResultSections: "set bearer",
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult parseResult,
                 caretPosition: 10);

            SetBearerCommand setBearerCommand = new SetBearerCommand();
            IEnumerable<string> suggestions = setBearerCommand.Suggest(shellState, httpState, parseResult);

            Assert.Single(suggestions);
            Assert.Equal("bearer", suggestions.First());
        }


    }
}
