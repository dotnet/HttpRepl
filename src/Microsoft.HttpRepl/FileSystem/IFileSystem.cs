// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
