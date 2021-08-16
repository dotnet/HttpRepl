// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using Microsoft.Repl.ConsoleHandling;

namespace Microsoft.HttpRepl.Preferences
{
    public interface IJsonConfig
    {
        int IndentSize { get; }

        AllowedColors DefaultColor { get; }

        AllowedColors ArrayBraceColor { get; }

        AllowedColors ObjectBraceColor { get; }

        AllowedColors CommaColor { get; }

        AllowedColors NameColor { get; }

        AllowedColors NameSeparatorColor { get; }

        AllowedColors BoolColor { get; }

        AllowedColors NumericColor { get; }

        AllowedColors StringColor { get; }

        AllowedColors NullColor { get; }
    }
}
