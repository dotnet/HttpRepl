// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.HttpRepl.Fakes
{
    public class MockHttpContent : HttpContent
    {
        public string Content { get; }

        public MockHttpContent(string content)
        {
            Content = content ?? string.Empty;
        }

        protected async override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            byte[] byteArray = Encoding.ASCII.GetBytes(Content);
            await stream.WriteAsync(byteArray, 0, byteArray.Length);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = Content.Length;
            return true;
        }
    }
}
