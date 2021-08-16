// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System;

namespace Microsoft.Repl.ConsoleHandling
{
    // The values for the non-bold colors come from the ANSI escape sequences,
    // specifically the SGR foreground codes, which can be referenced here:
    // https://en.wikipedia.org/wiki/ANSI_escape_code#Colors
    [Flags]
    public enum AllowedColors
    {
        None = 0x0,
        Black = 0x1E,
        BoldBlack = Bold | Black,
        Red = 0x1F,
        BoldRed = Bold | Red,
        Green = 0x20,
        BoldGreen = Bold | Green,
        Yellow = 0x21,
        BoldYellow = Bold | Yellow,
        Blue = 0x22,
        BoldBlue = Bold | Blue,
        Magenta = 0x23,
        BoldMagenta = Bold | Magenta,
        Cyan = 0x24,
        BoldCyan = Bold | Cyan,
        White = 0x25,
        BoldWhite = White | Bold,
        Bold = 0x100
    }
}
