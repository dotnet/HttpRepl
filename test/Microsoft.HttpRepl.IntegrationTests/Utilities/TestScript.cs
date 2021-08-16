// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System;
using System.IO;

namespace Microsoft.HttpRepl.IntegrationTests.Utilities
{
    public class TestScript : IDisposable
    {
        public string FilePath { get; }

        public TestScript(string content)
        {
            FilePath = Path.GetTempFileName();
            File.WriteAllText(FilePath, content);
        }

        public void Dispose()
        {
            if(File.Exists(FilePath))
            {
                File.Delete(FilePath);
            }
        }
    }
}
