// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;

namespace Microsoft.HttpRepl.IntegrationTests
{
    public class BaseIntegrationTest
    {
        protected static string NormalizeOutput(string output, string baseUrl)
        {
            // The console implementation uses trailing whitespace when a new line's text is shorter than the previous
            // line.  For example (the trailing * represent spaces):
            // Line 1: (Disconnected)~ run C:\path\to\a\test\script\file.txt
            // Line 2: (Disconnected)~ set base http://localhost:12345******
            // This having this whitespace makes it harder to read/write test baselines, so here we'll trim each line
            string result = string.Join(Environment.NewLine, output.Split(Environment.NewLine).Select(l => l.TrimEnd()));

            // next, normalize the base URL from the test fixture
            if (!string.IsNullOrEmpty(baseUrl))
            {
                result = result.Replace(baseUrl, "[BaseUrl]");
            }

            return result;
        }
    }
}
