// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Repl.ConsoleHandling;
using Xunit;

namespace Microsoft.Repl.Tests.ConsoleHandling
{
    public class AnsiColorExtensionsTests
    {
        [Theory]
        [InlineData("BlackText", AllowedColors.BoldBlack, "\x1B[30m\x1B[1mBlackText\x1B[39m\x1B[39m")]
        [InlineData("RedText", AllowedColors.BoldRed, "\x1B[31m\x1B[1mRedText\x1B[39m\x1B[39m")]
        [InlineData("GreenText", AllowedColors.BoldGreen, "\x1B[32m\x1B[1mGreenText\x1B[39m\x1B[39m")]
        [InlineData("YellowText", AllowedColors.BoldYellow, "\x1B[33m\x1B[1mYellowText\x1B[39m\x1B[39m")]
        [InlineData("BlueText", AllowedColors.BoldBlue, "\x1B[34m\x1B[1mBlueText\x1B[39m\x1B[39m")]
        [InlineData("MagentaText", AllowedColors.BoldMagenta, "\x1B[35m\x1B[1mMagentaText\x1B[39m\x1B[39m")]
        [InlineData("CyanText", AllowedColors.BoldCyan, "\x1B[36m\x1B[1mCyanText\x1B[39m\x1B[39m")]
        [InlineData("WhiteText", AllowedColors.BoldWhite, "\x1B[37m\x1B[1mWhiteText\x1B[39m\x1B[39m")]
        public void SetColor_WithBold_ResponseIncludesColorAndBold(string text, AllowedColors color, string expected)
        {
            var result = text.SetColor(color);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("BlackText", AllowedColors.Black, "\x1B[30mBlackText\x1B[39m")]
        [InlineData("RedText", AllowedColors.Red, "\x1B[31mRedText\x1B[39m")]
        [InlineData("GreenText", AllowedColors.Green, "\x1B[32mGreenText\x1B[39m")]
        [InlineData("YellowText", AllowedColors.Yellow, "\x1B[33mYellowText\x1B[39m")]
        [InlineData("BlueText", AllowedColors.Blue, "\x1B[34mBlueText\x1B[39m")]
        [InlineData("MagentaText", AllowedColors.Magenta, "\x1B[35mMagentaText\x1B[39m")]
        [InlineData("CyanText", AllowedColors.Cyan, "\x1B[36mCyanText\x1B[39m")]
        [InlineData("WhiteText", AllowedColors.White, "\x1B[37mWhiteText\x1B[39m")]
        public void SetColor_WithoutBold_ResponseIncludesJustColor(string text, AllowedColors color, string expected)
        {
            var result = text.SetColor(color);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("None", AllowedColors.None, "None")]
        [InlineData("InvalidColor", (AllowedColors)0x50, "InvalidColor")]
        public void SetColor_NoneOrInvalidColor_ReturnsText(string text, AllowedColors color, string expected)
        {
            var result = text.SetColor(color);

            Assert.Equal(expected, result);
        }
    }
}
