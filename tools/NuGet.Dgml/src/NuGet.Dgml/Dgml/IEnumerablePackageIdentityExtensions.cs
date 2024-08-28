using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NuGet.Frameworks;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;

namespace NuGet.Dgml
{
    /// <summary>
    /// Provides static extension methods for <see cref="IEnumerable{PackageIdentity}"/>.
    /// </summary>
    public static class IEnumerablePackageIdentityExtensions
    {
        /// <summary>
        /// Visualizes the upgradeable dependencies of the specified packages.
        /// </summary>
        /// <param name="packages">The packages.</param>
        /// <param name="repository">The repository to resolve package dependencies.</param>
        /// <returns>The graph of the upgradeable dependencies of the specified packages.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="packages"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <c>null</c>.</exception>
        /// <seealso cref="VisualizeUpgradeableDependenciesAsync(IEnumerable{PackageIdentity}, SourceRepository, NuGetFramework)"/>
        public static Task<DirectedGraph> VisualizeUpgradeableDependenciesAsync(this IEnumerable<PackageIdentity> packages, SourceRepository repository)
            => VisualizeUpgradeableDependenciesAsync(packages, repository, NuGetFramework.AnyFramework);

        /// <summary>
        /// Visualizes the upgradeable dependencies of the specified packages.
        /// </summary>
        /// <param name="packages">The packages.</param>
        /// <param name="repository">The repository to resolve package dependencies.</param>
        /// <param name="targetFramework">The framework to find compatible package dependencies.</param>
        /// <returns>The graph of the upgradeable dependencies of the specified packages.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="packages"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <c>null</c>.</exception>
        /// <seealso cref="UpgradeWalker"/>
        /// <seealso cref="PackageUpgradeVisualizer"/>
        /// <remarks>
        /// The method estimates the impact of an upgrade action. The palette ranges from green to red indicating the risk of an upgrade action.
        /// <list type="table">
        /// <listheader>
        /// <term><see cref="PackageUpgradeAction"/></term>
        /// <term>Color</term>
        /// <term>Risk</term>
        /// </listheader>
        /// <item>
        /// <term><see cref="PackageUpgradeAction.None"/></term>
        /// <term>Black</term>
        /// <term>0</term>
        /// </item>
        /// <item>
        /// <term><see cref="PackageUpgradeAction.MinVersion"/></term>
        /// <term>ForestGreen</term>
        /// <term>1</term>
        /// </item>
        /// <item>
        /// <term><see cref="PackageUpgradeAction.ReleaseToRelease"/></term>
        /// <term>Goldenrod</term>
        /// <term>2</term>
        /// </item>
        /// <item>
        /// <term><see cref="PackageUpgradeAction.PrereleaseToRelease"/></term>
        /// <term>DarkOrange</term>
        /// <term>3</term>
        /// </item>
        /// <item>
        /// <term><see cref="PackageUpgradeAction.PrereleaseToPrerelease"/></term>
        /// <term>OrangeRed</term>
        /// <term>4</term>
        /// </item>
        /// <item>
        /// <term><see cref="PackageUpgradeAction.ReleaseToPrerelease"/></term>
        /// <term>Firebrick</term>
        /// <term>5</term>
        /// </item>
        /// <item>
        /// <term><see cref="PackageUpgradeAction.Unknown"/></term>
        /// <term>DarkGray</term>
        /// <term>-</term>
        /// </item>
        /// </list>
        /// </remarks>
        public static async Task<DirectedGraph> VisualizeUpgradeableDependenciesAsync(
            this IEnumerable<PackageIdentity> packages,
            SourceRepository repository,
            NuGetFramework targetFramework)
        {
            if (packages == null)
                throw new ArgumentNullException(nameof(packages));
            if (repository == null)
                throw new ArgumentNullException(nameof(repository));

            var walker = new UpgradeWalker(repository, targetFramework);

            var directedGraph = DirectedGraphFactory.CreateDependencyGraph();

            var packageUpgradeActionPalette = new PackageUpgradeActionPalette();
            packageUpgradeActionPalette[PackageUpgradeAction.None] = "Black";
            packageUpgradeActionPalette[PackageUpgradeAction.MinVersion] = "ForestGreen";
            packageUpgradeActionPalette[PackageUpgradeAction.ReleaseToRelease] = "Goldenrod";
            packageUpgradeActionPalette[PackageUpgradeAction.PrereleaseToRelease] = "DarkOrange";
            packageUpgradeActionPalette[PackageUpgradeAction.PrereleaseToPrerelease] = "OrangeRed";
            packageUpgradeActionPalette[PackageUpgradeAction.ReleaseToPrerelease] = "Firebrick";
            packageUpgradeActionPalette[PackageUpgradeAction.Unknown] = "DarkGray";

            var packageUpgradePalette = new PackageUpgradePalette(packageUpgradeActionPalette)
            {
                MissingPackageColor = "Red",
                PrereleaseColor = "Gainsboro",
            };

            var visualizer = new PackageUpgradeVisualizer(directedGraph, packageUpgradePalette);

            foreach (var recentPackage in packages)
            {
                var upgrades = await walker.GetPackageUpgradesAsync(recentPackage).ConfigureAwait(false);
                visualizer.Visualize(recentPackage, upgrades);
            }

            return directedGraph;
        }
    }
}
