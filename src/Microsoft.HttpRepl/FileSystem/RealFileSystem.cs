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
            File.WriteAllLines(path, contents);
        }

        public void DeleteFile(string path)
        {
            File.Delete(path);
        }

        public Stream CreateFile(string path)
        {
            return File.Create(path);
        }

        public string GetTempFileName()
        {
            return Path.GetTempFileName();
        }
    }
}
