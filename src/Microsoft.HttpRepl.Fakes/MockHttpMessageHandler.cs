// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.HttpRepl.Fakes
{
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _contentType;
        private readonly string _fileContents;
        private readonly string _header;
        private readonly bool _readFromFile;
        private readonly IDictionary<string, string> _urlsWithResponse;

        public MockHttpMessageHandler(IDictionary<string, string> urlsWithResponse,
            string header,
            bool readFromFile,
            string fileContents,
            string contentType)
        {
            _contentType = contentType;
            _fileContents = fileContents;
            _header = header;
            _readFromFile = readFromFile;
            _urlsWithResponse = urlsWithResponse;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string absoluteUri = request.RequestUri.AbsoluteUri;
            _urlsWithResponse.TryGetValue(absoluteUri, out string responseContent);

            HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            httpResponseMessage.RequestMessage = request;

            if (_readFromFile)
            {
                httpResponseMessage.Content = new MockHttpContent(_fileContents);
            }
            else if (!string.IsNullOrEmpty(_header))
            {
                httpResponseMessage.Headers.Add(_header, responseContent);
            }
            else
            {
                httpResponseMessage.Content = new MockHttpContent(responseContent);
            }

            if (!string.IsNullOrEmpty(_contentType) && httpResponseMessage.Content != null)
            {
                httpResponseMessage.Content.Headers.Add("Content-Type", _contentType);
            }

            return Task.FromResult(httpResponseMessage);
        }
    }
}
