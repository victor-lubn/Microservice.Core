using System.Collections.Generic;
using System.Linq;
using NuGet.Packaging.Core;
using NuGet.Versioning;

namespace NuGet
{
    internal static class StubPackageFactory
    {
        internal static IEnumerable<PackageDependencyInfo> CreatePackages(string id, params string[] versions)
        {
            return versions.Select(version => CreatePackage(id, version));
        }

        internal static PackageDependencyInfo CreatePackage(string id, string version)
        {
            return CreatePackage(id, version, Enumerable.Empty<PackageDependency>());
        }

        internal static PackageDependencyInfo CreatePackage(string id, string version, params PackageDependency[] packageDependencies)
        {
            return CreatePackage(id, version, (IEnumerable<PackageDependency>)packageDependencies);
        }

        internal static PackageDependencyInfo CreatePackage(string id, string version, IEnumerable<PackageDependency> packageDependencies)
        {
            return new PackageDependencyInfo(id, new NuGetVersion(version), packageDependencies);
        }
    }
}
