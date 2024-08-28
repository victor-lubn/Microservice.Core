using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NuGet.Frameworks;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;

namespace NuGet.Dgml
{
    /// <summary>
    /// Provides static extension methods for <see cref="PackageIdentity"/>.
    /// </summary>
    public static class PackageIdentityExtensions
    {
        /// <summary>
        /// Visualizes the upgradeable dependencies of the specified package.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <param name="sourceRepository">The repository to resolve package dependencies.</param>
        /// <returns>The graph of the upgradeable dependencies of the specified package.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="package"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="sourceRepository"/> is <c>null</c>.</exception>
        /// <seealso cref="VisualizeUpgradeableDependenciesAsync(PackageIdentity, SourceRepository, NuGetFramework)"/>
        public static Task<DirectedGraph> VisualizeUpgradeableDependenciesAsync(this PackageIdentity package, SourceRepository sourceRepository)
            => VisualizeUpgradeableDependenciesAsync(package, sourceRepository, NuGetFramework.AnyFramework);

        /// <summary>
        /// Visualizes the upgradeable dependencies of the specified package.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <param name="sourceRepository">The repository to resolve package dependencies.</param>
        /// <param name="targetFramework">The framework to find compatible package dependencies.</param>
        /// <returns>The graph of the upgradeable dependencies of the specified package.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="package"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="sourceRepository"/> is <c>null</c>.</exception>
        /// <seealso cref="IEnumerablePackageIdentityExtensions.VisualizeUpgradeableDependenciesAsync(IEnumerable{PackageIdentity}, SourceRepository, NuGetFramework)"/>
        public static Task<DirectedGraph> VisualizeUpgradeableDependenciesAsync(
            this PackageIdentity package,
            SourceRepository sourceRepository,
            NuGetFramework targetFramework)
        {
            var packages = new[] { package, };
            return packages.VisualizeUpgradeableDependenciesAsync(sourceRepository, targetFramework);
        }
    }
}
