// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Resources;

namespace Microsoft.HttpRepl.IntegrationTests.Mocks
{
    internal class MockUriLauncher : IUriLauncher
    {
        private bool _uriLaunchSuccessful;

        public MockUriLauncher(bool uriLaunchSuccessful)
        {
            _uriLaunchSuccessful = uriLaunchSuccessful;
        }

        public Task LaunchUriAsync(Uri uri)
        {
            if (_uriLaunchSuccessful)
            {
                return Task.CompletedTask;
            }
            else
            {
                string uriLaunchErrorMessage = string.Format(Strings.UICommand_UnableToLaunchUriError, uri);

                return Task.FromException(new Exception(uriLaunchErrorMessage));
            }
        }
    }
}
