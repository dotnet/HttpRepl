// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
