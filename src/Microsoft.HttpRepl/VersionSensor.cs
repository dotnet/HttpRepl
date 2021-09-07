// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System;
using System.Reflection;

namespace Microsoft.HttpRepl
{
    internal class VersionSensor
    {
        private static readonly Lazy<BuildInfo> buildInfo = new Lazy<BuildInfo>(() =>
        {
            Assembly assembly = typeof(VersionSensor).GetTypeInfo().Assembly;

            BuildInfo buildInfo = new BuildInfo()
            {
                AssemblyInformationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                                                       .InformationalVersion,
                AssemblyVersion = assembly.GetName().Version
            };

            return buildInfo;
        });

        public static string AssemblyInformationalVersion => buildInfo.Value.AssemblyInformationalVersion;
        public static Version AssemblyVersion => buildInfo.Value.AssemblyVersion;

        private class BuildInfo
        {
            public string AssemblyInformationalVersion { get; internal set; }
            public Version AssemblyVersion { get; internal set; }
        }
    }
}
