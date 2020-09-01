// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
