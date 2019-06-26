// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.HttpRepl.FileSystem;

namespace Microsoft.HttpRepl.Commands
{
    public class PatchCommand : BaseHttpCommand
    {
        public PatchCommand(IFileSystem fileSystem) : base(fileSystem) { }

        protected override string Verb => "patch";

        protected override bool RequiresBody => true;
    }
}
