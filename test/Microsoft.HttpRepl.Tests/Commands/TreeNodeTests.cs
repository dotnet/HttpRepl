// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.HttpRepl.Commands;
using Xunit;

namespace Microsoft.HttpRepl.Tests.Commands
{
    public class TreeNodeTests
    {
        [Fact]
        public void Constructor_WithNullFormatter_ThrowsArgumentNullException()
        {
            Formatter formatter = null;
            string prefix = "";
            string entry = "";

            Assert.Throws<ArgumentNullException>(() => new TreeNode(formatter, prefix, entry));
        }

        [Fact]
        public void Constructor_WithNullPrefix_ThrowsArgumentNullException()
        {
            Formatter formatter = new Formatter();
            string prefix = null;
            string entry = "";

            Assert.Throws<ArgumentNullException>(() => new TreeNode(formatter, prefix, entry));
        }

        [Fact]
        public void AddChild_WithNullPrefix_ThrowsArgumentNullException()
        {
            Formatter parentFormatter = new Formatter();
            string parentPrefix = "";
            string parentEntry = "";
            TreeNode treeNode = new TreeNode(parentFormatter, parentPrefix, parentEntry);
            string childPrefix = null;
            string childEntry = "";

            Assert.Throws<ArgumentNullException>(() => treeNode.AddChild(childPrefix, childEntry));
        }

        [Fact]
        public void AddChild_Valid_ReturnedChildAddedToChildren()
        {
            Formatter parentFormatter = new Formatter();
            string parentPrefix = "";
            string parentEntry = "";
            TreeNode treeNode = new TreeNode(parentFormatter, parentPrefix, parentEntry);
            string childPrefix = "";
            string childEntry = "";

            TreeNode childTreeNode = treeNode.AddChild(childPrefix, childEntry);

            Assert.Contains(childTreeNode, treeNode.Children);
        }
    }
}
