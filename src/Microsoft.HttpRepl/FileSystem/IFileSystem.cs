using System.Collections.Generic;
using System.IO;

namespace Microsoft.HttpRepl.FileSystem
{
    public interface IFileSystem
    {
        Stream CreateFile(string path);
        void DeleteFile(string path);
        bool FileExists(string path);
        string GetTempFileName();
        byte[] ReadAllBytesFromFile(string path);
        string[] ReadAllLinesFromFile(string path);
        void WriteAllTextToFile(string path, string contents);
        void WriteAllLinesToFile(string path, IEnumerable<string> contents);
    }
}
