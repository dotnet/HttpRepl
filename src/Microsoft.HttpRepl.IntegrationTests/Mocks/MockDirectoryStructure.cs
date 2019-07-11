// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.HttpRepl.IntegrationTests.Mocks
{
    internal class MockDirectoryStructure : IDirectoryStructure
    {
        private string _body;
        private string _contentType;
        private string _method;

        public MockDirectoryStructure(string method, string contentType, string body)
        {
            _body = body;
            _contentType = contentType;
            _method = method;
        }

        public IEnumerable<string> DirectoryNames => throw new NotImplementedException();

        public IDirectoryStructure Parent => throw new NotImplementedException();

        public IDirectoryStructure GetChildDirectory(string name)
        {
            throw new NotImplementedException();
        }

        public IRequestInfo RequestInfo
        {
            get
            {
                RequestInfo requestInfo = new RequestInfo();
                requestInfo.SetRequestBody(_method, _contentType, _body);

                return requestInfo;
            }
        }
    }
}
