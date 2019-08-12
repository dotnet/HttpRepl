using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Microsoft.HttpRepl
{
    public class ApiDefinition
    {
        public Uri SourceEndpoint { get; set; }
        public IList<Uri> BaseAddresses { get; } = new List<Uri>();

        public IDirectoryStructure DirectoryStructure { get; set; }
    }
}
