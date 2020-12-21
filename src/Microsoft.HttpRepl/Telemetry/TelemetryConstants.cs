// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.HttpRepl.Telemetry
{
    internal class TelemetryEventNames
    {
        public const string CommandExecuted = nameof(CommandExecuted);
        public const string Connect = nameof(Connect);
        public const string HttpCommand = nameof(HttpCommand);
        public const string Preference = nameof(Preference);
        public const string SetHeader = nameof(SetHeader);
        public const string SetQueryString = nameof(SetQueryString);
        public const string Started = nameof(Started);
        public const string WebApiF5Fix = nameof(WebApiF5Fix);
    }

    internal class TelemetryPropertyNames
    {
        public const string CommandExecuted_CommandName = "CommandName";
        public const string CommandExecuted_WasSuccessful = "WasSuccessful";

        public const string Connect_BaseSpecified = "BaseSpecified";
        public const string Connect_OpenApiFound = "OpenApiFound";
        public const string Connect_OpenApiSpecified = "OpenApiSpecified";
        public const string Connect_RootSpecified = "RootSpecified";

        public const string HttpCommand_HeaderSpecified = "HeaderSpecified";
        public const string HttpCommand_Method = "Method";
        public const string HttpCommand_NoBodySpecified = "NoBodySpecified";
        public const string HttpCommand_NoFormattingSpecified = "NoFormattingSpecified";
        public const string HttpCommand_PathSpecified = "PathSpecified";
        public const string HttpCommand_RequestBodyContentSpecified = "RequestBodyContentSpecified";
        public const string HttpCommand_RequestBodyFileSpecified = "RequestBodyFileSpecified";
        public const string HttpCommand_ResponseHeadersFileSpecified = "ResponseHeadersFileSpecified";
        public const string HttpCommand_ResponseBodyFileSpecified = "ResponseBodyFileSpecified";
        public const string HttpCommand_StreamingSpecified = "StreamingSpecified";

        public const string Preference_GetOrSet = "GetOrSet";
        public const string Preference_PreferenceName = "PreferenceName";

        public const string SetHeader_HeaderName = "HeaderName";
        public const string SetHeader_IsValueEmpty = "IsValueEmpty";

        public const string SetQueryString_Key= "QueryStringKey";
        public const string SetQueryString_IsValueEmpty = "IsValueEmpty";

        public const string Started_WithHelp = "WithHelp";
        public const string Started_WithOtherArgs = "WithOtherArgs";
        public const string Started_WithOutputRedirection = "WithOutputRedirection";
        public const string Started_WithRun = "WithRun";

        public const string WebApiF5Fix_SkippedByPreference = "SkippedByPreference";
    }
}
