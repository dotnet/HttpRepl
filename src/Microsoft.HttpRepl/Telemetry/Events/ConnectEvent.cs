// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.HttpRepl.Telemetry.Events
{
    internal class ConnectEvent : TelemetryEventBase
    {
        public ConnectEvent(bool baseSpecified, bool rootSpecified, bool openApiSpecified, bool openApiFound) : base("Connect")
        {
            SetProperty("BaseSpecified", baseSpecified);
            SetProperty("RootSpecified", rootSpecified);
            SetProperty("OpenApiSpecified", openApiSpecified);
            SetProperty("OpenApiFound", openApiFound);
        }
    }
}
