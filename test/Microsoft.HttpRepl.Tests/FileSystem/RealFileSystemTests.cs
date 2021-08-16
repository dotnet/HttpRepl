// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System;
using System.IO;
using Microsoft.HttpRepl.FileSystem;
using Xunit;

namespace Microsoft.HttpRepl.Tests.FileSystem
{
    public class RealFileSystemTests
    {
        [Theory]
        [InlineData(".json")]
        [InlineData(".xml")]
        [InlineData(".tmp")]
        [InlineData(".a")]
        public void GetTempFileName_WithValidInput_ReturnsFileNameWithExtension(string extension)
        {
            RealFileSystem realFileSystem = new RealFileSystem();

            string fullName = realFileSystem.GetTempFileName(extension);

            Assert.NotNull(fullName);
            Assert.EndsWith(extension, fullName, StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData(".json")]
        [InlineData(".xml")]
        [InlineData(".tmp")]
        [InlineData(".a")]
        public void GetTempFileName_WithValidInput_ReturnsFileInTempPath(string extension)
        {
            RealFileSystem realFileSystem = new RealFileSystem();
            string expectedPath = Path.GetTempPath();

            string actualPath = realFileSystem.GetTempFileName(extension);

            Assert.StartsWith(expectedPath, actualPath, StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData(".json")]
        [InlineData(".xml")]
        [InlineData(".tmp")]
        [InlineData(".a")]
        public void GetTempFileName_WithValidInput_ReturnsFileThatStartsWithHttpRepl(string extension)
        {
            RealFileSystem realFileSystem = new RealFileSystem();
            string expectedStart = "HttpRepl.";

            string fullName = realFileSystem.GetTempFileName(extension);
            string actualFileName = Path.GetFileName(fullName);

            Assert.StartsWith(expectedStart, actualFileName, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void GetTempFileName_WithNullExtension_ThrowsArgumentNullException()
        {
            RealFileSystem realFileSystem = new RealFileSystem();

            Assert.Throws<ArgumentNullException>(() => realFileSystem.GetTempFileName(null));
        }

        [Theory]
        [InlineData("")]
        [InlineData(".")]
        [InlineData(",a")]
        public void GetTEmpFileName_WithInvalidInput_ThrowsArgumentException(string extension)
        {
            RealFileSystem realFileSystem = new RealFileSystem();

            Assert.Throws<ArgumentException>(() => realFileSystem.GetTempFileName(extension));
        }
    }
}
