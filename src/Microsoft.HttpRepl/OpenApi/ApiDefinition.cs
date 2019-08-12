using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Microsoft.HttpRepl.OpenApi
{
    public class ApiDefinition
    {
        public Uri SourceEndpoint { get; set; }
        public IList<Server> BaseAddresses { get; } = new List<Server>();
        public IDirectoryStructure DirectoryStructure { get; set; }

        public class Server
        {
            public Uri Url { get; set; }
            public string Description { get; set; }
        }
    }
}
