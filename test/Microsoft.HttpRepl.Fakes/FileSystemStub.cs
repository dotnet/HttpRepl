// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System.Collections.Generic;
using Microsoft.HttpRepl.FileSystem;

namespace Microsoft.HttpRepl.Fakes
{
    public class FileSystemStub : IFileSystem
    {
        public void DeleteFile(string path)
        {

        }

        public bool FileExists(string path)
        {
            return default;
        }

        public string GetTempFileName(string fileExtension)
        {
            return default;
        }

        public byte[] ReadAllBytesFromFile(string path)
        {
            return default;
        }

        public string[] ReadAllLinesFromFile(string path)
        {
            return default;
        }

        public void WriteAllLinesToFile(string path, IEnumerable<string> contents)
        {

        }

        public void WriteAllTextToFile(string path, string contents)
        {

        }
    }
}
