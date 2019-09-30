// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.HttpRepl.Resources;

namespace Microsoft.HttpRepl.FileSystem
{
    internal class RealFileSystem : IFileSystem
    {
        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public void WriteAllTextToFile(string path, string contents)
        {
            VerifyDirectoryExists(path);
            File.WriteAllText(path, contents);
        }

        public byte[] ReadAllBytesFromFile(string path)
        {
            return File.ReadAllBytes(path);
        }

        public string[] ReadAllLinesFromFile(string path)
        {
            return File.ReadAllLines(path);
        }

        public void WriteAllLinesToFile(string path, IEnumerable<string> contents)
        {
            VerifyDirectoryExists(path);
            File.WriteAllLines(path, contents);
        }

        public void DeleteFile(string path)
        {
            File.Delete(path);
        }

        public string GetTempFileName(string fileExtension)
        {
            fileExtension = fileExtension ?? throw new ArgumentNullException(nameof(fileExtension));

            if (!fileExtension.StartsWith(".", StringComparison.Ordinal) || fileExtension.Length < 2)
            {
                throw new ArgumentException(string.Format(Strings.RealFileSystem_Error_InvalidExtension, nameof(fileExtension)), nameof(fileExtension));
            }

            string tempFileName = Path.Combine(Path.GetTempPath(), GetRandomFileName(fileExtension));

            return tempFileName;
        }

        private static string GetRandomFileName(string fileExtension)
        {
            // Start it with HttpRepl so we can easily find it if necessary for debugging, etc
            // Use a GUID to make it unique enough
            return "HttpRepl." + Guid.NewGuid().ToString() + fileExtension;
        }

        private void VerifyDirectoryExists(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));   
        }
    }
}
