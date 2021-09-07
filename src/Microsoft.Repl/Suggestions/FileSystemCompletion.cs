// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.Repl.Suggestions
{
    public static class FileSystemCompletion
    {
        public static IEnumerable<string> GetCompletions(string prefix)
        {
            prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));

            if (prefix.StartsWith("\"", StringComparison.Ordinal))
            {
                prefix = prefix.Substring(1);

                int lastQuote = prefix.LastIndexOf('\"');

                if (lastQuote > -1)
                {
                    prefix = prefix.Remove(lastQuote, 1);
                }

                while (prefix.EndsWith($"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
                {
                    prefix = prefix.Substring(0, prefix.Length - 1);
                }
            }

            int lastPathIndex = prefix.LastIndexOfAny(new[] { '\\', '/' });
            if (lastPathIndex < 0)
            {
                return null;
            }

            string dir = prefix.Substring(0, lastPathIndex + 1);

            if (dir.IndexOfAny(Path.GetInvalidPathChars()) > -1)
            {
                return null;
            }

            string partPrefix = prefix.Substring(lastPathIndex + 1);
            if (Directory.Exists(dir))
            {
                return Directory.EnumerateDirectories(dir).Where(x => Path.GetFileName(x).StartsWith(partPrefix, StringComparison.OrdinalIgnoreCase))
                    .Union(Directory.EnumerateFiles(dir).Where(x => Path.GetFileName(x).StartsWith(partPrefix, StringComparison.OrdinalIgnoreCase))).Select(x => x.IndexOf(' ') > -1 ? $"\"{x}\"" : x);
            }

            return null;
        }
    }
}
