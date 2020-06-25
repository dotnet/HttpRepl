// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Resources;

namespace Microsoft.HttpRepl
{
    public static class WellKnownHeaders
    {
        public static readonly IEnumerable<string> CommonHeaders = new[]
        {
            "A-IM",
            "Accept",
            "Accept-Charset",
            "Accept-Encoding",
            "Accept-Language",
            "Accept-Datetime",
            "Access-Control-Request-Method",
            "Access-Control-Request-Headers",
            "Allow",
            "Authorization",
            "Cache-Control",
            "Connection",
            "Content-Disposition",
            "Content-Encoding",
            "Content-Language",
            "Content-Length",
            "Content-Location",
            "Content-MD5",
            "Content-Range",
            ContentType,
            "Cookie",
            "Date",
            "Expect",
            "Expires",
            "Forwarded",
            "From",
            "Host",
            "If-Match",
            "If-Modified-Since",
            "If-None-Match",
            "If-Range",
            "If-Unmodified-Since",
            "Last-Modified",
            "Max-Forwards",
            "Origin",
            "Pragma",
            "Proxy-Authentication",
            "Range",
            "Referer",
            "TE",
            "User-Agent",
            "Upgrade",
            "Via",
            "Warning",
            //Non-standard
            "Upgrade-Insecure-Requests",
            "X-Requested-With",
            "DNT",
            "X-Forwarded-For",
            "X-Forwarded-Host",
            "X-Forwarded-Proto",
            "Front-End-Https",
            "X-Http-Method-Override",
            "X-ATT-DeviceId",
            "X-Wap-Profile",
            "Proxy-Connection",
            "X-UIDH",
            "X-Csrf-Token",
            "X-Request-ID",
            "X-Correlation-ID"
        };

        public static readonly IEnumerable<string> ContentHeaders = new []
        {
            "Allow",
            "Content-Disposition",
            "Content-Encoding",
            "Content-Language",
            "Content-Length",
            "Content-Location",
            "Content-MD5",
            "Content-Range",
            ContentType,
            "Expires",

            "Last-Modified",
        };

        public static readonly string ContentType = "Content-Type";
    }
}
