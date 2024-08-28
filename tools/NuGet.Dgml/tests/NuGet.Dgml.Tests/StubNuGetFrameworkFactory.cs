using System;
using NuGet.Frameworks;

namespace NuGet
{
    internal class StubNuGetFrameworkFactory
    {
        internal NuGetFramework NET40() => NET("4.0");

        internal NuGetFramework NET45() => NET("4.5");

        private static NuGetFramework NET(string version) => NET(new Version(version));

        private static NuGetFramework NET(Version version) => new NuGetFramework(".NETFramework", version);
    }
}
