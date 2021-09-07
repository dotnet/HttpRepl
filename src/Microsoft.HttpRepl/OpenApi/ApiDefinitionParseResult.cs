// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

#nullable enable

using System;
using System.Collections.Generic;

namespace Microsoft.HttpRepl.OpenApi
{
    public class ApiDefinitionParseResult
    {
        public static ApiDefinitionParseResult Failed { get; } = new ApiDefinitionParseResult(false, null, null);

        public bool Success { get; private set; }
        public IReadOnlyCollection<string> ValidationMessages { get; private set; }
        public ApiDefinition? ApiDefinition { get; private set; }

        public ApiDefinitionParseResult(bool success, ApiDefinition? apiDefinition, IEnumerable<string>? validationMessages)
        {
            Success = success;
            ApiDefinition = apiDefinition;
            ValidationMessages = validationMessages is null ? Array.Empty<string>() : new List<string>(validationMessages);
        }
    }
}
