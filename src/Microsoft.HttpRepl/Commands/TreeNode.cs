// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;

namespace Microsoft.HttpRepl.Commands
{
    public class TreeNode
    {
        private readonly int _depth;
        private readonly Formatter _formatter;
        private readonly string _prefix;
        private readonly string _entry;
        private readonly List<TreeNode> _children = new List<TreeNode>();

        public IReadOnlyList<TreeNode> Children => _children;

        public TreeNode(Formatter formatter, string prefix, string entry)
            : this(formatter, prefix, entry, 0)
        {
        }

        private TreeNode(Formatter formatter, string prefix, string entry, int depth)
        {
            _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            _prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
            formatter.RegisterEntry(prefix.Length, depth);
            _entry = entry;
            _depth = depth;
        }

        public TreeNode AddChild(string prefix, string entry)
        {
            TreeNode child = new TreeNode(_formatter, prefix, entry, _depth + 1);
            _children.Add(child);
            return child;
        }

        public override string ToString()
        {
            string self = _formatter.Format(_prefix, _entry, _depth);

            if (_children.Count == 0)
            {
                return self;
            }

            return self + Environment.NewLine + string.Join(Environment.NewLine, _children);
        }
    }
}
