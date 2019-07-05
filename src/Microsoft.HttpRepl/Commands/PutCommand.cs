// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.HttpRepl.FileSystem;
using Microsoft.HttpRepl.Preferences;

namespace Microsoft.HttpRepl.Commands
{
    public class PutCommand : BaseHttpCommand
    {
        public PutCommand(IFileSystem fileSystem, IPreferences preferences) : base(fileSystem, preferences) { }

        protected override string Verb => "put";

        protected override bool RequiresBody => true;
    }
}
