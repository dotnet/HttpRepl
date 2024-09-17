// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using Microsoft.HttpRepl.FileSystem;
using Microsoft.HttpRepl.Preferences;

namespace Microsoft.HttpRepl.Commands
{
    public class GetCommand : BaseHttpCommand
    {
        public GetCommand(IFileSystem fileSystem, IPreferences preferences) : base(fileSystem, preferences) { }

        protected override string Verb => "get";

        protected override bool RequiresBody => false;
    }
}
