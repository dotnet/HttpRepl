// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System.Linq;
using Microsoft.Repl.Parsing;
using Xunit;

namespace Microsoft.Repl.Tests.Parsing
{
    public class CoreParseResultTests
    {
        [Fact]
        public void Slice_WithZero_ReturnsThis()
        {
            CoreParser parser = new CoreParser();
            string commandText = "set header content-type application/json";
            ICoreParseResult parseResult = parser.Parse(commandText, commandText.Length);

            ICoreParseResult result = parseResult.Slice(0);

            Assert.Same(parseResult, result);
        }

        [Fact]
        public void Slice_WithTooMany_ReturnsDefaultResult()
        {
            CoreParser parser = new CoreParser();
            string commandText = "set header content-type application/json";
            ICoreParseResult parseResult = parser.Parse(commandText, commandText.Length);

            ICoreParseResult result = parseResult.Slice(100);

            Assert.Equal(0, result.CaretPositionWithinCommandText);
            Assert.Equal(0, result.CaretPositionWithinSelectedSection);
            Assert.Equal(string.Empty, result.CommandText);
            Assert.Single(result.Sections);
            Assert.Contains(string.Empty, result.Sections);
            Assert.Equal(0, result.SelectedSection);
            Assert.Single(result.SectionStartLookup);
            Assert.Equal(0, result.SectionStartLookup.Single().Key);
            Assert.Equal(0, result.SectionStartLookup.Single().Value);
        }

        [Theory]
        [InlineData("set header content-type application/json", 1, "header content-type application/json")]
        [InlineData("set header content-type application/json", 2, "content-type application/json")]
        [InlineData("set header content-type application/json", 3, "application/json")]
        public void Slice_WithVariousSliceLengths_CorrectCommandTextOutput(string commandText, int toRemove, string expectedCommandText)
        {
            CoreParser parser = new CoreParser();
            ICoreParseResult parseResult = parser.Parse(commandText, commandText.Length);

            ICoreParseResult result = parseResult.Slice(toRemove);

            Assert.Equal(expectedCommandText, result.CommandText);
        }

        [Fact]
        public void Slice_WithCaretInSlicedRegion_CaretIsZero()
        {
            CoreParser parser = new CoreParser();
            string commandText = "set header content-type application/json";
            ICoreParseResult parseResult = parser.Parse(commandText, 5);

            ICoreParseResult result = parseResult.Slice(2);

            Assert.Equal(0, result.CaretPositionWithinCommandText);
        }

        [Theory]
        [InlineData("set header content-type application/json", 1, 26, 2)]
        [InlineData("set header content-type application/json", 2, 26, 1)]
        [InlineData("set header content-type application/json", 3, 26, 0)]
        public void Slice_WithSelectedSection_SelectedSectionMoves(string commandText, int toRemove, int caretPosition, int expectedSelectedSection)
        {
            CoreParser parser = new CoreParser();
            ICoreParseResult parseResult = parser.Parse(commandText, caretPosition);

            ICoreParseResult result = parseResult.Slice(toRemove);

            Assert.Equal(expectedSelectedSection, result.SelectedSection);
        }

        [Theory]
        [InlineData("set base  https://localhost", 3)]
        [InlineData("run  c:\\script.txt", 2)]
        [InlineData("help  connect", 2)]
        [InlineData("set base ", 3)]
        [InlineData("", 1)]
        [InlineData(" set base https://localhost", 3)]
        public void Parse_WithDoubleSpace_RemovesEmptySections(string commandText, int expectedSectionCount)
        {
            CoreParser parser = new CoreParser();
            ICoreParseResult parseResult = parser.Parse(commandText, commandText.Length + 1);

            Assert.Equal(expectedSectionCount, parseResult.Sections.Count);
        }

        [Fact]
        public void Parse_WithQuotesButNoSpaces_DoesNotCombineSections()
        {
            // Arrange
            string commandText = "GET --response:headers \"file.txt\" --response:body \"file.txt\"";
            int caretPosition = commandText.Length + 1;
            CoreParser parser = new CoreParser();

            int expectedSectionCount = 5;

            // Act
            ICoreParseResult parseResult = parser.Parse(commandText, caretPosition);

            // Assert
            Assert.Equal(expectedSectionCount, parseResult.Sections.Count);
        }
    }
}
