// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OpenApi.Models;

namespace Microsoft.HttpRepl.OpenApi
{
    internal class EndpointMetadata
    {
        public EndpointMetadata(string path)
        {
            Path = path;
        }

        public string Path { get; }

        public ICollection<RequestMetadata> AvailableRequests { get; } = new List<RequestMetadata>();
    }

    internal class RequestMetadata
    {
        public RequestMetadata(OperationType operation)
        {
            Operation = operation;
        }

        public OperationType Operation { get; }

        public ICollection<RequestContentMetadata> Content { get; } = new List<RequestContentMetadata>();

        public ICollection<OpenApiParameter> Parameters { get; } = new List<OpenApiParameter>();
    }

    internal class RequestContentMetadata
    {
        public RequestContentMetadata(string contentType, bool isRequired, OpenApiSchema bodySchema)
        {
            ContentType = contentType;
            IsRequired = isRequired;
            BodySchema = bodySchema;
        }

        public string ContentType { get; }

        public bool IsRequired { get; }

        public OpenApiSchema BodySchema { get; }
    }
}
