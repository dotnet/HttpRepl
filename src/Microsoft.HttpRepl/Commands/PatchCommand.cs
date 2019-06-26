// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.HttpRepl.Commands
{
    public class PatchCommand : BaseHttpCommand
    {
        public PatchCommand(IServiceProvider serviceProvider) : base(serviceProvider) { }

        protected override string Verb => "patch";

        protected override bool RequiresBody => true;
    }
}
