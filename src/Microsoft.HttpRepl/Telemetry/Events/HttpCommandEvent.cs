// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.HttpRepl.Telemetry.Events
{
    internal class HttpCommandEvent : TelemetryEventBase
    {
        public HttpCommandEvent(string method, bool isPathSpecified, bool isHeaderSpecified, bool isResponseHeadersFileSpecified,
                                bool isResponseBodyFileSpecified, bool isNoFormattingSpecified, bool isStreamingSpecified,
                                bool isNoBodySpecified, bool isRequestBodyFileSpecified, bool isRequestBodyContentSpecified)
            : base(TelemetryEventNames.HttpCommand)
        {
            SetProperty(TelemetryPropertyNames.HttpCommand_Method, method);
            SetProperty(TelemetryPropertyNames.HttpCommand_PathSpecified, isPathSpecified);
            SetProperty(TelemetryPropertyNames.HttpCommand_HeaderSpecified, isHeaderSpecified);
            SetProperty(TelemetryPropertyNames.HttpCommand_ResponseHeadersFileSpecified, isResponseHeadersFileSpecified);
            SetProperty(TelemetryPropertyNames.HttpCommand_ResponseBodyFileSpecified, isResponseBodyFileSpecified);
            SetProperty(TelemetryPropertyNames.HttpCommand_NoFormattingSpecified, isNoFormattingSpecified);
            SetProperty(TelemetryPropertyNames.HttpCommand_StreamingSpecified, isStreamingSpecified);
            SetProperty(TelemetryPropertyNames.HttpCommand_NoBodySpecified, isNoBodySpecified);
            SetProperty(TelemetryPropertyNames.HttpCommand_RequestBodyFileSpecified, isRequestBodyFileSpecified);
            SetProperty(TelemetryPropertyNames.HttpCommand_RequestBodyContentSpecified, isRequestBodyContentSpecified);
        }
    }
}
