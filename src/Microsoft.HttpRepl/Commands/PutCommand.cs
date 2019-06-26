// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.HttpRepl.FileSystem;

namespace Microsoft.HttpRepl.Commands
{
    public class PutCommand : BaseHttpCommand
    {
        public PutCommand(IFileSystem fileSystem) : base(fileSystem) { }

        protected override string Verb => "put";

        protected override bool RequiresBody => true;
    }
}
