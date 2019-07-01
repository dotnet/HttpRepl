using System.Collections.Generic;

namespace Microsoft.HttpRepl.FileSystem
{
    public interface IFileSystem
    {
        void DeleteFile(string path);
        bool FileExists(string path);
        string GetTempFileName();
        byte[] ReadAllBytesFromFile(string path);
        string[] ReadAllLinesFromFile(string path);
        void WriteAllTextToFile(string path, string contents);
        void WriteAllLinesToFile(string path, IEnumerable<string> contents);
    }
}
