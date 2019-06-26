using System.Collections.Generic;
using System.IO;

namespace Microsoft.HttpRepl.FileSystem
{
    public interface IFileSystem
    {
        Stream Create(string path);
        void Delete(string path);
        bool Exists(string path);
        string GetTempFileName();
        byte[] ReadAllBytes(string path);
        string[] ReadAllLines(string path);
        void WriteAllText(string path, string contents);
        void WriteAllLines(string path, IEnumerable<string> contents);
    }
}
