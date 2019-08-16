// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;

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

        public string GetTempFileName()
        {
            return Path.GetTempFileName();
        }

        private void VerifyDirectoryExists(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));   
        }
    }
}
