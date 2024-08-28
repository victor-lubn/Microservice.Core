using NuGet.Packaging.Core;
using NuGet.Versioning;

namespace NuGet
{
    internal static class StubPackageDependencyFactory
    {
        internal static PackageDependency CreateExact(string id, string version)
        {
            var versionRange = CreateVersionSpec(version, version, true, true);
            return new PackageDependency(id, versionRange);
        }

        internal static PackageDependency Create(string id, string minVersion)
        {
            var versionRange = CreateVersionSpec(minVersion);
            return new PackageDependency(id, versionRange);
        }

        internal static PackageDependency Create(string id, string minVersion, string maxVersion)
        {
            var versionRange = CreateVersionSpec(minVersion, maxVersion);
            return new PackageDependency(id, versionRange);
        }

        internal static PackageDependency Create(string id, string minVersion, string maxVersion, bool isMinInclusive, bool isMaxInclusive)
        {
            var versionRange = CreateVersionSpec(minVersion, maxVersion, isMinInclusive, isMaxInclusive);
            return new PackageDependency(id, versionRange);
        }

        private static VersionRange CreateVersionSpec(string minVersion = null, string maxVersion = null, bool includeMinVersion = true, bool includeMaxVersion = false)
        {
            NuGetVersion minNuGetVersion = null;
            if (minVersion != null)
                minNuGetVersion = new NuGetVersion(minVersion);
            NuGetVersion maxNuGetVersion = null;
            if (maxVersion != null)
                maxNuGetVersion = new NuGetVersion(maxVersion);
            return new VersionRange(minNuGetVersion, includeMinVersion, maxNuGetVersion, includeMaxVersion);
        }
    }
}
