// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System.Collections.Generic;

namespace Microsoft.HttpRepl
{
    public static class WellKnownHeaders
    {
        public static readonly string ContentType = "Content-Type";

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
    }
}
