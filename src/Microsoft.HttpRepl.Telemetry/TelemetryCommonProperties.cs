// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using RuntimeEnvironment = Microsoft.DotNet.PlatformAbstractions.RuntimeEnvironment;

namespace Microsoft.HttpRepl.Telemetry
{
    public class TelemetryCommonProperties
    {
        private readonly string _productVersion;

        public TelemetryCommonProperties(
            string productVersion,
            Func<string, string> hasher = null,
            Func<string> getMACAddress = null,
            IDockerContainerDetector dockerContainerDetector = null,
            IUserLevelCacheWriter userLevelCacheWriter = null)
        {
            _productVersion = productVersion;
            _hasher = hasher ?? Sha256Hasher.Hash;
            _getMACAddress = getMACAddress ?? MacAddressGetter.GetMacAddress;
            _dockerContainerDetector = dockerContainerDetector ?? new DockerContainerDetectorForTelemetry();
            _userLevelCacheWriter = userLevelCacheWriter ?? new UserLevelCacheWriter(productVersion);
        }

        private readonly IUserLevelCacheWriter _userLevelCacheWriter;
        private readonly IDockerContainerDetector _dockerContainerDetector;
        private readonly Func<string, string> _hasher;
        private readonly Func<string> _getMACAddress;
        private const string OSVersion = "OS Version";
        private const string OSPlatform = "OS Platform";
        private const string RuntimeId = "Runtime Id";
        private const string ProductVersion = "Product Version";
        private const string DockerContainer = "Docker Container";
        private const string MachineId = "Machine ID";
        private const string KernelVersion = "Kernel Version";

        private const string MachineIdCacheKey = "MachineId";
        private const string IsDockerContainerCacheKey = "IsDockerContainer";

        public Dictionary<string, string> GetTelemetryCommonProperties()
        {
            return new Dictionary<string, string>
            {
                {OSVersion, RuntimeEnvironment.OperatingSystemVersion},
                {OSPlatform, RuntimeEnvironment.OperatingSystemPlatform.ToString()},
                {RuntimeId, RuntimeEnvironment.GetRuntimeIdentifier()},
                {ProductVersion, _productVersion},
                {DockerContainer, IsDockerContainer()},
                {MachineId, GetMachineId()},
                {KernelVersion, GetKernelVersion()}
            };
        }

        private string GetMachineId()
        {
            return _userLevelCacheWriter.RunWithCache(MachineIdCacheKey, () =>
            {
                var macAddress = _getMACAddress();
                if (macAddress != null)
                {
                    return _hasher(macAddress);
                }
                else
                {
                    return Guid.NewGuid().ToString();
                }
            });
        }

        private string IsDockerContainer()
        {
            return _userLevelCacheWriter.RunWithCache(IsDockerContainerCacheKey, () =>
            {
                return _dockerContainerDetector.IsDockerContainer().ToString("G");
            });
        }

        /// <summary>
        /// Returns a string identifying the OS kernel.
        /// For Unix this currently comes from "uname -srv".
        /// For Windows this currently comes from RtlGetVersion().
        /// </summary>
        private static string GetKernelVersion()
        {
            return RuntimeInformation.OSDescription;
        }
    }
}
