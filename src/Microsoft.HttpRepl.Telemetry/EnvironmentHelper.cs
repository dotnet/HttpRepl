// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.HttpRepl.Telemetry
{
    internal static class EnvironmentHelper
    {
        public static bool GetEnvironmentVariableAsBool(string name, bool defaultValue = false)
        {
            var str = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrEmpty(str))
            {
                return defaultValue;
            }

            switch (str.ToUpperInvariant())
            {
                case "TRUE":
                case "1":
                case "YES":
                    return true;
                case "FALSE":
                case "0":
                case "NO":
                    return false;
                default:
                    return defaultValue;
            }
        }
    }
}
