// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

namespace Microsoft.HttpRepl.Telemetry
{
    internal class TelemetryEventNames
    {
        public const string CommandExecuted = nameof(CommandExecuted);
        public const string Connect = nameof(Connect);
        public const string HttpCommand = nameof(HttpCommand);
        public const string Preference = nameof(Preference);
        public const string SetHeader = nameof(SetHeader);
        public const string AddQueryParam = nameof(AddQueryParam);
        public const string ClearQueryParam = nameof(ClearQueryParam);
        public const string Started = nameof(Started);
        public const string WebApiF5Fix = nameof(WebApiF5Fix);
    }

    internal class TelemetryPropertyNames
    {
        public const string CommandExecuted_CommandName = "CommandName";
        public const string CommandExecuted_WasSuccessful = "WasSuccessful";

        public const string ClearQueryParam_Key = "QueryParamKey";
        public const string ClearQueryParam_IsValueEmpty = "IsValueEmpty";

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

        public const string AddQueryParam_Key = "QueryParamKey";
        public const string AddQueryParam_IsValueEmpty = "IsValueEmpty";

        public const string Started_WithHelp = "WithHelp";
        public const string Started_WithOtherArgs = "WithOtherArgs";
        public const string Started_WithOutputRedirection = "WithOutputRedirection";
        public const string Started_WithRun = "WithRun";

        public const string WebApiF5Fix_SkippedByPreference = "SkippedByPreference";
    }
}
