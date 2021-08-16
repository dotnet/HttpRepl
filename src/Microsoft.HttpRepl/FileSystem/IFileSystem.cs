// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System.Collections.Generic;

namespace Microsoft.HttpRepl.FileSystem
{
    public interface IFileSystem
    {
        void DeleteFile(string path);
        bool FileExists(string path);
        string GetTempFileName(string fileExtension);
        byte[] ReadAllBytesFromFile(string path);
        string[] ReadAllLinesFromFile(string path);
        void WriteAllTextToFile(string path, string contents);
        void WriteAllLinesToFile(string path, IEnumerable<string> contents);
    }
}
