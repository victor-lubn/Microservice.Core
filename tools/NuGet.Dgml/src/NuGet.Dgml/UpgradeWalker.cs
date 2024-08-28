using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Frameworks;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;

namespace NuGet
{
    /// <summary>
    /// Walks the dependencies of a package and identifies upgradeable dependencies.
    /// </summary>
    public class UpgradeWalker
    {
        private readonly NuGetFramework _targetFramework;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpgradeWalker"/> class.
        /// </summary>
        /// <param name="repository">The repository to resolve package dependencies.</param>
        /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <c>null</c>.</exception>
        public UpgradeWalker(SourceRepository repository)
            : this(repository, NuGetFramework.AnyFramework)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpgradeWalker"/> class.
        /// </summary>
        /// <param name="repository">The repository to resolve package dependencies.</param>
        /// <param name="targetFramework">The framework to find compatible package dependencies.</param>
        /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <c>null</c>.</exception>
        public UpgradeWalker(SourceRepository repository, NuGetFramework targetFramework)
        {
            Repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _targetFramework = targetFramework;
        }

        /// <summary>
        /// Gets the repository to resolve package dependencies.
        /// </summary>
        public SourceRepository Repository { get; }

        /// <summary>
        /// Gets the dependency upgrades of the specified package.
        /// </summary>
        /// <param name="package">The package to identify upgrades.</param>
        /// <param name="logger">The logger for actions performed on the repository.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The package dependency upgrades of the specified package.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="package"/> is <c>null</c>.</exception>
        public Task<IEnumerable<PackageUpgrade>> GetPackageUpgradesAsync(PackageIdentity package, ILogger logger = null, CancellationToken? cancellationToken = null)
        {
            if (package == null)
                throw new ArgumentNullException(nameof(package));

            var internalCancellationToken = cancellationToken ?? CancellationToken.None;
            logger = logger ?? NullLogger.Instance;
            return IdentifyUpgradesAsync(package, logger, internalCancellationToken);
        }

        private async Task<IEnumerable<PackageUpgrade>> IdentifyUpgradesAsync(PackageIdentity package, ILogger logger, CancellationToken cancellationToken)
        {
            IList<PackageUpgrade> upgrades = new List<PackageUpgrade>();

            var packageDependencyInfo = await GetDependencies(package, logger, cancellationToken).ConfigureAwait(false);

            foreach (var dependency in packageDependencyInfo.Dependencies)
            {
                var recentDependencyPackage = await FindPackage(dependency.Id, logger, cancellationToken).ConfigureAwait(false);

                var upgradeType = DetectUpgradeAction(dependency, recentDependencyPackage);

                upgrades.Add(new PackageUpgrade(dependency, upgradeType, recentDependencyPackage));
            }

            return upgrades;
        }

        private async Task<SourcePackageDependencyInfo> GetDependencies(PackageIdentity package, ILogger logger, CancellationToken cancellationToken)
        {
            var dependencyInfo = await Repository.GetResourceAsync<DependencyInfoResource>(cancellationToken).ConfigureAwait(false);
            using (var cache = new SourceCacheContext())
            {
                return await dependencyInfo.ResolvePackage(package, _targetFramework, cache, logger, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task<PackageIdentity> FindPackage(string packageId, ILogger logger, CancellationToken cancellationToken)
        {
            var packageSearch = await Repository.GetResourceAsync<PackageSearchResource>(cancellationToken).ConfigureAwait(false);
            using (var cache = new SourceCacheContext())
            {
                var searchResult = await packageSearch.SearchAsync(packageId, new SearchFilter(false), 0, 1, logger, cancellationToken).ConfigureAwait(false);
                return searchResult.FirstOrDefault()?.Identity;
            }
        }

        private static PackageUpgradeAction DetectUpgradeAction(PackageDependency dependency, PackageIdentity recentPackage)
        {
            PackageUpgradeAction upgradeType;
            if (recentPackage == null)
            {
                upgradeType = PackageUpgradeAction.Unknown;
            }
            else if (dependency.VersionRange.Satisfies(recentPackage.Version))
            {
                upgradeType = IsMinVersionUpgradeable(dependency, recentPackage) ? PackageUpgradeAction.MinVersion : PackageUpgradeAction.None;
            }
            else
            {
                var fromRelease = DependsOnReleaseVersion(dependency);

                if (recentPackage.Version.IsPrerelease)
                    upgradeType = fromRelease ? PackageUpgradeAction.ReleaseToPrerelease : PackageUpgradeAction.PrereleaseToPrerelease;
                else
                    upgradeType = fromRelease ? PackageUpgradeAction.ReleaseToRelease : PackageUpgradeAction.PrereleaseToRelease;
            }

            return upgradeType;
        }

        private static bool IsMinVersionUpgradeable(PackageDependency dependency, PackageIdentity recentPackage)
            => (dependency.VersionRange.MinVersion != null) && (dependency.VersionRange.MinVersion < recentPackage.Version);

        private static bool DependsOnReleaseVersion(PackageDependency dependency)
            => (dependency.VersionRange.MaxVersion != null) &&
                !dependency.VersionRange.MaxVersion.IsPrerelease &&
                dependency.VersionRange.IsMaxInclusive;
    }
}
