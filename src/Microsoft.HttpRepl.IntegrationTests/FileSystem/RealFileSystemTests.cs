// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.HttpRepl.FileSystem;
using Xunit;

namespace Microsoft.HttpRepl.IntegrationTests.FileSystem
{
    public class RealFileSystemTests
    {
        [Fact]
        public void GetTempFileName_WithoutExtension_ReturnsTmpFile()
        {
            RealFileSystem realFileSystem = new RealFileSystem();

            string fileName = realFileSystem.GetTempFileName();

            Assert.NotNull(fileName);
            Assert.EndsWith(".TMP", fileName, StringComparison.OrdinalIgnoreCase);

            // Since the default case creates the file, we need to delete it.
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }

        [Theory]
        [InlineData(".json")]
        [InlineData(".xml")]
        public void GetTempFileName_WithExtension_ReturnsFileNameWithExtension(string extension)
        {
            RealFileSystem realFileSystem = new RealFileSystem();

            string fullName = realFileSystem.GetTempFileName(extension);

            Assert.NotNull(fullName);
            Assert.EndsWith(extension, fullName, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void GetTempFileName_WithoutExtension_ReturnsFileInTempPath()
        {
            RealFileSystem realFileSystem = new RealFileSystem();
            string expectedPath = Path.GetTempPath();

            string actualPath = realFileSystem.GetTempFileName();

            Assert.StartsWith(expectedPath, actualPath, StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData(".json")]
        [InlineData(".xml")]
        public void GetTempFileName_WithExtension_ReturnsFileInTempPath(string extension)
        {
            RealFileSystem realFileSystem = new RealFileSystem();
            string expectedPath = Path.GetTempPath();

            string actualPath = realFileSystem.GetTempFileName(extension);
            
            Assert.StartsWith(expectedPath, actualPath, StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData(".json")]
        [InlineData(".xml")]
        public void GetTempFileName_WithExtension_ReturnsFileThatStartsWithHttpRepl(string extension)
        {
            RealFileSystem realFileSystem = new RealFileSystem();
            string expectedStart = "HttpRepl.";

            string fullName = realFileSystem.GetTempFileName(extension);
            string actualFileName = Path.GetFileName(fullName);

            Assert.StartsWith(expectedStart, actualFileName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
