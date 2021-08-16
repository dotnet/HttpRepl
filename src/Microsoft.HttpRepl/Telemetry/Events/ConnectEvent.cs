// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

namespace Microsoft.HttpRepl.Telemetry.Events
{
    internal class ConnectEvent : TelemetryEventBase
    {
        public ConnectEvent(bool baseSpecified, bool rootSpecified, bool openApiSpecified, bool openApiFound) : base(TelemetryEventNames.Connect)
        {
            SetProperty(TelemetryPropertyNames.Connect_BaseSpecified, baseSpecified);
            SetProperty(TelemetryPropertyNames.Connect_RootSpecified, rootSpecified);
            SetProperty(TelemetryPropertyNames.Connect_OpenApiSpecified, openApiSpecified);
            SetProperty(TelemetryPropertyNames.Connect_OpenApiFound, openApiFound);
        }
    }
}
