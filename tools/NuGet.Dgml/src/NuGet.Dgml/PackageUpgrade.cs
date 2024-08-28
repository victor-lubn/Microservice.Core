using NuGet.Packaging.Core;

namespace NuGet
{
    /// <summary>
    /// Specifies an upgrade of a <see cref="PackageDependency"/>.
    /// </summary>
    public class PackageUpgrade
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PackageUpgrade"/> class.
        /// </summary>
        /// <param name="packageDependency">The package dependency affected by the upgrade.</param>
        /// <param name="action">The upgrade action of the package dependency.</param>
        /// <param name="package">The package that can be used to perform the upgrade action on the package dependency.</param>
        public PackageUpgrade(PackageDependency packageDependency, PackageUpgradeAction action, PackageIdentity package)
        {
            PackageDependency = packageDependency;
            Action = action;
            Package = package;
        }

        /// <summary>
        /// Gets the package dependency affected by the upgrade.
        /// </summary>
        public PackageDependency PackageDependency { get; }

        /// <summary>
        /// Gets the upgrade action of the package dependency.
        /// </summary>
        public PackageUpgradeAction Action { get; }

        /// <summary>
        /// Gets the package that can be used to perform the upgrade action on the package dependency.
        /// </summary>
        public PackageIdentity Package { get; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => $"{PackageDependency} {Action} -> {Package.Id} {Package.Version}";
    }
}
