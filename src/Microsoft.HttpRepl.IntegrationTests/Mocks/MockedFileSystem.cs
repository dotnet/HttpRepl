using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.HttpRepl.FileSystem;
using Microsoft.HttpRepl.IntegrationTests.Commands;

namespace Microsoft.HttpRepl.IntegrationTests.Mocks
{
    internal class MockedFileSystem : IFileSystem
    {
        private readonly Dictionary<string, MockedFile> _files = new Dictionary<string, MockedFile>()
        {
            // Setup the "files" that should exist in the file system for the various tests here.
            { $"{nameof(PostCommandTests)}-{nameof(PostCommandTests.ExecuteAsync_MultiPartRouteWithBodyFromFile_VerifyResponse)}.txt", new MockedFile("Test Post Body From File") },
            { $"{nameof(PutCommandTests)}-{nameof(PutCommandTests.ExecuteAsync_MultiPartRouteWithBodyFromFile_VerifyResponse)}.txt", new MockedFile("Test Put Body From File") },
            { $"{nameof(PatchCommandTests)}-{nameof(PatchCommandTests.ExecuteAsync_MultiPartRouteWithBodyFromFile_VerifyResponse)}.txt", new MockedFile("Test Patch Body From File") }
        };

        internal string GetFile(string path)
        {
            return _files[path].Content;
        }

        internal void SetFile(string path, string contents)
        {
            _files[path].Content = contents;
        }

        public Stream Create(string path)
        {
            _files[path] = new MockedFile("");
            return _files[path].Stream;
        }

        public void Delete(string path)
        {
            if (_files.ContainsKey(path))
            {
                _files.Remove(path);
            }
        }

        public bool Exists(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            return _files.ContainsKey(path);
        }

        public byte[] ReadAllBytes(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(path);
            }

            if (!Exists(path))
            {
                throw new FileNotFoundException();
            }

            return _files[path].Stream.ToArray();
        }

        public string[] ReadAllLines(string path)
        {
            string alltext = _files[path].Content;
            return alltext.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        }

        public void WriteAllLines(string path, IEnumerable<string> contents)
        {
            _files[path] = new MockedFile(string.Join(Environment.NewLine, contents));
        }

        public void WriteAllText(string path, string contents)
        {
            _files[path].Content = contents;
        }

        public string GetTempFileName()
        {
            string path = GetRandomFileName();
            _files[path].Content = "";
            return path;
        }

        private string GetRandomFileName()
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] bytes = new byte[9];
            rng.GetBytes(bytes);
            string path = Convert.ToBase64String(bytes);
            return path;
        }
    }

    internal class MockedFile
    {
        public MemoryStream Stream { get; private set; }

        public string Content
        {
            get
            {
                var bytes = Stream.ToArray();
                return Encoding.UTF8.GetString(bytes);
            }
            set
            {
                var bytes = Encoding.UTF8.GetBytes(value);
                Stream.SetLength(bytes.Length);
                Stream.Write(bytes, 0, bytes.Length);
            }
        }

        public MockedFile(string content)
        {
            Stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        }
    }
}
