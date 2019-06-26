using System.Collections.Generic;
using System.IO;

namespace Microsoft.HttpRepl.FileSystem
{
    internal class RealFileSystem : IFileSystem
    {
        public bool Exists(string path)
        {
            return File.Exists(path);
        }

        public void WriteAllText(string path, string contents)
        {
            File.WriteAllText(path, contents);
        }

        public byte[] ReadAllBytes(string path)
        {
            return File.ReadAllBytes(path);
        }

        public string[] ReadAllLines(string path)
        {
            return File.ReadAllLines(path);
        }

        public void WriteAllLines(string path, IEnumerable<string> contents)
        {
            File.WriteAllLines(path, contents);
        }

        public void Delete(string path)
        {
            File.Delete(path);
        }

        public Stream Create(string path)
        {
            return File.Create(path);
        }

        public string GetTempFileName()
        {
            return Path.GetTempFileName();
        }
    }
}
