// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System.Collections.Generic;
using Microsoft.HttpRepl.Telemetry;

namespace Microsoft.HttpRepl.Fakes
{
    public class NullTelemetry : ITelemetry
    {
        private readonly IFirstTimeUseNoticeSentinel _firstTimeUseNoticeSentinel = new NullFirstTimeUseNoticeSentinel();

        public bool Enabled => false;

        public IFirstTimeUseNoticeSentinel FirstTimeUseNoticeSentinel => _firstTimeUseNoticeSentinel;

        public void TrackEvent(string eventName, IReadOnlyDictionary<string, string> properties, IReadOnlyDictionary<string, double> measurements)
        {

        }
    }

    public class NullFirstTimeUseNoticeSentinel : IFirstTimeUseNoticeSentinel
    {
        public void CreateIfNotExists() { }

        public bool Exists() => true;
    }
}
