// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

namespace Microsoft.HttpRepl.Commands
{
    public class Formatter
    {
        private int _prefix;
        private int _maxDepth;

        public void RegisterEntry(int prefixLength, int depth)
        {
            if (depth > _maxDepth)
            {
                _maxDepth = depth;
            }

            if (prefixLength > _prefix)
            {
                _prefix = prefixLength;
            }
        }

        public string Format(string prefix, string entry, int level)
        {
            string indent = "".PadRight(level * 4);
            return (indent + prefix).PadRight(_prefix + 3 + _maxDepth * 4) + entry;
        }
    }
}
