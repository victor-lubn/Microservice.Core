using System;

namespace NuGet.Dgml
{
    /// <summary>
    /// Provides a color palette to visualize data of <see cref="PackageUpgrade"/>.
    /// </summary>
    public class PackageUpgradePalette
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PackageUpgradePalette"/> class.
        /// </summary>
        public PackageUpgradePalette()
            : this(new PackageUpgradeActionPalette())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageUpgradePalette"/> class.
        /// </summary>
        /// <param name="upgradeActionPalette">The palette to visualize a <see cref="PackageUpgradeAction"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="upgradeActionPalette"/> is <c>null</c>.</exception>
        public PackageUpgradePalette(PackageUpgradeActionPalette upgradeActionPalette)
            => UpgradeActionPalette = upgradeActionPalette ?? throw new ArgumentNullException(nameof(upgradeActionPalette));

        /// <summary>
        /// Gets or sets the color of a prerelease package.
        /// </summary>
        public string PrereleaseColor { get; set; }

        /// <summary>
        /// Gets or sets the color of an undiscoverable package that is referenced by a package dependency.
        /// </summary>
        public string MissingPackageColor { get; set; }

        /// <summary>
        /// Gets the palette to visualize a <see cref="PackageUpgradeAction"/>.
        /// </summary>
        public PackageUpgradeActionPalette UpgradeActionPalette { get; }
    }
}
