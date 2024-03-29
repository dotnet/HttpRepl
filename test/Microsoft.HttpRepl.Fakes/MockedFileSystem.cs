// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.HttpRepl.FileSystem;

namespace Microsoft.HttpRepl.Fakes
{
    public class MockedFileSystem : IFileSystem
    {
        private readonly Dictionary<string, string> _files = new Dictionary<string, string>();

        public MockedFileSystem AddFile(string path, string contents)
        {
            _files[path] = contents;
            return this;
        }

        public string ReadFile(string path)
        {
            return _files[path];
        }

        public void DeleteFile(string path)
        {
            if (_files.ContainsKey(path))
            {
                _files.Remove(path);
            }
        }

        public bool FileExists(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            return _files.ContainsKey(path);
        }

        public byte[] ReadAllBytesFromFile(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(path);
            }

            if (!FileExists(path))
            {
                throw new FileNotFoundException();
            }

            return Encoding.UTF8.GetBytes(_files[path]);
        }

        public string[] ReadAllLinesFromFile(string path)
        {
            string alltext = _files[path];
            return alltext.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        }

        public void WriteAllLinesToFile(string path, IEnumerable<string> contents)
        {
            _files[path] = string.Join(Environment.NewLine, contents);
        }

        public void WriteAllTextToFile(string path, string contents)
        {
            _files[path] = contents;
        }

        public string GetTempFileName(string fileExtension)
        {
            string path = GetRandomFileName();
            _files[path] = "";
            return path;
        }

        private string GetRandomFileName()
        {
            byte[] bytes = RandomNumberGenerator.GetBytes(9);
            string path = Convert.ToBase64String(bytes);
            return path;
        }
    }
}
