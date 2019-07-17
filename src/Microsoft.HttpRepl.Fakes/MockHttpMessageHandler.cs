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
        IDictionary<string, string> _urlsWithResponse;

        public MockHttpMessageHandler(IDictionary<string, string> urlsWithResponse)
        {
            _urlsWithResponse = urlsWithResponse;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string absoluteUri = request.RequestUri.AbsoluteUri;
            _urlsWithResponse.TryGetValue(absoluteUri, out string responseContent);

            HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            httpResponseMessage.Content = new MockHttpContent(responseContent);

            return Task.FromResult(httpResponseMessage);
        }
    }
}
