// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.HttpRepl
{
    public static class DirectoryStructureExtensions
    {
        public static IEnumerable<string> GetDirectoryListingAtPath(this IDirectoryStructure structure, string path)
        {
            structure = structure ?? throw new ArgumentNullException(nameof(structure));
            return structure.TraverseTo(path).DirectoryNames;
        }

        public static IDirectoryStructure TraverseTo(this IDirectoryStructure structure, string path)
        {
            structure = structure ?? throw new ArgumentNullException(nameof(structure));
            path = path ?? throw new ArgumentNullException(nameof(path));

            string[] parts = path.Replace('\\', '/').Split('/');
            return structure.TraverseTo(parts);
        }

        public static IDirectoryStructure TraverseTo(this IDirectoryStructure structure, IEnumerable<string> pathParts)
        {
            structure = structure ?? throw new ArgumentNullException(nameof(structure));

            IDirectoryStructure s = structure;
            
            if (pathParts is null)
            {
                return s;
            }

            IReadOnlyList<string> parts = pathParts.ToList();
            if (parts.Count == 0)
            {
                return s;
            }

            if (parts[0].Length == 0 && parts.Count > 1)
            {
                while (s.Parent != null)
                {
                    s = s.Parent;
                }
            }

            foreach (string part in parts)
            {
                if (part == ".")
                {
                    continue;
                }

                if (part == "..")
                {
                    s = s?.Parent ?? s;
                }
                else if (!string.IsNullOrEmpty(part) && s is object)
                {
                    s = s.GetChildDirectory(part);
                }
            }

            return s;
        }
    }
}
