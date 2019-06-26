// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.HttpRepl.Commands
{
    public class PutCommand : BaseHttpCommand
    {
        public PutCommand(IServiceProvider serviceProvider) : base(serviceProvider) { }

        protected override string Verb => "put";

        protected override bool RequiresBody => true;
    }
}
