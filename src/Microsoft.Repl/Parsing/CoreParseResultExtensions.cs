// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Repl.Parsing
{
    public static class CoreParseResultExtensions
    {
        public static bool ContainsExactly(this ICoreParseResult parseResult, int length, StringComparison stringComparison, params string[] sections)
        {
            parseResult = parseResult ?? throw new ArgumentNullException(nameof(parseResult));

            if (parseResult.Sections.Count != length || parseResult.Sections.Count < sections.Length)
            {
                return false;
            }

            return CompareSections(parseResult.Sections, sections, stringComparison);
        }

        public static bool ContainsExactly(this ICoreParseResult parseResult, int length, params string[] sections)
        {
            return ContainsExactly(parseResult, length, StringComparison.OrdinalIgnoreCase, sections);
        }

        public static bool ContainsExactly(this ICoreParseResult parseResult, params string[] sections)
        {
            return ContainsExactly(parseResult, sections.Length, sections);
        }

        public static bool ContainsAtLeast(this ICoreParseResult parseResult, int minimumLength, StringComparison stringComparison, params string[] sections)
        {
            parseResult = parseResult ?? throw new ArgumentNullException(nameof(parseResult));

            if (parseResult.Sections.Count < minimumLength || parseResult.Sections.Count < sections.Length)
            {
                return false;
            }

            return CompareSections(parseResult.Sections, sections, stringComparison);
        }

        public static bool ContainsAtLeast(this ICoreParseResult parseResult, int minimumLength, params string[] sections)
        {
            return ContainsAtLeast(parseResult, minimumLength, StringComparison.OrdinalIgnoreCase, sections);
        }

        public static bool ContainsAtLeast(this ICoreParseResult parseResult, params string[] sections)
        {
            return ContainsAtLeast(parseResult, minimumLength: sections.Length, sections);
        }

        private static bool CompareSections(IReadOnlyList<string> parseSections, string[] sections, StringComparison stringComparison)
        {
            for (int index = 0; index < sections.Length; index++)
            {
                if (!string.Equals(parseSections[index], sections[index], stringComparison))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
