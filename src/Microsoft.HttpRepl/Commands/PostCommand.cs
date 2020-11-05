// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.HttpRepl.FileSystem;
using Microsoft.HttpRepl.Preferences;
using Microsoft.HttpRepl.Telemetry;

namespace Microsoft.HttpRepl.Commands
{
    public class PostCommand : BaseHttpCommand
    {
        public PostCommand(IFileSystem fileSystem, IPreferences preferences, ITelemetry telemetry) : base(fileSystem, preferences, telemetry) { }

        protected override string Verb => "post";

        protected override bool RequiresBody => true;
    }
}
