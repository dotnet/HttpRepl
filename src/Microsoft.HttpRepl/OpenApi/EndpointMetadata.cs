// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

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
