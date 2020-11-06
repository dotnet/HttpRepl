// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
